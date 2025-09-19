using UnityEditor.PackageManager;
using UnityEditor.SceneManagement;
using UnityEngine;

public class HexGrid : MonoBehaviour
{
    [SerializeField] public bool DrawGizmos;
    [SerializeField] public HexOrientation HexOrientation;
    [SerializeField] public float HexSize;
    [SerializeField] public HexCell HexCell;
    [SerializeField] public TextAsset MapSource;

    private int Width, Height;
    private MapData gameMap;
    private HexCell[] hexCells;
    private HexMesh hexMesh;

    void Awake()
    {
        hexMesh = GetComponentInChildren<HexMesh>();

        ClearMap();
        BuildMap();
    }

    void OnDrawGizmos()
    {
        if (!DrawGizmos)
            return;

        for (int z = 0; z < Height; z++)
        {
            for (int x = 0; x < Width; x++)
            {
                Vector3 hexCenter = HexMath.GetHexCenter(HexSize, x, z, HexOrientation) + transform.position;
                Vector3[] hexCorners = HexMath.GetHexCorners(HexSize, HexOrientation);
                for (int s = 0; s < hexCorners.Length; s++)
                {
                    Gizmos.DrawLine(
                        hexCenter + hexCorners[s % 6],
                        hexCenter + hexCorners[(s + 1) % 6]
                    );
                }
            }
        }
    }

    void LoadMapTilesData()
    {
        gameMap = JsonUtility.FromJson<MapData>(MapSource.text);

        Debug.Log(@$"
        {gameMap.Name},
        {gameMap.Width}, {gameMap.Height}
        {gameMap.MapTilesData.Count}
        {gameMap.MapTilesData[0].TileType}
        {gameMap.MapTilesData[0].TilePosition.x}
        {gameMap.MapTilesData[0].TilePosition.y}
        {gameMap.MapTilesData[0].TilePosition.z}
        ");
    }

    public void BuildMap()
    {
        if (hexMesh == null)
        {
            Debug.LogError("Hex mesh is null");
            return;
        }

        LoadMapTilesData();

        hexCells = new HexCell[gameMap.Width * gameMap.Height];

        int i = 0;

        foreach (var mapTileData in gameMap.MapTilesData)
        {
            Vector3 hexCenter = HexMath.GetHexCenter(
                HexSize,
                Mathf.FloorToInt(mapTileData.TilePosition.x),
                Mathf.FloorToInt(mapTileData.TilePosition.y),
                Mathf.FloorToInt(mapTileData.TilePosition.z),
                HexOrientation
            ) + transform.position;

            HexCell hexCell = Instantiate(HexCell, hexCenter, Quaternion.identity, this.transform);

            hexCell.CenterPosition = hexCenter;
            hexCell.MapTileData = mapTileData;

            hexCells[i++] = hexCell;
        }

        hexMesh.Triangulate(hexCells, HexSize, HexOrientation);
    }

    public void ClearMap()
    {
        if (hexMesh == null)
            return;

        hexMesh.ClearMesh();

        if (hexCells == null || hexCells.Length == 0)
            return;

        foreach (var hexCell in hexCells)
        {
            if (hexCell == null)
                continue;

            if (Application.isEditor && !Application.isPlaying)
            {
                DestroyImmediate(hexCell.gameObject);
            }
            else
            {
                Destroy(hexCell.gameObject);
            }
        }
    }
}
