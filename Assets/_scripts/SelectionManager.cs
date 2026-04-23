using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
public enum SelectionState
{
    Idle,           // Nothing selected—ready for hex click
    TileSelected,   // Hex picked; show move/attack highlights if unit present
    UnitSelected    // Unit on hex picked; show actions (move, attack, info)
}
public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance;
    [Header("Selection Manager")]
    [SerializeField] public uint moveSelected = 0;
    [SerializeField] private LayerMask tileLayerMask = 1 << 6; // e.g., Layer 6 = "HexTiles"
    [SerializeField] private LayerMask unitLayerMask = 1 << 7; // Layer 7 = "Units"
    [SerializeField] private Camera mainCamera; // Scene camera
    private Unit mySelectedUnit;
    private Faction myCurrentFaction;

    public SelectionState CurrentState { get; private set; } = SelectionState.Idle;
    public ISelectable CurrentSelection { get; private set; }
    public ISelectable[] CurrentSelections;
    // Events: Publishers for subscribers (UI, GameManager, etc.)
    public static event Action<SelectionState> OnStateChanged;
    public static event Action<ISelectable> OnSelectionChanged;
    public static event Action OnDeselected; 
    private List<UnitStateMachine> allUnitStates = new List<UnitStateMachine>();
    public void ToggleMove(uint _state)
    {
        moveSelected = _state;
    }
    //input map, map of maps
    private InputSystem_Actions playerInput;
    //individual action maps
    [Header("Player Actions")]
    public InputAction PTesting;
    public InputAction OTesting;

    #region unity functions
    private void Awake()
    {
        playerInput = new InputSystem_Actions();
        mainCamera ??= Camera.main;
    }
    public void OnEnable() {
        playerInput.Enable();
        OTesting = playerInput.Testing.BlowUp;
        OTesting.Enable();
        OTesting.performed += GameEntry.Instance.EndGame;

        PTesting = playerInput.Testing.BuildLevel;
        PTesting.Enable();
        PTesting.performed += GameEntry.Instance.StartGame;

        playerInput.Testing.Click.performed += OnClick;
        playerInput.Testing.Deselect.performed += _ => DeselectAll();
        CurrentSelections = new ISelectable[2];

        playerInput.Testing.Spawn.performed += SpawnPlayable;
    }
    public void OnDisable() {
        playerInput.Disable();
        playerInput.Testing.Click.performed -= OnClick;

        PTesting.Disable();
        PTesting.performed -= GameEntry.Instance.StartGame;
        PTesting = null;

        OTesting.Disable();
        OTesting.performed -= GameEntry.Instance.EndGame;
        OTesting = null;

        playerInput.Testing.Click.performed -= OnClick;
        playerInput.Testing.Deselect.performed -= _ => DeselectAll();
    }
    #endregion

    #region Selection
    public void RegisterUnit(UnitStateMachine stateMachine) {
        allUnitStates.Add(stateMachine);
    }
    private void SpawnPlayable(InputAction.CallbackContext context)
    {
        GameEntry.Instance.GetObjectManager().SpawnPlayableUnit(null);
    }
    private void OnClick(InputAction.CallbackContext context)
    {
        Debug.Log("PlayerController OnClick");
        //HandleGenericClick();
        HandleClick();
    }
    public void HandleGenericClick()
    {
        if (!Physics.Raycast(mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue()) , out RaycastHit hit , Mathf.Infinity , tileLayerMask))
            return; // Missed any tile

        HexTile hitTile = hit.collider.GetComponent<HexTile>();
        if (hitTile == null) return;
        // Check for unit on tile
        Collider unitCollider = hitTile.GetComponentInChildren<Collider>(); // Assumes unit is child or same hex
        ISelectable newSelection = unitCollider?.GetComponent<ISelectable>() ?? hitTile as ISelectable;

        //valid tile selection
        if (newSelection != null && newSelection.IsSelectable)
        {
            if (hitTile.heldObject != null)
            {
                Select(hitTile.heldObject.GetComponent<ISelectable>());
            }
            else
            {
                Select(newSelection);
            }
        }
    }
    private void HandleHexClick()
    {
        if (!Physics.Raycast(mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue()) , out RaycastHit hit , Mathf.Infinity , tileLayerMask))
            return; // Missed any tile

        HexTile hitTile = hit.collider.GetComponent<HexTile>();
        if (hitTile == null) return;

        // Check for unit on tile
        Collider unitCollider = hitTile.GetComponentInChildren<Collider>(); // Assumes unit is child or same hex
        ISelectable newSelection = unitCollider?.GetComponent<ISelectable>() ?? hitTile as ISelectable;

        if (newSelection != null && newSelection.IsSelectable)
        {
            Select(newSelection);
        }
    }

    private void HandleClick()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray);

        // Sort by layer priority: Units first
        hits = hits.OrderBy(h => h.collider.gameObject.layer).ToArray();

        foreach (RaycastHit hit in hits) {
            if (((1 << hit.collider.gameObject.layer) & unitLayerMask) != 0) {
                Unit unit = hit.collider.GetComponent<Unit>();
                if (unit && unit.myOwner == myCurrentFaction) { // Your faction only
                    SelectUnit(unit);
                    return; // Stop here - unit selected
                }
            }
        }

        // No unit hit → check ground
        foreach (RaycastHit hit in hits) {
            if (((1 << hit.collider.gameObject.layer) & tileLayerMask) != 0) {
                HexTile targetTile = hit.collider.GetComponent<HexTile>();
                if (targetTile && mySelectedUnit != null) {
                    //TryMoveOrAttack(targetTile);
                    return;
                }
            }
        }
    }
    public void SelectUnit(Unit unit) {
        if (mySelectedUnit == unit) {
            DeselectUnit();
            return;
        }

        // Deselect previous directly
        if (mySelectedUnit != null) {
            mySelectedUnit.GetComponent<UnitStateMachine>().SetState(UnitState.Friendly);
        }

        mySelectedUnit = unit;
        unit.GetComponent<UnitStateMachine>().SetState(UnitState.Selected);

        //ShowMovementRange(unit);
    }
    public void DeselectUnit() {
        if (mySelectedUnit != null) {
            var stateMachine = mySelectedUnit.GetComponent<UnitStateMachine>();
            stateMachine.SetState(
                mySelectedUnit.myOwner == myCurrentFaction
                    ? UnitState.Friendly
                    : UnitState.Hostile
            );
            mySelectedUnit = null;
        }
        //ClearRangeOverlays();
    }
    // Called by TurnManager when turn changes
    public void OnTurnChanged(Faction newFaction) {
        myCurrentFaction = newFaction;

        // ONLY refresh visuals — no events
        foreach (var stateMachine in allUnitStates) {
            Unit unit = stateMachine.GetComponent<Unit>();
            if (unit == mySelectedUnit) continue; // Selected overrides

            stateMachine.SetState(
                unit.myOwner == myCurrentFaction
                    ? UnitState.Friendly
                    : UnitState.Hostile
            );
        }

        // Deselect if selected unit no longer belongs to current player
        if (mySelectedUnit != null && mySelectedUnit.myOwner != myCurrentFaction) {
            DeselectUnit();
        }
    }
    private void Select(ISelectable selectable)
    {
        if (CurrentSelections[moveSelected] != null)
            CurrentSelections[moveSelected].OnDeselect();

        CurrentSelections[moveSelected] = selectable;
        selectable.OnSelect();

        // State transition
        CurrentState = selectable is HexTile ? SelectionState.TileSelected : SelectionState.UnitSelected;
        OnStateChanged?.Invoke(CurrentState);
        OnSelectionChanged?.Invoke(CurrentSelections[moveSelected]);
        moveSelected = 1;
    }

    public void DeselectAll()
    {
        for (int i = 0; i <= moveSelected; i++)
        {
            if (CurrentSelections[i] != null)
            {
                CurrentSelections[i].OnDeselect();
                CurrentSelections[i] = null;
            }
        }
        CurrentState = SelectionState.Idle;
        OnStateChanged?.Invoke(CurrentState);
        OnDeselected?.Invoke();
        moveSelected = 0;
    }
    #endregion
   
}
