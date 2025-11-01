using ObjectTag;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public interface IMovable
{
    //move length inpact
    int HeroRangeImpact { get; }
    // Max height delta
    float ClimbAbility { get; } 
    // Optional: bool CanFly, CanSwim, etc.
}
public struct HeroStats
{
    public string HeroName { get; private set; }
    public int HeroMoveRange { get; private set; }
    public int MoveSpeed { get; set; } // Heroscape-style
    public float ClimbAbility { get; }
}

public class Hero : ObjectTags
{
    public HeroStats myStats;
    // Actions: Called from SelectionManager.HandleActionClick() or UI buttons
    public void Attack(Hero target) { /* CombatSystem.Resolve() */ }
    public void ShowInfo() { /* UI popup with stats */ }
    public HexTile CurrentTile { get; set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }
    

    public void MoveTo(HexTile target)
    {
        List<HexTile> path = GameEntry.Instance.GetGridManager().FindPath(CurrentTile , target , myStats);
        if (path == null || path.Count > myStats.MoveSpeed + 1) return; // +1 for start tile

        // Animate along path (e.g., coroutine with Lerp)
        StartCoroutine(MoveAlongPath(path));
    }

    private IEnumerator MoveAlongPath(List<HexTile> path)
    {
        foreach (var tile in path.GetRange(1 , path.Count - 1)) // Skip start
        {
            transform.position = tile.transform.position + Vector3.up * 0.5f; // Above tile
            tile.ApplyEffects(this); // DoT/slow on enter
            yield return new WaitForSeconds(0.5f); // Animate
        }
        myStats.MoveSpeed -= path.Count - 1; // Deduct moves
    }
}
