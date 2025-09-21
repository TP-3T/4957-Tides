using System;
using Unity.Collections;
using UnityEditor.PackageManager;
using UnityEditor.SceneManagement;
using UnityEngine;

public class HexGrid : MonoBehaviour
{
    [SerializeField] public bool DrawGizmos;
    [SerializeField] public bool DrawDebugLabels;
    [SerializeField] public HexOrientation HexOrientation;
    [SerializeField] public float HexSize;
    [SerializeField] public HexCell HexCell;
    [SerializeField] public TextAsset MapSource;

    public static readonly int GRID_LAYER_MASK = 1 << 10;
    public HexCell[] HexCells;
    public MapData GameMapData;

    private HexMesh hexMesh;

    void InitializeGrid()
    {
        if (hexMesh == null)
            hexMesh = GetComponentInChildren<HexMesh>();
        if (hexMesh == null)
            Debug.LogError("HexMesh failed to retrieve component  from children.");
    }

    void Awake()
    {
        InitializeGrid();
        ClearMap();
        BuildMap();
    }

    void OnValidate()
    {
        InitializeGrid();
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
                    transform.position + hexCell.CellPosition + hexCorners[s % 6],
                    transform.position + hexCell.CellPosition + hexCorners[(s + 1) % 6]
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
                HexSize, mapTileData.Height, mapTileData.OffsetCoordinates, HexOrientation
            );

            CubeCoordinates hexCubeCoordinates = HexMath.OddOffsetToCube(
                mapTileData.OffsetCoordinates, HexOrientation
            );

            HexCell hexCell = Instantiate(HexCell, hexCenter, Quaternion.identity, this.transform);

            hexCell.CellPosition = hexCenter;
            hexCell.CellCubeCoordinates = hexCubeCoordinates;
            hexCell.MapTileData = mapTileData;

            // Debug.Log(@$"
            // {hexCell.CellPosition}, The real position
            // {hexCell.CellCubeCoordinates}, The cube position: q,r,s
            // {hexCell.MapTileData.OffsetCoordinates}, Logical map position (col, row): x,z 
            // ");

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
        {
            Debug.Log("Hex cell array reference lost");
            return;
        }

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

    public void HandlePlayerClick(Vector3 playerClickPoint)
    {
        Debug.Log($"Player clicked the grid {playerClickPoint}, {HexMath.PositionToCubeF(HexSize, playerClickPoint, HexOrientation)}");
        Debug.Log(@$"
        Player clicked the grid
        {playerClickPoint} cartesian
        {HexMath.PositionToCubeF(HexSize, playerClickPoint, HexOrientation)}
        {HexMath.RoundCube(HexMath.PositionToCubeF(HexSize, playerClickPoint, HexOrientation))}
        ");
    }
}
