using UnityEngine;

[CreateAssetMenu(fileName = "TileEffect" , menuName = "Heroscape/TileEffect")]
public class TileEffectData : ScriptableObject
{
    public float moveCostMultiplier = 1f; // e.g., Water: 2f (slow), Road: 0.5f (speed)
    public int damagePerTurn = 0; // e.g., Lava: 1
    public bool isOneWay = false; // Direction-specific? Extend with Vector2Int fromDir if needed
    public bool blocksMovement = false; // e.g., Walls
    // Future: Buffs (e.g., healPerTurn), visuals (particles)
}