// File: HeightmapSettings.asset
using UnityEngine;

[CreateAssetMenu(menuName = "Map/Heightmap Settings")]
public class HeightmapSettings : ScriptableObject {
    [Header("Noise Parameters")]
    public float noiseScale = 0.08f;      // Bigger = smoother continents
    public int octaves = 4;
    public float persistence = 0.5f;
    public float lacunarity = 2f;

    [Header("Height Curve")]
    public AnimationCurve heightCurve = AnimationCurve.Linear(0 , 0 , 1 , 1);

    [Header("Elevation Levels")]
    public int maxElevation = 5;          // How many height steps (0 to 5)
    public float waterThreshold = 0.35f;  // Below this = water
    public float mountainThreshold = 0.75f;

    [Header("Map Type Controls")]
    public MapType mapType = MapType.Continents;
    [Range(0f , 1f)] public float wetness = 0.5f; // 0 = dry, 1 = wet
    public enum MapType { Continents, Pangea, Archipelago, Waterworld }

    // Optional seed for reproducible maps
    public int seed = 0;
}