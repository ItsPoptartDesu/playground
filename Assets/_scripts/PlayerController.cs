using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.CullingGroup;
public enum SelectionState
{
    Idle,           // Nothing selected—ready for hex click
    TileSelected,   // Hex picked; show move/attack highlights if unit present
    UnitSelected    // Unit on hex picked; show actions (move, attack, info)
}
public class PlayerController : MonoBehaviour
{
    [Header("Selection Manager")]
    [SerializeField] public uint moveSelected = 0;
    [SerializeField] private LayerMask tileLayerMask = 1 << 6; // e.g., Layer 6 = "HexTiles"
    [SerializeField] private LayerMask unitLayerMask = 1 << 7; // Layer 7 = "Units"
    [SerializeField] private Camera mainCamera; // Scene camera
    public SelectionState CurrentState { get; private set; } = SelectionState.Idle;
    public ISelectable CurrentSelection { get; private set; }
    public ISelectable[] CurrentSelections;
    // Events: Publishers for subscribers (UI, GameManager, etc.)
    public static event Action<SelectionState> OnStateChanged;
    public static event Action<ISelectable> OnSelectionChanged;
    public static event Action OnDeselected;
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

    private void Awake()
    {
        playerInput = new InputSystem_Actions();
        mainCamera ??= Camera.main;

    }
    private void OnDisable()
    {
        playerInput.Disable();
        playerInput.Testing.Click.performed -= OnClick;

        PTesting.Disable();
        PTesting.performed -= BuildLevel;
        PTesting = null;

        OTesting.Disable();
        OTesting.performed -= BlowUpLevel;
        OTesting = null;

        playerInput.Testing.Click.performed -= OnClick;
        playerInput.Testing.Deselect.performed -= _ => DeselectAll();
    }
    private void OnEnable()
    {
        playerInput.Enable();
        OTesting = playerInput.Testing.BlowUp;
        OTesting.Enable();
        OTesting.performed += BlowUpLevel;

        PTesting = playerInput.Testing.BuildLevel;
        PTesting.Enable();
        PTesting.performed += BuildLevel;
       
        playerInput.Testing.Click.performed += OnClick;
        playerInput.Testing.Deselect.performed += _ => DeselectAll();
        CurrentSelections = new ISelectable[2];
    }

    #region Selection
    private void OnClick(InputAction.CallbackContext context)
    {
        Debug.Log("PlayerController OnClick");
        if (CurrentState == SelectionState.Idle || CurrentState == SelectionState.TileSelected)
        {
            HandleHexClick();
        }
        else if (CurrentState == SelectionState.UnitSelected)
        {
            // For unit actions: Raycast for targets (e.g., enemy hex/unit)
            HandleActionClick();
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

    private void HandleActionClick()
    {
        // Example: If unit selected, raycast for move/attack targets
        // This would use highlights from prior selection (e.g., valid hexes)
        // For now, stub: Deselect on off-target click
        if (!Physics.Raycast(mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue()) , out RaycastHit hit , Mathf.Infinity , tileLayerMask | unitLayerMask))
        {
            DeselectAll();
            return;
        }

        // Logic for move/attack: e.g., if hit is valid target, invoke Unit.MoveTo(hitTile)
        // Broadcast event: OnActionPerformed(newSelection, hit.collider.GetComponent<ISelectable>());
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
            if (CurrentSelections[i] != null) {
                CurrentSelections[i].OnDeselect();
                CurrentSelections[i] = null;
            }
        }
        //if (CurrentSelections[moveSelected] != null)
        //{
        //    CurrentSelections[moveSelected].OnDeselect();
        //    CurrentSelections[moveSelected] = null;
        //}
        CurrentState = SelectionState.Idle;
        OnStateChanged?.Invoke(CurrentState);
        OnDeselected?.Invoke();
        moveSelected = 0;
    }
    #endregion
    private void BuildLevel(InputAction.CallbackContext context)
    {
        Debug.Log("Player Wants to build a level");
        GameEntry.Instance.Build();
    }
    private void BlowUpLevel(InputAction.CallbackContext context)
    {
        Debug.Log("Player Wants to Destroy the level");
        GameEntry.Instance.BlowUp();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //playerInput.ReadValue<>
    }
}
