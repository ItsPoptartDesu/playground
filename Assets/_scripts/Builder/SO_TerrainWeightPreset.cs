using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Map/Terrain Weight Preset")]
public class TerrainWeightPreset : ScriptableObject {
    [System.Serializable]
    public class WeightedTerrain {
        public TerrainExpression terrain;
        [Range(0f , 100f)]
        public float weight = 10f; // Relative chance
    }

    public List<WeightedTerrain> terrains = new List<WeightedTerrain>
    {
        new WeightedTerrain { terrain = TerrainExpression.GRASS_TILE, weight = 50 },
        new WeightedTerrain { terrain = TerrainExpression.DIRT_TILE, weight = 20 },
        new WeightedTerrain { terrain = TerrainExpression.MOUNTAIN_TILE, weight = 10 },
        new WeightedTerrain { terrain = TerrainExpression.WATER_TILE, weight = 8 },
        new WeightedTerrain { terrain = TerrainExpression.FOREST_TILE, weight = 12 }
    };

    // Precompute for fast runtime selection
    [HideInInspector] public float totalWeight;
    [HideInInspector] public List<float> cumulativeWeights = new List<float>();

    private void OnValidate() {
        RebuildCumulative();
    }

    public void RebuildCumulative() {
        totalWeight = 0f;
        cumulativeWeights.Clear();

        foreach (var wt in terrains) {
            totalWeight += wt.weight;
            cumulativeWeights.Add(totalWeight);
        }
    }

    public TerrainExpression GetRandomTerrain() {
        if (totalWeight <= 0f) return TerrainExpression.GRASS_TILE;

        float randomValue = Random.Range(0f , totalWeight);
        for (int i = 0; i < cumulativeWeights.Count; i++) {
            if (randomValue < cumulativeWeights[i])
                return terrains[i].terrain;
        }
        return terrains[^1].terrain; // Fallback
    }
}