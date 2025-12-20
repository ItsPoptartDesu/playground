using UnityEngine;

[CreateAssetMenu(menuName = "Game/Feature Type")]
public class FeatureType : ScriptableObject {
    public string featureName; // "Forest", "Farm", "Wild Animal", "Fruit"
    public GameObject prefab;  // Tree model, farm building, etc.
    public int populationBonus = 0; // Polytopia-style
    public int resourceBonus = 0;
    public bool blocksVision = false;
}

