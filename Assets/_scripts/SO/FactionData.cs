using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(menuName = "Game/Faction Data")]
public class FactionData : ScriptableObject {
    [Header("Identity")]
    public string factionName = "Elyrion"; // Polytopia tribe name
    public Color factionColor = Color.blue;
    public Sprite emblemIcon; // UI portrait
    public GameObject startingUnitPrefab; // e.g., Warrior

    [Header("Starting Bonuses")]
    public int startingStars = 10;
    public List<TerrainExpression> preferredStartingTerrains = new() { TerrainExpression.GRASS_TILE , TerrainExpression.FOREST_TILE };
    public List<string> startingTechs = new() { "Organization" }; // Tech node names

    [Header("Unique Traits")]
    public float unitProductionSpeedMultiplier = 1f;
    public Dictionary<TerrainExpression , float> terrainYieldBonus = new(); // e.g., Imperius: +50% fruit on grass
}