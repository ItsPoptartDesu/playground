using UnityEngine;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;

public class GridManager : MonoBehaviour {
    private Dictionary<Vector2Int , HexTile> grid = new(); // Key: axial (q,r)
    private static readonly Vector2Int[] HexDirections = // Flat-top neighbors
    {
        new(1, 0), new(1, -1), new(0, -1),
        new(-1, 0), new(-1, 1), new(0, 1)
    };
    public Dictionary<Vector2Int , HexTile> GetMap() { return grid; }
    private float hexSize;

    public void GenerateMap(int _width , int _height) {
        ObjectManager OM = GameEntry.Instance.GetObjectManager();
        GameObject SpawnedHexTile = null;
        HexTile h;
        float y = 0f;
        var ms = GameEntry.Instance.MapSize;
        for (int i = 0; i < ms.x; i++) {
            for (int j = 0; j < ms.y; j++) {
                SpawnedHexTile = OM.CreateNewHexTile();
                h = SpawnedHexTile.GetComponent<HexTile>();
                hexSize = OM.GetHexSize();
                Vector3 pos = GetHexWorldPosition(i , y , j);
                h.Initialize(pos , i , (int)y , j);
                grid.Add(new Vector2Int(i , j) , h);
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
    public List<HexTile> FindPath(HexTile start , HexTile goal , HeroStats mover) {
        if (start == null || goal == null || goal.IsOccupied) return null;

        var openSet = new BinaryHeap<HexTile>();
        var cameFrom = new Dictionary<HexTile , HexTile>();
        var gScore = new Dictionary<HexTile , float> { [start] = 0f };
        var fScore = new Dictionary<HexTile , float> { [start] = Heuristic(start , goal) };

        openSet.Enqueue(start , fScore[start]);

        while (openSet.Count > 0) {
            HexTile current = openSet.Dequeue();

            if (current == goal)
                return ReconstructPath(cameFrom , current);

            foreach (var neighbor in GetNeighbors(current)) {
                if (neighbor.IsOccupied || !IsValidMove(mover , current , neighbor)) continue;

                float tentativeG = gScore[current] + neighbor.GetMoveCost(mover , current);

                if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor]) {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + Heuristic(neighbor , goal);

                    if (openSet.Contains(neighbor))
                        openSet.UpdatePriority(neighbor , fScore[neighbor]);
                    else
                        openSet.Enqueue(neighbor , fScore[neighbor]);
                }
            }
        }
        return null; // No path
    }

    private float Heuristic(HexTile a , HexTile b) {
        // Cube distance for hex: Convert axial to cube
        int ax = a.q, ay = a.r, az = -a.q - a.r;
        int bx = b.q, by = b.r, bz = -b.q - b.r;
        return (Mathf.Abs(ax - bx) + Mathf.Abs(ay - by) + Mathf.Abs(az - bz)) / 2f;
    }

    private List<HexTile> ReconstructPath(Dictionary<HexTile , HexTile> cameFrom , HexTile current) {
        var path = new List<HexTile> { current };
        while (cameFrom.ContainsKey(current)) {
            current = cameFrom[current];
            path.Insert(0 , current);
        }
        return path;
    }
    //TODO save and store neighbors affter map build
    public List<HexTile> GetNeighbors(HexTile tile) {
        var neighbors = new List<HexTile>();
        foreach (var dir in HexDirections) {
            Vector2Int neighborPos = new(tile.q + dir.x , tile.r + dir.y);
            if (grid.TryGetValue(neighborPos , out HexTile neighbor))
                neighbors.Add(neighbor);
        }
        return neighbors;
    }
    public void ShutDown() {
        Debug.Log("GridManager Shutting down");
        grid.Clear();
    }
    private bool IsValidMove(HeroStats mover , HexTile from , HexTile to) {
        // Add one-way checks here if in TileEffectData
        return true; // Extend for unit-specific (e.g., flying)
    }
}