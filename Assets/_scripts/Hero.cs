using ObjectTag;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public interface IMovable
{
    //move length inpact
    int MaxMoveDistanceAllowed { get; set; }
    // Max height delta
    int ClimbAbility { get; set; }
    // Optional: uint CanFly, CanSwim, etc. 
}
[System.Serializable]
public struct HeroStats
{
    public string HeroName;
    public int CurrentMoveDistance;
    public int MaxMoveDistanceAllowed;
    public int MaxClimbDistance;
    public int CurrentClimbDistance;
    public HeroStats(string _name , int _maxDistance , int _maxClimb)
    {
        HeroName = _name;
        CurrentMoveDistance = MaxMoveDistanceAllowed = _maxDistance;
        CurrentClimbDistance = MaxClimbDistance = _maxClimb;
    }
    public void TurnReset()
    {
        CurrentClimbDistance = MaxClimbDistance;
        CurrentMoveDistance = MaxMoveDistanceAllowed;
    }
}

public class Hero : ObjectTags, ISelectable
{
    public HeroStats myStats;
    // Actions: Called from SelectionManager.HandleActionClick() or UI buttons
    public void Attack(Hero target) { /* CombatSystem.Resolve() */ }
    public void ShowInfo() { /* UI popup with stats */ }
    public HexTile CurrentTile { get; set; }

    public bool IsSelectable => throw new System.NotImplementedException();

    public Vector3 WorldPosition => throw new System.NotImplementedException();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Initiate(string _name , int _moveD , int _climbD)
    {
        myStats = new HeroStats(_name , _moveD , _climbD);
    }
    public void EndTurn()
    {
        myStats.TurnReset();
    }

    public void MoveTo(HexTile target)
    {
        List<HexTile> path = GameEntry.Instance.GetGridManager().FindPath(CurrentTile , target , myStats);
        if (path == null || path.Count > myStats.MaxMoveDistanceAllowed + 1) return; // +1 for start tile

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
        myStats.CurrentMoveDistance -= path.Count - 1; // Deduct moves
    }

    public void OnSelect(UnityEvent onSelectedEvent = null)
    {
        Debug.Log(" ");
        Debug.Log(" ");
        Debug.Log($"{myStats.HeroName} is being Selected");
        Debug.Log($"{myStats}");
    }

    public void OnDeselect()
    {
        Debug.Log($"{myStats.HeroName} is being deselected");
    }
}
