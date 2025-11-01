using UnityEngine;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;

public class GridManager : MonoBehaviour
{
    private Dictionary<Vector2Int , HexTile> grid = new(); // Key: axial (q,r)
    private static readonly Vector2Int[] HexDirections = // Flat-top neighbors
    {
        new(1, 0), new(1, -1), new(0, -1),
        new(-1, 0), new(-1, 1), new(0, 1)
    };

    // ... Existing methods (GenerateBoard, etc.)

    public List<HexTile> FindPath(HexTile start , HexTile goal , HeroStats mover)
    {
        if (start == null || goal == null || goal.IsOccupied) return null;

        var openSet = new BinaryHeap<HexTile>();
        var cameFrom = new Dictionary<HexTile , HexTile>();
        var gScore = new Dictionary<HexTile , float> { [start] = 0f };
        var fScore = new Dictionary<HexTile , float> { [start] = Heuristic(start , goal) };

        openSet.Enqueue(start , fScore[start]);

        while (openSet.Count > 0)
        {
            HexTile current = openSet.Dequeue();

            if (current == goal)
                return ReconstructPath(cameFrom , current);

            foreach (var neighbor in GetNeighbors(current))
            {
                if (neighbor.IsOccupied || !IsValidMove(mover , current , neighbor)) continue;

                float tentativeG = gScore[current] + neighbor.GetMoveCost(mover , current);

                if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                {
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

    private float Heuristic(HexTile a , HexTile b)
    {
        // Cube distance for hex: Convert axial to cube
        int ax = a.q, ay = a.r, az = -a.q - a.r;
        int bx = b.q, by = b.r, bz = -b.q - b.r;
        return (Mathf.Abs(ax - bx) + Mathf.Abs(ay - by) + Mathf.Abs(az - bz)) / 2f;
    }

    private List<HexTile> ReconstructPath(Dictionary<HexTile , HexTile> cameFrom , HexTile current)
    {
        var path = new List<HexTile> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0 , current);
        }
        return path;
    }

    private List<HexTile> GetNeighbors(HexTile tile)
    {
        var neighbors = new List<HexTile>();
        foreach (var dir in HexDirections)
        {
            Vector2Int neighborPos = new(tile.q + dir.x , tile.r + dir.y);
            if (grid.TryGetValue(neighborPos , out HexTile neighbor))
                neighbors.Add(neighbor);
        }
        return neighbors;
    }

    private bool IsValidMove(HeroStats mover , HexTile from , HexTile to)
    {
        // Add one-way checks here if in TileEffectData
        return true; // Extend for unit-specific (e.g., flying)
    }
}