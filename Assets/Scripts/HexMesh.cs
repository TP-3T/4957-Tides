using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class HexMesh : MonoBehaviour
{
    private Mesh mesh;
    private MeshFilter meshFilter;
    private List<Vector3> vertices;

    private List<int> triangles;

    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = mesh = new Mesh();
        mesh.name = "The Hexagon Mesh";
        mesh.indexFormat = IndexFormat.UInt32;          // This is so that we can have > 65000 vertices in the mesh, platform dependant so idk, multiple meshes (please no)

        vertices = new();
        triangles = new();
    }

    void Triangulate(HexCell hexCell, float hexSize, HexOrientation hexOrientation)
    {
        int triVertexStart = vertices.Count;

        vertices.Add(hexCell.CenterPosition);

        Vector3[] corners = HexMath.GetHexCorners(hexSize, hexOrientation);

        foreach (Vector3 corner in corners)
        {
            vertices.Add(hexCell.CenterPosition + corner);
        }

        for (int i = 0; i < corners.Length; i++)
        {
            triangles.Add(triVertexStart);
            triangles.Add(triVertexStart + ((i == 5) ? 1 : i + 2));
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
        mesh.Clear();

        vertices.Clear();
        triangles.Clear();

        foreach (HexCell hexCell in hexCells)
        {
            Triangulate(hexCell, hexSize, hexOrientation);
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateNormals();
    }
}
