using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class HexMesh : MonoBehaviour
{
    private Mesh mesh;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private List<Vector3> vertices;

    private List<int> triangles;

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

        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;

        vertices = new();
        triangles = new();
    }

    void Awake()
    {
        InitializeMesh();
    }

    void Triangulate(HexCell hexCell, float hexSize, HexOrientation hexOrientation)
    {
        bool aboveSeaLevel = hexCell.CellPosition.y > 0f;
        int triVertexStart = vertices.Count;

        vertices.Add(hexCell.CellPosition);

        Vector3[] corners = HexMath.GetHexCorners(hexSize, hexOrientation);

        // Regular triangle vertices
        foreach (Vector3 corner in corners)
            vertices.Add(hexCell.CellPosition + corner);

        int sideTriVertexStart = vertices.Count;

        // Vertices that will be used to draw the side faces
        foreach (Vector3 corner in corners)
        {
            if (!aboveSeaLevel)
                continue;

            vertices.Add(hexCell.CellPosition + corner - new Vector3(0, hexCell.CellPosition.y, 0));
        }

        for (int i = 0; i < corners.Length; i++)
        {
            triangles.Add(triVertexStart);
            triangles.Add(triVertexStart + ((i == 5) ? 1 : i + 2));
            triangles.Add(triVertexStart + i + 1);

            if (!aboveSeaLevel)
                continue;

            // Tri one of the side face
            triangles.Add(sideTriVertexStart + ((i == 5) ? 0 : i + 1));
            triangles.Add(sideTriVertexStart + i);
            triangles.Add(triVertexStart + ((i == 5) ? 1 : i + 2));

            // Tri two of the side face
            triangles.Add(triVertexStart + ((i == 5) ? 1 : i + 2));
            triangles.Add(sideTriVertexStart + i);
            triangles.Add(triVertexStart + i + 1);
        }

        // triangles.Add(triVertexStart);
        // triangles.Add(triVertexStart + 2);
        // triangles.Add(triVertexStart + 1);

        // triangles.Add(triVertexStart);
        // triangles.Add(triVertexStart + 3);
        // triangles.Add(triVertexStart + 2);

        // triangles.Add(triVertexStart);
        // triangles.Add(triVertexStart + 4);
        // triangles.Add(triVertexStart + 3);

        // triangles.Add(triVertexStart);
        // triangles.Add(triVertexStart + 5);
        // triangles.Add(triVertexStart + 4);

        // triangles.Add(triVertexStart);
        // triangles.Add(triVertexStart + 6);
        // triangles.Add(triVertexStart + 5);

        // triangles.Add(triVertexStart);
        // triangles.Add(triVertexStart + 1);
        // triangles.Add(triVertexStart + 6);
    }

    public void Triangulate(HexCell[] hexCells, float hexSize, HexOrientation hexOrientation)
    {
        ClearMesh();

        foreach (HexCell hexCell in hexCells)
        {
            Triangulate(hexCell, hexSize, hexOrientation);
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();
    }

    public void ClearMesh()
    {
        vertices.Clear();
        triangles.Clear();
        mesh.Clear();
    }
}
