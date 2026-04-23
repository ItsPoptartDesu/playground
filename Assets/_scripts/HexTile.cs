using ObjectTag;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.LightTransport.PostProcessing;
using UnityEngine.UI;
public enum TerrainExpression {
    DIRT_TILE,
    FOREST_TILE,
    GRASS_TILE,
    MOUNTAIN_TILE,
    SAND_TILE,
    WATER_TILE,
    TUNDRA_TILE,
    DEFAULT_TILE,
}
[Serializable]
public struct HexExpression {
    public TerrainExpression m_TerrainExpression;
    public Material m_Material;
}

public class HexTile : ObjectTags, ISelectable {
    public TextMesh DebugText;
    [Header("Selectable")]
    [SerializeField] private UnityEvent onSelectedEvent; // Inspector-hookable for highlights
    [SerializeField] private Renderer highlightRenderer; // e.g., outline material
    public bool IsSelectable => true; // Always selectable, or check distance/ownership
    public Vector3 WorldPosition => transform.position;

    [Space(2f)]
    [Header("HexTile")]
    [SerializeField] TerrainExpression myTerrainExpression = TerrainExpression.DEFAULT_TILE;
    public int height = 0; // Stacked tiles increase this
    public GameObject heldObject; // Unit or stacked tile (check type)
    public TileEffectData effectData; // Assign in Inspector/prefab
    public int q, r;
    [SerializeField] ParticleSystem onSelectParticles;
    [SerializeField] Transform UnitAttachPoint;
    [HideInInspector] public List<HexTile> CachedNeighbors; // Add this!

    public bool IsOccupied => heldObject != null && !IsStackable(heldObject); // Can't move to occupied
    private bool IsStackable(GameObject obj) => obj.GetComponent<HexTile>() != null; // Allow stacking tiles for height
    public TerrainExpression GetTerrainExpression() { return myTerrainExpression; }
    public bool IsWater() { return myTerrainExpression == TerrainExpression.WATER_TILE; }

    #region Selectable
    public void OnSelect(UnityEvent customEvent = null) {
        // Visual: Enable highlight
        Debug.Log($"{GetHexInfo()} : has been clicked on");
        if (highlightRenderer != null)
            onSelectParticles.Play();
        onSelectedEvent?.Invoke(); // e.g., Show move range via GridManager
        customEvent?.Invoke();
        var tiles = GameEntry.Instance.GetLevelBuilder().GetGridManager().GetNeighbors(this);
        foreach (var t in tiles) {
            //Debug.Log($"{t.GetHexInfo()}");
            t.onSelectParticles.Play();
            t.onSelectedEvent?.Invoke();
            customEvent?.Invoke();
        }
    }
    public void OnDeselect() {
        Debug.Log($"{GetHexInfo()} : has been deselected on");
        if (highlightRenderer != null)
            onSelectParticles.Stop();
    }
    #endregion
    public float GetMoveCost(UnitData mover , HexTile fromTile) {
        float baseCost = 1f; // Default hex distance
        if (effectData != null) {
            baseCost *= effectData.moveCostMultiplier; // Slow/speed
            if (effectData.blocksMovement) return Mathf.Infinity; // Impassable
            // One-way: Check if entering from allowed dir (extend with fromTile)
        }

        // Height cost: Delta height * climb penalty (unless flying)
        int heightDelta = height - fromTile.height;
        //TODO climbing
        //if (heightDelta > 0 && heightDelta > mover.CurrentClimbDistance)
        //    return Mathf.Infinity; // Too steep
        baseCost += heightDelta > 0 ? heightDelta * 0.5f : 0f; // Extra cost for climbing

        return baseCost;
    }
    public void ApplyEffects(Unit _unit) {
        if (effectData == null) return;
        Debug.Log($"{_unit.myUnitData.myName} was hit with {effectData.name}");
        return;
    }
    public void UpdateTerrainExpression(TerrainExpression _type) {
        myTerrainExpression = _type;
        GetComponentInChildren<MeshRenderer>().sharedMaterial = GameEntry.Instance.GetObjectManager().GetHexTileMaterial(_type);
    }
    public void Initialize(Vector3 pos , int x , int y , int z , TerrainExpression _t = TerrainExpression.GRASS_TILE) {
        transform.position = pos;
        transform.rotation = Quaternion.identity;
        SetHexInfo(x , y , z);
        q = x;
        r = z;
        CachedNeighbors = new List<HexTile>(6); // Pre-allocate
        var tempstring = $"Q:{x} @ R:{z}";
        DebugText.text = tempstring.Replace("@" , System.Environment.NewLine);
        //TODO check if LB stores mapsize if not paass it along when you start the game.
        LevelBuilder LB = GameEntry.Instance.GetLevelBuilder();
        var ms = GameEntry.Instance.MapSize;

        // Sample Perlin height at grid coordinates (x,z)
        float height01 = LB.SampleHeight(x , z);

        // Step 1: What does the player/editor want overall?
        TerrainExpression desired = LB.baseTerrainWeights.GetRandomTerrain();

        // Step 2: How well does this fit the local environment?
        float fitScore = CalculateFitScore(desired , height01);

        // Step 3: Accept with probability based on fit
        if (UnityEngine.Random.value < fitScore) {
            UpdateTerrainExpression(desired);
        } else {
            for (int attempt = 0; attempt < 10; attempt++) {
                TerrainExpression retry = LB.baseTerrainWeights.GetRandomTerrain();
                float retryFit = CalculateFitScore(retry , height01);
                if (UnityEngine.Random.value < retryFit) {
                    UpdateTerrainExpression(retry);
                    break;
                }
            }
        }
        GetHexInfo();
    }
    public override string GetHexInfo() {
        return $"{myTerrainExpression.ToString()} ----- {base.GetHexInfo()}";

    }
    /// <summary>
    /// Returns how well a given terrain type fits this tile's environment (0–1).
    /// 1.0 = perfect fit, 0.0 = very unlikely to be chosen.
    /// </summary>
    public float CalculateFitScore(TerrainExpression terrainType , float height01) {
        float score = 0.5f; // Default neutral chance (can be adjusted)

        switch (terrainType) {
            // WATER: Loves low elevation, hates high ground
            case TerrainExpression.WATER_TILE:
                if (height01 < 0.35f) score = 0.98f;           // Deep ocean = almost certain
                else if (height01 < 0.45f) score = 0.80f;      // Shallow coast = good
                else if (height01 > 0.60f) score = 0.10f;      // High ground = very unlikely
                else score = 0.40f;                            // Mid elevation = rare
                break;

            // MOUNTAIN: Loves high elevation
            case TerrainExpression.MOUNTAIN_TILE:
                if (height01 > 0.75f) score = 0.95f;           // Peaks = perfect
                else if (height01 > 0.65f) score = 0.75f;      // Hills = good
                else score = 0.15f;                            // Lowlands = very rare
                break;
            // GRASS / PLAINS: Safe fallback, good everywhere
            case TerrainExpression.GRASS_TILE:
            case TerrainExpression.DIRT_TILE:
                score = 0.65f; // Reasonably likely almost everywhere
                break;

            // Add more biomes here as needed (jungle, swamp, beach, etc.)
            default:
                score = 0.50f; // Neutral for unknown types
                break;
        }

        return Mathf.Clamp01(score); // Keep it in 0–1 range
    }
}
