using ObjectTag;
using System;
using UnityEngine;
using UnityEngine.Events;
public enum TerrainExpression
{
    GRASS_TILE,
    WATER_TILE,
    SAND_TILE,
    LAVA_TILE,
    DEFAULT_TILE,
}
[Serializable]
public struct HexExpression
{
    public TerrainExpression m_TerrainExpression;
    public Material m_Material;
}

public class HexTile : ObjectTags, ISelectable
{
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
    public bool IsOccupied => heldObject != null && !IsStackable(heldObject); // Can't move to occupied
    private bool IsStackable(GameObject obj) => obj.GetComponent<HexTile>() != null; // Allow stacking tiles for height
    public int q, r;
    [SerializeField] ParticleSystem onSelectParticles;
    [SerializeField] Transform UnitAttachPoint;
    #region Selectable
    public void OnSelect(UnityEvent customEvent = null)
    {
        // Visual: Enable highlight
        Debug.Log($"{GetHexInfo()} : has been clicked on");
        if (highlightRenderer != null)
            onSelectParticles.Play();
        onSelectedEvent?.Invoke(); // e.g., Show move range via GridManager
        customEvent?.Invoke();
    }

    public void OnDeselect()
    {
        Debug.Log($"{GetHexInfo()} : has been deselected on");
        if (highlightRenderer != null)
            onSelectParticles.Stop();
    }
    #endregion

    public float GetMoveCost(HeroStats mover , HexTile fromTile)
    {
        float baseCost = 1f; // Default hex distance
        if (effectData != null)
        {
            baseCost *= effectData.moveCostMultiplier; // Slow/speed
            if (effectData.blocksMovement) return Mathf.Infinity; // Impassable
            // One-way: Check if entering from allowed dir (extend with fromTile)
        }

        // Height cost: Delta height * climb penalty (unless flying)
        int heightDelta = height - fromTile.height;
        if (heightDelta > 0 && heightDelta > mover.CurrentClimbDistance)
            return Mathf.Infinity; // Too steep
        baseCost += heightDelta > 0 ? heightDelta * 0.5f : 0f; // Extra cost for climbing

        return baseCost;
    }

    public void ApplyEffects(Hero unit)
    {
        if (effectData == null) return;
        //if (effectData.damagePerTurn > 0)
        //    unit.TakeDamage(effectData.damagePerTurn); // DoT
        // Future: Slow (reduce unit speed temp), etc.
    }
    public TerrainExpression GetTerrainExpression() { return myTerrainExpression; }
    public void UpdateTerrainExpression(TerrainExpression _type)
    {
        myTerrainExpression = _type;
        GetComponentInChildren<MeshRenderer>().sharedMaterial = GameEntry.Instance.GetObjectManager().GetHexTileMaterial(_type);
    }
    public void Initialize(Vector3 pos , int x , int y , int z , TerrainExpression _t = TerrainExpression.GRASS_TILE)
    {
        transform.position = pos;
        transform.rotation = Quaternion.identity;
        SetHexInfo(x , y , z);
        int tileSize = GameEntry.Instance.GetObjectManager().GetHexExpressionCount();
        _t = (TerrainExpression)UnityEngine.Random.Range(0 , tileSize);
        UpdateTerrainExpression(_t);
        GetHexInfo();
    }
    public override string GetHexInfo()
    {
        return $"{myTerrainExpression.ToString()} -----{base.GetHexInfo()}";

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
