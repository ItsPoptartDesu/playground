using ObjectTag;
using UnityEngine;

public class LevelBuilder : MonoBehaviour
{
    private int width, height;
    private float hexSize;
    public void Build(int _w , int _h)
    {
        ObjectManager OM = GameEntry.Instance.GetObjectManager();
        width = _w;
        height = _h;

        GameObject SpawnedHexTile = null;
        HexTile h;
        float y = 0f;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                SpawnedHexTile = OM.CreateNewHexTile();
                h = SpawnedHexTile.GetComponent<HexTile>();
                hexSize = OM.GetHexSize();
                Vector3 pos = GetHexWorldPosition(i , y , j);
                h.Initialize(pos , i , (int)y , j);
            }
        }
    }
    private Vector3 GetHexWorldPosition(int x , float y , int z)
    {
        float newX = x * hexSize;
        if (z % 2 == 1)
        {
            // Offset every other row by half the hex width for proper alignment.
            newX += hexSize / 2f;
        }
        // Vertical spacing based on hex geometry.
        float newZ = z * (hexSize * Mathf.Sqrt(3f) / 2f);
        return new Vector3(newX , y , newZ);
    }

}
