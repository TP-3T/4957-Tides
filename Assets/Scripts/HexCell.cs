using System;
using UnityEngine;

public enum TileType
{
    grassland,
    water,
    forest,
    hill
}

public class HexCell : MonoBehaviour
{
    [NonSerialized]
    public Vector3 CenterPosition = Vector3.zero;          // You will set this in the code
    public TileType TileType;
    public float Height;
}
