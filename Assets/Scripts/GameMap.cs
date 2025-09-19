using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MapDataTile
{
    public TileType TileType;
    public float Height;
}

[Serializable]
public class MapData
{
    public string Name;
    public int Width;
    public int Height;
    public List<List<MapDataTile>> HexTiles;
}
