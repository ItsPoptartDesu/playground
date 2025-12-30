using ObjectTag;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class ObjectManager : MonoBehaviour {
    [SerializeField] private GameObject _hexTile;
    private float hexSize = -1f;
    public float GetHexSize() { return hexSize; }
    [SerializeField] private Transform LevelParent;
    private Dictionary<ObjExpression , List<GameObject>> ObjectsInScene;
    [SerializeField] private List<HexExpression> HexExpressions;
    [SerializeField] private Material DEFAULT_Mat;
    [SerializeField] private GameObject TestHero;
    public Material GetDefaultMat() { return DEFAULT_Mat; }
    private void Awake() {
        ObjectsInScene = new Dictionary<ObjExpression , List<GameObject>>();
    }
    public int GetHexExpressionCount() {
        return HexExpressions.Count;
    }
    public Material GetHexTileMaterial(TerrainExpression _te) {
        foreach (var he in HexExpressions) {
            if (he.m_TerrainExpression == _te)
                return he.m_Material;
        }
        return GameEntry.Instance.GetObjectManager().GetDefaultMat();
    }
    public void SpawnPlayableUnit(string _name) {
        if (_name == null) {
            List<GameObject> Tiles = ObjectsInScene[ObjExpression.HEXTILE];
            int RandomTileIndex = UnityEngine.Random.Range(0 , Tiles.Count);
            GameObject TileParent = Tiles[RandomTileIndex];
            GameObject newlySpawned = Instantiate(TestHero , TileParent.transform);
            Tiles[RandomTileIndex].GetComponent<HexTile>().heldObject = newlySpawned;
            string name = RandomString(7);
            int moveD = UnityEngine.Random.Range(1 , 5);
            int climbD = UnityEngine.Random.Range(0 , 2);

            newlySpawned.GetComponent<Unit>().Initiate();
            if (!ObjectsInScene.ContainsKey(ObjExpression.UNIT))
                ObjectsInScene[ObjExpression.UNIT] = new List<GameObject> { newlySpawned };
            else
                ObjectsInScene[ObjExpression.UNIT].Add(newlySpawned);

            return;
        }
    }
    public string RandomString(int length) {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars , length)
            .Select(s => s[UnityEngine.Random.Range(0 , s.Length)]).ToArray());
    }
    public GameObject CreateNewHexTile() {
        if (_hexTile == null) {
            Debug.LogError("HexTilePrefab not assigned! Cannot build grid.");
            return null;
        }

        GameObject go = Instantiate(_hexTile , LevelParent);
        if (hexSize <= 0f)
            hexSize = CalculateHexSize(go);
        ObjectTags ot = go.GetComponent<ObjectTags>();
        if (!ObjectsInScene.ContainsKey(ot.expr))
            ObjectsInScene[ot.expr] = new List<GameObject>();

        ObjectsInScene[ot.expr].Add(go);
        return go;
    }
    public void ShutDown() {
        Debug.Log("Removing all Hex Tiles");
        foreach (GameObject obj in ObjectsInScene[ObjExpression.HEXTILE]) {
            Destroy(obj);
        }
        ObjectsInScene[ObjExpression.HEXTILE].Clear();
        hexSize = -1f;
    }

    private float CalculateHexSize(GameObject go) {
        MeshFilter meshFilter = go.GetComponentInChildren<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null) {
            Debug.LogError("HexTilePrefab missing MeshFilter or mesh! Falling back to default size.");
            return -1f;
        }

        // Get local bounds of the mesh
        Bounds bounds = meshFilter.sharedMesh.bounds;

        // Compute flat-to-flat width: full x-size multiplied by scale (handles scaled prefabs)
        float computedSize = bounds.size.x * go.transform.localScale.x;
        if (computedSize <= 0f) {
            Debug.LogError("Invalid hex size computed (0 or negative)! Falling back to default.");
            return -1f;
        }
        Debug.Log($"Size:{computedSize}");
        return computedSize;
    }
}
