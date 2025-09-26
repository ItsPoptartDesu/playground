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
