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
    public int x;
    public int y;
    public int z;

    public Vector3 ToVector3()
    {
        return new(x, y, z);
    }

    public override string ToString()
    {
        return $"({x},{y},{z})";
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
