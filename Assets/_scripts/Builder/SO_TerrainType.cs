using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Terrain Type")]
public class TerrainType : ScriptableObject {
    public string typeName;
    public Color editorColor;     // For painting visualization
    public GameObject tilePrefab; // Visual model (grass mesh, water shader, etc.)
    public bool isWater = false;
    public int movementCost = 1;
    public float defenseBonus = 0f;
    public List<FeatureType> allowedFeatures; // e.g., grass allows forest/farm
}