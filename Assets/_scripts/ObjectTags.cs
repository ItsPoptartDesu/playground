using System;
using Unity.VisualScripting;
using UnityEngine;
namespace ObjectTag
{
    public enum ObjExpression
    {
        HEXTILE,
        HERO,
        UNDEFINED,
    }

    public class ObjectTags : MonoBehaviour
    {
        [Header("Base Expression")]
        public ObjExpression expr = ObjExpression.UNDEFINED;
        [Header("Optional Base Info")]
        [Space(2f)]
        [Header("Hex Tile")]
        public string ObjName;

        public Vector3 id;
        public void SetHexInfo(int x, int y, int z)
        {
            id = new Vector3(x, y, z);
            ObjName = $"HexTile:({id})";
            gameObject.name = ObjName;
        }
        public virtual string GetHexInfo()
        {
            string hasError = "";
            if (id == null || ObjName == string.Empty || expr == ObjExpression.UNDEFINED)
                hasError = $"THIS OBJECT HAS AN ISSUE{gameObject.name}";
            return $"{expr} - {ObjName} (ID: {id})" + hasError;
        }

    }
}

