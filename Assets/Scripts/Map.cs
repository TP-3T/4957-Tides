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
public class MapTileData
{
    public TileType TileType;
    public int Height;
    public OffsetCoordinates OffsetCoordinates;
}

[Serializable]
public class MapData
{
    public string Name;
    public int Width;
    public int Height;
    public List<MapTileData> MapTilesData;
}
