using ObjectTag;
using UnityEngine;
using UnityEngine.Events;

public class Hero : ObjectTags
{
    public string HeroName { get; private set; }
    public int HeroMoveRange { get; private set; }
    // Actions: Called from SelectionManager.HandleActionClick() or UI buttons
    public void MoveTo(HexTile target) { /* Pathfind & animate */ }
    public void Attack(Hero target) { /* CombatSystem.Resolve() */ }
    public void ShowInfo() { /* UI popup with stats */ }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HeroMoveRange = 3;
        HeroName = "Axe";
    }

    // Update is called once per frame
    void Update()
    {

    }
}
