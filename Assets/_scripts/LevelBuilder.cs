using ObjectTag;
using UnityEngine;

public class LevelBuilder : MonoBehaviour {
    private int width, height;
    private float hexSize;
    public TerrainWeightPreset baseTerrainWeights;
    public HeightmapSettings perlinHeightMapSettings;
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
    public void Build(int _w , int _h) {
        ObjectManager OM = GameEntry.Instance.GetObjectManager();
        width = _w;
        height = _h;

        GameObject SpawnedHexTile = null;
        HexTile h;
        float y = 0f;
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                SpawnedHexTile = OM.CreateNewHexTile();
                h = SpawnedHexTile.GetComponent<HexTile>();
                hexSize = OM.GetHexSize();
                Vector3 pos = GetHexWorldPosition(i , y , j);
                h.Initialize(pos , i , (int)y , j);
            }
        }
    }
    private Vector3 GetHexWorldPosition(int x , float y , int z) {
        float newX = x * hexSize;
        if (z % 2 == 1) {
            // Offset every other row by half the hex width for proper alignment.
            newX += hexSize / 2f;
        }
        // Vertical spacing based on hex geometry.
        float newZ = z * (hexSize * Mathf.Sqrt(3f) / 2f);
        return new Vector3(newX , y , newZ);
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
}
