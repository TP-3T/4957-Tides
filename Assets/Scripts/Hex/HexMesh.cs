using System.Collections.Generic;
using TTT.DataClasses.HexData;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Rendering;

namespace TTT.Hex
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider)),
    RequireComponent(typeof(NetworkObject)),
    RequireComponent(typeof(NetworkTransform))]
    public class HexMesh : NetworkBehaviour
    {
        public static LayerMask LayerMask = 1 << 10;

        private Mesh mesh;
        private MeshFilter meshFilter;
        private MeshCollider meshCollider;
        private List<Vector3> vertices = new();
        private List<int> triangles = new();
        private List<Color> colors = new();
        private Vector3[] cvertices = new Vector3[0];
        private Color[] ccolors = new Color[0];

        void InitializeMesh()
        {
            if (meshFilter == null)
                meshFilter = GetComponent<MeshFilter>();
            if (meshCollider == null)
                meshCollider = GetComponent<MeshCollider>();

            mesh = new Mesh
            {
                name = "The Hexagon Mesh",
                indexFormat = IndexFormat.UInt32, // This is so that we can have > 65000 vertices in the mesh, platform dependant so idk, multiple meshes (please no)
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

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
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

        public void Triangulate(NetworkList<HexCell> hexCells, float hexSize, HexOrientation hexOrientation)
        {
            ClearMesh();

            for (int i = 0; i < hexCells.Count; i++)
            {
                HexCell hexCell = hexCells[i];
                int triVertexStart = vertices.Count;

                if (IsServer)
                    hexCell.CenterVertexIndex = triVertexStart;

                vertices.Add(hexCell.CellPosition);
                colors.Add(hexCell.CellColor);

                Vector3[] corners = HexMath.GetHexCorners(hexSize, hexOrientation);

                // Regular triangle vertices
                foreach (Vector3 corner in corners)
                {
                    vertices.Add(hexCell.CellPosition + corner);
                    colors.Add(hexCell.CellColor);
                }

                int sideTriVertexStart = vertices.Count;

                // Vertices that will be used to draw the side faces
                foreach (Vector3 corner in corners)
                {
                    vertices.Add(
                        hexCell.CellPosition + corner - new Vector3(0, hexCell.CellPosition.y, 0)
                    );
                    colors.Add(hexCell.CellColor);
                }

                // Populate triangle and color arrays
                for (int k = 0; k < corners.Length; k++)
                {
                    AddTopTriangles(triVertexStart, k);
                    AddSideTriangles(triVertexStart, sideTriVertexStart, k);
                }

                if (IsServer)
                    hexCells[i] = hexCell;
            }

            mesh.vertices = cvertices = vertices.ToArray();
            mesh.colors = ccolors = colors.ToArray();
            mesh.triangles = triangles.ToArray();

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            meshFilter.sharedMesh = mesh;
            meshCollider.sharedMesh = mesh;
        }

        /// <summary>
        /// Retriangulates a single cell in the mesh.
        /// </summary>
        /// <param name="hexCell"></param>
        /// <param name="hexSize"></param>
        /// <param name="hexOrientation"></param>
        public void ReTriangulateCell(HexCell hexCell, float hexSize, HexOrientation hexOrientation)
        {
            // Debug.Log(hexCell.CenterVertexIndex);
            // Debug.Log(hexCell.CellColor);

            int count = hexCell.CenterVertexIndex; // c = counter, 😉

            cvertices[count] = hexCell.CellPosition;
            ccolors[count++] = (hexCell.CellColor);

            Vector3[] corners = HexMath.GetHexCorners(hexSize, hexOrientation);

            // Regular triangle vertices
            foreach (Vector3 corner in corners)
            {
                cvertices[count] = hexCell.CellPosition + corner;
                ccolors[count++] = hexCell.CellColor;
            }

            // Vertices that will be used to draw the side faces
            foreach (Vector3 corner in corners)
            {
                cvertices[count] =
                    hexCell.CellPosition + corner - new Vector3(0, hexCell.CellPosition.y, 0);
                ccolors[count++] = hexCell.CellColor;
            }

            mesh.SetVertices(cvertices);
            mesh.SetColors(ccolors);
        }

        /// <summary>
        /// Retriangulates a single cell with a temporary highlight color applied ONLY to the side/edge vertices.
        /// The top face retains its permanent color from the last full Triangulate (the cell's original color).
        /// </summary>
        /// <param name="hexCell">The cell data (used for vertex index).</param>
        /// <param name="highlightColor">The temporary color to use for the edges.</param>
        public void ReTriangulateCellEdgeHighlight(HexCell hexCell, Color highlightColor) 
        {

            int count = hexCell.CenterVertexIndex;
        
            count++; 

            count += 6; 
            
            for (int i = 0; i < 6; i++) {
                // Apply the highlight color to the side/edge vertices
                ccolors[count++] = highlightColor; 
            }

            // Apply the new colors array to the mesh on the local client
            mesh.SetColors(ccolors);

        }
        /// <summary>
        /// Retriangualtes a subset of the mesh.
        /// </summary>
        /// <param name="hexCells"></param>
        /// <param name="hexSize"></param>
        /// <param name="hexOrientation"></param>
        public void ReTriangulateCells(
            HexCell[] hexCells,
            float hexSize,
            HexOrientation hexOrientation
        )
        {
            foreach (HexCell c in hexCells)
            {
                int count = c.CenterVertexIndex; // c = counter, 😉

                cvertices[count] = c.CellPosition;
                ccolors[count++] = (c.CellColor);

                Vector3[] corners = HexMath.GetHexCorners(hexSize, hexOrientation);

                // Regular triangle vertices
                foreach (Vector3 corner in corners)
                {
                    cvertices[count] = c.CellPosition + corner;
                    ccolors[count++] = c.CellColor;
                }

                // Vertices that will be used to draw the side faces
                foreach (Vector3 corner in corners)
                {
                    cvertices[count] =
                        c.CellPosition + corner - new Vector3(0, c.CellPosition.y, 0);
                    ccolors[count++] = c.CellColor;
                }
            }

            mesh.SetVertices(cvertices);
            mesh.SetColors(ccolors);
        }

        public void ClearMesh()
        {
            vertices.Clear();
            triangles.Clear();
            colors.Clear();
            mesh.Clear();
        }
    }
}
