using System;
using System.Collections.Generic;

[Serializable]
public class MapTileData
{
    public string TileTypeUid;
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
