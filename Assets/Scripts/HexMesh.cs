using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class HexMesh : NetworkBehaviour
{
    private Mesh mesh;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private List<Vector3> vertices;
    private List<int> triangles;
    private List<Color> colors;
    private Vector3[] cvertices;
    private Color[] ccolors;

    void InitializeMesh()
    {
        if (meshFilter == null)
            meshFilter = GetComponent<MeshFilter>();
        if (meshCollider == null)
            meshCollider = GetComponent<MeshCollider>();

        mesh = new Mesh
        {
            name = "The Hexagon Mesh",
            indexFormat = IndexFormat.UInt32          // This is so that we can have > 65000 vertices in the mesh, platform dependant so idk, multiple meshes (please no)
        };

        vertices = new();
        triangles = new();
        colors = new();
    }
    
    void Awake()
    {
        InitializeMesh();
    }

    void OnValidate()
    {
        InitializeMesh();
    }

    /// <summary>
    /// Adds of the HexCell.
    /// </summary>
    /// <param name="triVertexStart"></param>
    /// <param name="i"></param>
    void AddTopTriangles(int triVertexStart, int i)
    {
        triangles.Add(triVertexStart);
        triangles.Add(triVertexStart + ((i == 5) ? 1 : i + 2));
        triangles.Add(triVertexStart + i + 1);
    }

    /// <summary>
    /// Adds the triangles to the side of each cell.
    /// </summary>
    /// <param name="triVertexStart"></param>
    /// <param name="sideTriVertexStart"></param>
    /// <param name="i"></param>
    void AddSideTriangles(int triVertexStart, int sideTriVertexStart, int i)
    {
        // Tri one of the side face
        triangles.Add(sideTriVertexStart + ((i == 5) ? 0 : i + 1));
        triangles.Add(sideTriVertexStart + i);
        triangles.Add(triVertexStart + ((i == 5) ? 1 : i + 2));

        // Tri two of the side face
        triangles.Add(triVertexStart + ((i == 5) ? 1 : i + 2));
        triangles.Add(sideTriVertexStart + i);
        triangles.Add(triVertexStart + i + 1);
    }

    void TriangulateHelper(
        HexCell hexCell, float hexSize, HexOrientation hexOrientation)
    {
        bool aboveSeaLevel = hexCell.CellPosition.y > 0f;
        int triVertexStart = vertices.Count;
        hexCell.CenterVertexIndex = triVertexStart;

        vertices.Add(hexCell.CellPosition);
        colors.Add(hexCell.CellColor
            ?? hexCell.TerrainType.tileColor.Value);

        Vector3[] corners = HexMath.GetHexCorners(hexSize, hexOrientation);

        // Regular triangle vertices
        foreach (Vector3 corner in corners)
        {
            vertices.Add(hexCell.CellPosition + corner);
            colors.Add(hexCell.CellColor
                ?? hexCell.TerrainType.tileColor.Value);
        }

        int sideTriVertexStart = vertices.Count;

        // Vertices that will be used to draw the side faces
        foreach (Vector3 corner in corners)
        {
            if (!aboveSeaLevel)
                continue;

            vertices.Add(
                hexCell.CellPosition + corner - new Vector3(0, hexCell.CellPosition.y, 0));
            colors.Add(hexCell.CellColor
                ?? hexCell.TerrainType.tileColor.Value);
        }

        // Populate triangle and color arrays
        for (int i = 0; i < corners.Length; i++)
        {
            AddTopTriangles(triVertexStart, i);

            if (!aboveSeaLevel)
                continue;

            AddSideTriangles(triVertexStart, sideTriVertexStart, i);
        }
    }

    public void Triangulate(
        HexCell[,] hexCells, float hexSize, HexOrientation hexOrientation)
    {
        ClearMesh();

        foreach (HexCell hexCell in hexCells)
        {
            if (hexCell is null)
                continue;

            TriangulateHelper(hexCell, hexSize, hexOrientation);
        }

        mesh.vertices   = cvertices = vertices.ToArray();
        mesh.colors     = ccolors   = colors.ToArray();
        mesh.triangles  = triangles.ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    /// <summary>
    /// </summary>
    /// <param name="hexCell"></param>
    /// <param name="hexSize"></param>
    /// <param name="hexOrientation"></param>
    public void TriangulateCell(
        HexCell hexCell, float hexSize, HexOrientation hexOrientation)
    {
        bool aboveSeaLevel = hexCell.CellPosition.y > 0f;
        int c = hexCell.CenterVertexIndex; // c = counter, 😉

        Debug.Log(c);

        cvertices[c] = hexCell.CellPosition;
        ccolors[c++] = (hexCell.CellColor
            ?? hexCell.TerrainType.tileColor.Value);

        Vector3[] corners = HexMath.GetHexCorners(hexSize, hexOrientation);

        // Regular triangle vertices
        foreach (Vector3 corner in corners)
        {
            cvertices[c] = hexCell.CellPosition + corner;
            ccolors[c++] = hexCell.CellColor
                ?? hexCell.TerrainType.tileColor.Value;
        }

        // Vertices that will be used to draw the side faces
        foreach (Vector3 corner in corners)
        {
            if (!aboveSeaLevel)
                continue;

            cvertices[c] = hexCell.CellPosition + corner
               - new Vector3(0, hexCell.CellPosition.y, 0);
            ccolors[c++] = hexCell.CellColor
                ?? hexCell.TerrainType.tileColor.Value;
        }

        mesh.vertices   = cvertices;
        mesh.colors     = ccolors;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;

        Debug.Log("We are fine.");
    }

    public void ClearMesh()
    {
        vertices.Clear();
        triangles.Clear();
        colors.Clear();
        mesh.Clear();
    }
}
