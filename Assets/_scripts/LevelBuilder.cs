using ObjectTag;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static HeightmapSettings;

public class LevelBuilder : MonoBehaviour {
    public TerrainWeightPreset baseTerrainWeights;
    public HeightmapSettings perlinHeightMapSettings;
    public SO_BiomeSettings biomeSettings;
    private System.Random pseudoRandom;
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
    public void DecorateMap(int _w , int _h) {
        SmoothCoastlines();
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
        //float normalized = Mathf.InverseLerp(0f , 2f , noiseHeight); // depends on octaves/persistence
        //return perlinHeightMapSettings.heightCurve.Evaluate(normalized);

        float rawNoise = Mathf.InverseLerp(0f , 2f , noiseHeight); // Your existing fBm Perlin code (0-1)
        float shaped = rawNoise;
        // Dynamic water threshold
        float waterThreshold = perlinHeightMapSettings.waterThreshold * perlinHeightMapSettings.wetness;

        if (perlinHeightMapSettings.mapType == MapType.Pangea || perlinHeightMapSettings.mapType == MapType.Continents) {
            // Radial falloff: land in center
            var ms = GameEntry.Instance.MapSize;
            float centerX = (float)x / ms.x - 0.5f;
            float centerZ = (float)z / ms.y - 0.5f;
            float distFromCenter = Mathf.Sqrt(centerX * centerX + centerZ * centerZ) * 2f; // 0-1
            float falloff = 1f - distFromCenter;
            shaped = Mathf.Lerp(rawNoise , falloff , waterThreshold);
        }

        

        // In GetTerrainFromHeight or biome: use waterThreshold
        return shaped;
    }
    public void SmoothCoastlines(int iterations = 4) {
        for (int i = 0; i < iterations; i++) {
            Dictionary<HexTile , int> waterNeighborCounts = new();
            var allTiles = GameEntry.Instance.GetGridManager().GetMap();
            foreach (HexTile tile in allTiles.Values) {
                if (!tile.IsWater()) continue;
                var n = GameEntry.Instance.GetGridManager().GetNeighbors(tile);
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
    public int GetElevationFromHeight(float height01) {
        float t = height01;
        int elevation = Mathf.FloorToInt(t * perlinHeightMapSettings.maxElevation);
        return Mathf.Clamp(elevation , 0 , perlinHeightMapSettings.maxElevation);
    }

    public TerrainExpression GetTerrainFromHeight(float height01) {
        if (height01 < perlinHeightMapSettings.waterThreshold)
            return TerrainExpression.WATER_TILE;

        if (height01 > perlinHeightMapSettings.mountainThreshold)
            return TerrainExpression.MOUNTAIN_TILE;

        // Mid range: use weighted choice for grass/dirt/forest etc.
        return baseTerrainWeights?.GetRandomTerrain() ?? TerrainExpression.GRASS_TILE;
    }
    public float SampleTemperature(int x , int z) {
        float temp = SampleNoise(x , z , biomeSettings.temperatureScale ,
                                 biomeSettings.temperatureOctaves ,
                                 biomeSettings.temperaturePersistence);

        if (biomeSettings.useLatitudeTemperature) {
            // Hotter near vertical center (equator)
            float latitude = Mathf.Abs((float)z / GameEntry.Instance.MapSize.y - 0.5f) * 2f; // 0 at center, 1 at poles
            temp -= latitude * biomeSettings.equatorHeatBonus;
        }

        return Mathf.Clamp01(temp); // 0 = cold, 1 = hot
    }

    public float SampleMoisture(int x , int z) {
        return SampleNoise(x , z , biomeSettings.moistureScale ,
                           biomeSettings.moistureOctaves ,
                           biomeSettings.moisturePersistence);
    }
    // Reuse your existing SampleHeight logic but make a generic one
    private float SampleNoise(int x , int z , float scale , int octaves , float persistence) {
        // Your existing fBm code here, same as SampleHeight but without the curve yet
        // Return 0-1 value
        if (biomeSettings == null) return 0f;

        float sampleX = x * scale;
        float sampleZ = z * scale;

        float amplitude = 1f;
        float frequency = 1f;
        float noiseHeight = 0f;

        for (int i = 0; i < octaves; i++) {
            float offsetX = pseudoRandom?.Next(-100000 , 100000) ?? 0;
            float offsetZ = pseudoRandom?.Next(-100000 , 100000) ?? 0;

            float perlinValue = Mathf.PerlinNoise(sampleX * frequency + offsetX , sampleZ * frequency + offsetZ);
            noiseHeight += perlinValue * amplitude;

            amplitude *= persistence;
            //frequency *= biomeSettings.lacunarity;
        }

        // Normalize to 0-1 range roughly
        float normalized = Mathf.InverseLerp(0f , 2f , noiseHeight); // depends on octaves/persistence
        return normalized;
    }
    public TerrainExpression GetBiomeTerrain(float height01 , float temperature01 , float moisture01) {
        // First: Water or mountain (unchanged)
        if (height01 < perlinHeightMapSettings.waterThreshold)
            return TerrainExpression.WATER_TILE;
        if (height01 > perlinHeightMapSettings.mountainThreshold) {
            // High mountains: Snow regardless of temp/moisture
            return temperature01 < 0.4f ? TerrainExpression.MOUNTAIN_TILE : TerrainExpression.MOUNTAIN_TILE;
        }
        //return TerrainExpression.GRASS_TILE;
        // Land biomes
        if (temperature01 > 0.7f) // Hot
        {
            if (moisture01 > 0.6f)
                return TerrainExpression.FOREST_TILE;
            if (moisture01 > 0.3f)
                return TerrainExpression.SAND_TILE;
            else
                return TerrainExpression.GRASS_TILE;
        } else if (temperature01 < 0.4f) // Cold
          {
            if (moisture01 > 0.5f)
                return TerrainExpression.FOREST_TILE; // Snowy forest
            else
                return TerrainExpression.TUNDRA_TILE;
        }
        if (moisture01 > 0.6f)
            return TerrainExpression.FOREST_TILE;
        if (moisture01 > 0.3f)
            return TerrainExpression.GRASS_TILE;
        else
            return TerrainExpression.SAND_TILE; // Or dry grass
    }
}
