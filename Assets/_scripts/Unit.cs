using ObjectTag;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class Unit : ObjectTags, ISelectable
{
    public UnitData myUnitData;
    public int myCurrentHp;
    public HexTile myTile;
    public int myCurrentMovement;
    public Faction myOwner;
    public bool IsSelectable => throw new System.NotImplementedException();

    public Vector3 WorldPosition => throw new System.NotImplementedException();

    public void OnDeselect() {
        Debug.Log($"{myUnitData.myName} is being deselected");
    }

    public void OnSelect(UnityEvent onSelectedEvent = null) {
        Debug.Log(" ");
        Debug.Log(" ");
        Debug.Log($"{myUnitData.myName} is being Selected");
        Debug.Log($"{myUnitData}");
    }
    public void MoveTo(HexTile target) {
        List<HexTile> path = GameEntry.Instance.GetLevelBuilder().GetGridManager().FindPath(myTile , target , myUnitData);
        if (path == null || path.Count > myUnitData.myMovement + 1) return; // +1 for start tile

        // Animate along path (e.g., coroutine with Lerp)
        StartCoroutine(MoveAlongPath(path));
    }

    private IEnumerator MoveAlongPath(List<HexTile> path) {
        foreach (var tile in path.GetRange(1 , path.Count - 1)) // Skip start
        {
            transform.position = tile.transform.position + Vector3.up * 0.5f; // Above tile
            tile.ApplyEffects(this); // DoT/slow on enter
            yield return new WaitForSeconds(0.5f); // Animate
        }
        myCurrentMovement -= path.Count - 1; // Deduct moves
    }
    public void Initiate(Faction _owner) {
        myCurrentHp = myUnitData.myHP;
        myCurrentMovement = myUnitData.myMovement;
        this.name = myUnitData.myName;
        myOwner = _owner;
    }
}
