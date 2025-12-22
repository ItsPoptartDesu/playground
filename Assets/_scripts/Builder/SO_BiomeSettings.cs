using UnityEngine;

[CreateAssetMenu(fileName = "SO_BiomeSettings", menuName = "Scriptable Objects/SO_BiomeSettings")]
public class SO_BiomeSettings : ScriptableObject
{
    [Header("Temperature Noise")]
    public float temperatureScale = 0.06f;
    public int temperatureOctaves = 4;
    public float temperaturePersistence = 0.5f;

    [Header("Moisture Noise")]
    public float moistureScale = 0.07f;
    public int moistureOctaves = 5;
    public float moisturePersistence = 0.5f;

    // Optional: Simple "equator" effect – hotter in map center
    public bool useLatitudeTemperature = true;
    public float equatorHeatBonus = 0.3f;
}
