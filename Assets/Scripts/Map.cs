using System;
using System.Collections.Generic;
using System.Numerics;

public enum TileType
{
    grassland,
    water,
    forest,
    hill
}

[Serializable]
public class MapTilePosition
{
    public float x;
    public float y;
    public float z;

    public Vector3 ToVector3()
    {
        return new (x, y, z);
    }
}

[Serializable]
public class MapTileData
{
    public TileType TileType;
    public MapTilePosition TilePosition;
}

[Serializable]
public class MapData
{
    public string Name;
    public int Width;
    public int Height;
    public List<MapTileData> MapTilesData;
}
