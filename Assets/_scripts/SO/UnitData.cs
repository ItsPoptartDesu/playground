using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "UnitData" , menuName = "Scriptable Objects/UnitData")]
public class UnitData : ScriptableObject {
    public string myName;
    public GameObject myPrefab;
    public int myMovement;
    public int myAttack;
    public int myDefense;
    public int myHP;
    public int myAttackRange;
    public int myCostToPlay;
    public List<string> myAbilities;
}
