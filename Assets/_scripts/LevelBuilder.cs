using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LevelBuilder : MonoBehaviour {
    public TerrainWeightPreset baseTerrainWeights;
    public HeightmapSettings perlinHeightMapSettings;
    private System.Random pseudoRandom;
    [SerializeField] private GridManager myGridManager;
    public GridManager GetGridManager() { return myGridManager; }
    public float GetElevationStepHeight() => 1f; // or expose as public field
    void Awake() {
        if (baseTerrainWeights != null)
            baseTerrainWeights.RebuildCumulative();
        if (perlinHeightMapSettings != null && perlinHeightMapSettings.seed != 0) {
            pseudoRandom = new System.Random(perlinHeightMapSettings.seed);
        }
    }

    public TerrainExpression GetWeightedRandomTerrain() {
        return baseTerrainWeights != null
            ? baseTerrainWeights.GetRandomTerrain()
            : TerrainExpression.GRASS_TILE;
    }
    public void BuildMap(int _w , int _h) {
        myGridManager.GenerateMap(_w , _h);
        DecorateMap(_w , _h);
    }
    public void ShutdownMap() {
        myGridManager.ShutDown();
    }
    private void DecorateMap(int _w , int _h) {
        //SmoothCoastlines();
        //generate Rivers
        //add towns
        //add map resources
    }

    // We'll use this in HexTile initialization
    public float SampleHeight(int x , int z) {
        if (perlinHeightMapSettings == null) return 0f;

        float sampleX = x * perlinHeightMapSettings.noiseScale;
        float sampleZ = z * perlinHeightMapSettings.noiseScale;

        float amplitude = 1f;
        float frequency = 1f;
        float noiseHeight = 0f;

        for (int i = 0; i < perlinHeightMapSettings.octaves; i++) {
            float offsetX = pseudoRandom?.Next(-100000 , 100000) ?? 0;
            float offsetZ = pseudoRandom?.Next(-100000 , 100000) ?? 0;

            float perlinValue = Mathf.PerlinNoise(sampleX * frequency + offsetX , sampleZ * frequency + offsetZ);
            noiseHeight += perlinValue * amplitude;

            amplitude *= perlinHeightMapSettings.persistence;
            frequency *= perlinHeightMapSettings.lacunarity;
        }

        // Normalize to 0-1 range roughly
        float normalized = Mathf.InverseLerp(0f , 2f , noiseHeight); // depends on octaves/persistence
        return perlinHeightMapSettings.heightCurve.Evaluate(normalized);
    }
    public void SmoothCoastlines(int iterations = 4) {
        for (int i = 0; i < iterations; i++) {
            Dictionary<HexTile , int> waterNeighborCounts = new();
            var allTiles = GameEntry.Instance.GetLevelBuilder().GetGridManager().GetMap();
            foreach (HexTile tile in allTiles.Values) {
                if (!tile.IsWater()) continue;
                var n = GameEntry.Instance.GetLevelBuilder().GetGridManager().GetNeighbors(tile);
                foreach (HexTile neighbor in n) {
                    if (waterNeighborCounts.ContainsKey(neighbor))
                        waterNeighborCounts[neighbor]++;
                    else
                        waterNeighborCounts[neighbor] = 1;
                }
            }
            foreach (var kvp in waterNeighborCounts) {
                HexTile tile = kvp.Key;
                int count = kvp.Value;
                if (tile.IsWater() && count < 3) tile.UpdateTerrainExpression(TerrainExpression.GRASS_TILE); // Fill tiny lakes
                else if (!tile.IsWater() && count > 4) tile.UpdateTerrainExpression(TerrainExpression.WATER_TILE); // Expand seas
            }
        }
    }
}
