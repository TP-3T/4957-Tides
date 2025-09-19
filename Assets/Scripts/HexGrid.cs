using UnityEditor.PackageManager;
using UnityEngine;

public class HexGrid : MonoBehaviour
{

    [SerializeField]
    public bool DrawGizmos;

    [SerializeField]
    public HexOrientation HexOrientation;

    [SerializeField]
    public int Width, Height;

    [SerializeField]
    public float HexSize;

    [SerializeField]
    public HexCell HexCell;

    private HexCell[] hexCells;

    private HexMesh hexMesh;

    void Instantiate()
    {
        hexMesh = GetComponentInChildren<HexMesh>();

        hexCells = new HexCell[Width * Height];

        for (int z = 0, i = 0; z < Height; z++)
        {
            for (int x = 0; x < Width; x++)
            {
                Vector3 hexCenter = HexMath.GetHexCenter(HexSize, x, z, HexOrientation) + transform.position;
                HexCell hexCell = Instantiate(HexCell, hexCenter, Quaternion.identity, this.transform);

                hexCell.CenterPosition = hexCenter;

                hexCells[i++] = hexCell;
            }
        }

        hexMesh.Triangulate(hexCells, HexSize, HexOrientation);
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

    void Awake()
    {
        Instantiate();
    }
}
