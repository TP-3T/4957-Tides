using Unity.Collections;
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

    public HexCell[] HexCells;
    public MapData GameMapData;

    private HexMesh hexMesh;

    void Awake()
    {
        if (hexMesh == null)
            hexMesh = GetComponentInChildren<HexMesh>();
        if (hexMesh == null)
            Debug.LogError("HexMesh failed to retrieve component  from children.");

        ClearMap();
        BuildMap();
    }

    /// <summary>
    /// Draws an outline around the top of each hex cell.
    /// </summary>
    void OnDrawGizmos()
    {
        if (!DrawGizmos)
            return;

        foreach (HexCell hexCell in HexCells)
        {
            Vector3[] hexCorners = HexMath.GetHexCorners(HexSize, HexOrientation);
            for (int s = 0; s < hexCorners.Length; s++)
            {
                Gizmos.DrawLine(
                    hexCell.CellPosition + hexCorners[s % 6],
                    hexCell.CellPosition + hexCorners[(s + 1) % 6]
                );
            }
        }
    }

    /// <summary>
    /// Loads map tile data from JSON.
    /// </summary>
    void LoadMapTilesData()
    {
        GameMapData = JsonUtility.FromJson<MapData>(MapSource.text);

        if (GameMapData == null)
            Debug.LogError("Game map JSON failed to decode.");
    }

    /// <summary>
    /// Builds the map HexCells form JSON and initiates mesh triangulation.
    /// </summary>
    public void BuildMap()
    {
        if (hexMesh == null)
        {
            Debug.LogError("Hex mesh is null");
            return;
        }

        LoadMapTilesData();

        HexCells = new HexCell[GameMapData.Width * GameMapData.Height];

        int i = 0;

        foreach (var mapTileData in GameMapData.MapTilesData)
        {
            Vector3 hexCenter = HexMath.GetHexCenter(
                HexSize, mapTileData.TilePosition, HexOrientation
            ) + transform.position;

            CubeCoordinates hexCubeCoordinates = HexMath.CubeFromPosition(
                mapTileData.TilePosition, HexOrientation
            );

            HexCell hexCell = Instantiate(HexCell, hexCenter, Quaternion.identity, this.transform);

            hexCell.CellPosition = hexCenter;
            hexCell.CellCoordinates = hexCubeCoordinates;
            hexCell.MapTileData = mapTileData;

            Debug.Log(@$"
            {hexCell.CellPosition}, The real position
            {hexCell.CellCoordinates}, The cube position: q,r,s
            {hexCell.MapTileData.TilePosition}, Logical map position (col,height,row): x,y,z
            ");

            HexCells[i++] = hexCell;
        }

        hexMesh.Triangulate(HexCells, HexSize, HexOrientation);
    }

    /// <summary>
    /// Destroys the hex cells and clears the mesh.
    /// 
    /// For development.
    /// </summary>
    public void ClearMap()
    {
        if (hexMesh == null)
            return;

        hexMesh.ClearMesh();

        if (HexCell == null || HexCells.Length == 0)
            return;

        foreach (var hexCell in HexCells)
        {
            if (hexCell == null)
                continue;

            if (Application.isEditor && !Application.isPlaying)
                DestroyImmediate(hexCell.gameObject);
            else
                Destroy(hexCell.gameObject);
        }
    }
}
