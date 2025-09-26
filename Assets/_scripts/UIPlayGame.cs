using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class UIPlayGame : MonoBehaviour
{
    [SerializeField] private Transform container;
    // Shows hex info
    [SerializeField] private GameObject infoPanel; 
    [SerializeField] private GameObject unitPanel; 
    // Buttons: Move, Attack, Info
    [SerializeField] private Button moveButton, attackButton, infoButton;
    public void AddEventMove(UnityAction _action)
    {
        moveButton.onClick.AddListener(_action);
    }
    public void ToggleStart(bool _start)
    {
        if (_start)
        {
        }
        else
        {
            TurnAllChildrenOff();
        }
    }
    private void TurnAllChildrenOff()
    {
        int count = container.childCount;
        for (int i = 0; i < count; i++)
        {
            container.GetChild(i).gameObject.SetActive(false);
        }
    }
    private void OnEnable()
    {
        PlayerController.OnStateChanged += UpdateUIForState;
        PlayerController.OnSelectionChanged += UpdateUIForSelection;
        PlayerController.OnDeselected += HideAllPanels;
        //infoButton.onClick.AddListener(infoButtonOnClick);
        PlayerController pc = FindAnyObjectByType<PlayerController>();
        
    }
    private void infoButtonOnClick()
    {
        infoPanel.SetActive(true);
    }

    private void OnDisable()
    {
        PlayerController.OnStateChanged -= UpdateUIForState;
        // ... unsubscribe others
    }

    private void UpdateUIForState(SelectionState state)
    {
        infoPanel.SetActive(state == SelectionState.TileSelected);
        //unitPanel.SetActive(state == SelectionState.UnitSelected);
        // Hide others if needed
    }

    private void UpdateUIForSelection(ISelectable selection)
    {
        if (selection is HexTile tile)
        {
            // Update tilePanel text: "Hex (q,r): Terrain: Grass"
            TextMeshProUGUI displayTileText = infoPanel.GetComponentInChildren<TextMeshProUGUI>();
            displayTileText.text = tile.GetHexInfo();
            OneHexSelected();
        }
        //else if (selection is Unit unit)
        //{
        //    // Update unitPanel: Stats, bind buttons to unit.MoveTo(), etc.
        //    moveButton.onClick.RemoveAllListeners();
        //    moveButton.onClick.AddListener(() => { /* Prompt target selection */ });
        //}
    }
    private void OneHexSelected()
    {
        Debug.Log("MOVE BUTTON ENABLED");
        moveButton.gameObject.SetActive(true);
    }
    private void HideAllPanels()
    {
        infoPanel.SetActive(false);
        //unitPanel.SetActive(false);
        moveButton.gameObject.SetActive(false);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
