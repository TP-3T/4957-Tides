using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;
using TTT.DataClasses.HexData;

namespace TTT.Hex
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshCollider), typeof(MeshRenderer)),
    RequireComponent(typeof(NetworkObject))]
    public class SeaMesh : NetworkBehaviour
    {
        private Mesh _seaMesh;
        private MeshFilter _meshFilter;
        private MeshCollider _meshCollider;
        private List<Vector3> _vertices = new();
        private List<int> _triangles = new();
        // private List<Color> _colors = new();
        // private Vector3[] _cvertices = new Vector3[0];
        // private Color[] _ccolors = new Color[0];

        private void InitializeMesh()
        {
            if (_meshFilter == null)
                _meshFilter = GetComponent<MeshFilter>();
            if (_meshCollider == null)
                _meshCollider = GetComponent<MeshCollider>();

            _seaMesh = new Mesh
            {
                name = "The Hexagon Mesh",
                indexFormat = IndexFormat.UInt32, // This is so that we can have > 65000 vertices in the mesh, platform dependant so idk, multiple meshes (please no)
            };

            _vertices = new();
            _triangles = new();
            // colors = new();
        }

        void Awake()
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
            _triangles.Add(triVertexStart);
            _triangles.Add(triVertexStart + ((i == 5) ? 1 : i + 2));
            _triangles.Add(triVertexStart + i + 1);
        }

        public void Verticify(NetworkList<HexCell> hexCells, float hexSize, HexOrientation hexOrientation)
        {
            ClearMesh();

            for (int i = 0; i < hexCells.Count; i++)
            {
                HexCell hexCell = hexCells[i];
                int triVertexStart = _vertices.Count;

                _vertices.Add(hexCell.CellPosition);

                Vector3[] corners = HexMath.GetHexCorners(hexSize, hexOrientation);

                // Regular triangle vertices
                foreach (Vector3 corner in corners)
                {
                    _vertices.Add(hexCell.CellPosition + corner);
                }

                int sideTriVertexStart = _vertices.Count;

                // Vertices that will be used to draw the side faces
                foreach (Vector3 corner in corners)
                {
                    _vertices.Add(
                        hexCell.CellPosition + corner - new Vector3(0, hexCell.CellPosition.y, 0)
                    );
                }

                // Populate triangle and color arrays
                for (int k = 0; k < corners.Length; k++)
                {
                    AddTopTriangles(triVertexStart, k);
                }
            }

            _seaMesh.vertices = /*_cvertices =*/ _vertices.ToArray();
            // _seaMesh.colors = ccolors = colors.ToArray();
            _seaMesh.triangles = _triangles.ToArray();

            _seaMesh.RecalculateNormals();
            _seaMesh.RecalculateBounds();

            _meshFilter.sharedMesh = _seaMesh;
            _meshCollider.sharedMesh = _seaMesh;
        }

        public void TriangulateCell(
            HexCell hexCell,
            float hexSize,
            HexOrientation hexOrientation)
        {
        }
        
        public void TriangulateCells(
            HexCell[] hexCells,
            float hexSize,
            HexOrientation hexOrientation
        )
        {
            foreach (HexCell c in hexCells)
            {
                int count = c.CenterVertexIndex; // c = counter, 😉

                // cvertices[count] = c.CellPosition;
                // ccolors[count++] = (c.CellColor);

                Vector3[] corners = HexMath.GetHexCorners(hexSize, hexOrientation);

                // Regular triangle vertices
                foreach (Vector3 corner in corners)
                {
                    // cvertices[count] = c.CellPosition + corner;
                    // ccolors[count++] = c.CellColor;
                }

                // Vertices that will be used to draw the side faces
                foreach (Vector3 corner in corners)
                {
                    // cvertices[count] =
                    //     c.CellPosition + corner - new Vector3(0, c.CellPosition.y, 0);
                    // ccolors[count++] = c.CellColor;
                }
            }

            // mesh.SetVertices(cvertices);
            // mesh.SetColors(ccolors);
        }

        public void ClearMesh()
        {
            _vertices.Clear();
            _triangles.Clear();
            // colors.Clear();
            _seaMesh.Clear();
        }
    }
}
