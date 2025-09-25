using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class HexGrid : NetworkBehaviour
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

    public NetworkVariable<Color> meshColor = new NetworkVariable<Color>(
        Color.white,
        //settings for the network variable
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner

        );


    void InitializeGrid()
    {
        if (hexMesh == null)
            hexMesh = GetComponentInChildren<HexMesh>();
        if (hexMesh == null)
            Debug.LogError("HexMesh failed to retrieve component  from children.");
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        BuildandCreateGrid();


        // ⭐ NEW: Subscribe to the value changed event on ALL clients
        meshColor.OnValueChanged += OnMeshColorChanged;

        // Apply the current color immediately when a client connects/spawns.
        // This is important for clients joining mid-game.
        if (hexMesh != null)
        {
            ApplyColorToMesh(meshColor.Value);
        }
    }
    
    // ⭐ NEW: Unsubscribe on despawn to prevent memory leaks
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        meshColor.OnValueChanged -= OnMeshColorChanged;
    }

    // ⭐ NEW: Callback method to apply the new color
    private void OnMeshColorChanged(Color previousColor, Color newColor)
    {
        Debug.Log($"HexGrid mesh color changed from {previousColor} to {newColor}");
        ApplyColorToMesh(newColor);
    }

    private void ApplyColorToMesh(Color colorToApply)
    {
        if (hexMesh != null && hexMesh.GetComponent<MeshRenderer>() != null)
        {
            hexMesh.GetComponent<MeshRenderer>().material.color = colorToApply;
        }
    }

    void BuildandCreateGrid()
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
            hexCell.AddComponent<MeshRenderer>();
            hexCell.GetComponent<MeshRenderer>().material.color = Color.blue;
            // Debug.Log(@$"
            // {hexCell.CellPosition}, The real position
            // {hexCell.CellCubeCoordinates}, The cube position: q,r,s
            // {hexCell.MapTileData.OffsetCoordinates}, Logical map position (col, row): x,z 

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

    [ServerRpc(RequireOwnership = false)]
    public void HandlePlayerClickServerRpc(Vector3 playerClickPoint)
    {
        Color nextColor;

        Color currentColor = this.meshColor.Value;

        if (currentColor == Color.red)
        {
            nextColor = Color.blue;
        }
        else
        {
            nextColor = Color.red;
        }

        // Debug.Log("hexcell 1 position: " + HexCells[1].CellPosition);
        // Debug.Log("Hexcell 1 cell cube coord: " + HexCells[1].CellCubeCoordinates);
        // Debug.Log("clickpoint to cubeF" + HexMath.PositionToCubeF(HexSize, playerClickPoint, HexOrientation));
        // Debug.Log("clickpoint rounded cubeF" + HexMath.RoundCube(HexMath.PositionToCubeF(HexSize, playerClickPoint, HexOrientation)));

        foreach (var HexCell in HexCells)
        {
            if (HexCell.CellCubeCoordinates.q
            == HexMath.RoundCube(HexMath.PositionToCubeF(HexSize, playerClickPoint, HexOrientation)).q &&
            HexCell.CellCubeCoordinates.r
            == HexMath.RoundCube(HexMath.PositionToCubeF(HexSize, playerClickPoint, HexOrientation)).r &&
            HexCell.CellCubeCoordinates.s
            == HexMath.RoundCube(HexMath.PositionToCubeF(HexSize, playerClickPoint, HexOrientation)).s)
            {
                // Debug.Log("Player clicked on hex cell " + HexCell.CellCubeCoordinates);
            }
        }
        this.meshColor.Value = nextColor;
    }
}