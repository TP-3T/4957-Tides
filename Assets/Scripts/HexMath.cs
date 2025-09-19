using JetBrains.Annotations;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public enum HexOrientation
{
    FLAT_TOP,
    POINTY_TOP
}

public struct AxialCoordinates
{
    public int q { get; private set; }
    public int r { get; private set; }

    public AxialCoordinates(int q, int r)
    {
        this.q = q;
        this.r = r;
    }
}

public struct CubeCoordinates
{
    public int q { get; private set; }
    public int r { get; private set; }
    public int s { get; private set; }

    public CubeCoordinates(int q, int r, int s)
    {
        this.q = q;
        this.r = r;
        this.s = s;
    }

    public static CubeCoordinates FromAxial(AxialCoordinates ac)
    {
        return new CubeCoordinates(ac.q, ac.r, (-ac.q - ac.r));
    }
}

public static class HexMath
{
    public static float OuterRadius(float hexSize)
    {
        return hexSize;
    }

    public static float InnerRadius(float hexSize)
    {
        return hexSize * 0.866025404f;
    }

    public static Vector3 GetHexCorner(float hexSize, int cornerIndex, HexOrientation orientation)
    {
        Vector3 corner;
        float angle = cornerIndex * 60f;

        if (orientation == HexOrientation.POINTY_TOP)
        {
            angle += 30f;
        }

        corner.x = hexSize * Mathf.Cos(Mathf.Deg2Rad * angle);
        corner.y = 0f;
        corner.z = hexSize * Mathf.Sin(Mathf.Deg2Rad * angle);

        return corner;
    }

    public static Vector3[] GetHexCorners(float hexSize, HexOrientation orientation)
    {
        Vector3[] corners = new Vector3[6];
        for (int i = 0; i < corners.Length; i++)
        {
            corners[i] = GetHexCorner(hexSize, i, orientation);
        }
        return corners;
    }

    /// <summary>
    /// In the flat top orientation, the horizontal distance between adjacent hexagons centers is horiz = 3/4 * width = 3/2 * size. The vertical distance is vert = height = sqrt(3) * size = 2 * inradius.
    /// In the pointy top orientation, the horizontal distance between adjacent hexagon centers is horiz = width == sqrt(3) * size == 2 * inradius. The vertical distance is vert = 3/4 * height == 3/2 * size.
    /// 
    /// https://www.redblobgames.com/grids/hexagons/#basics
    /// </summary>
    /// <param name="hexSize"></param>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <param name="orientation"></param>
    /// <returns></returns>
    public static Vector3 GetHexCenter(float hexSize, int x, int z, HexOrientation orientation)
    {
        Vector3 point;
        if (orientation == HexOrientation.POINTY_TOP)
        {
            point.x = (x + z * 0.5f - z / 2) * (InnerRadius(hexSize) * 2f);     // Determine offset by even or odd row
            point.y = 0f;
            point.z = z * (OuterRadius(hexSize) * 1.5f);
        }
        else
        {
            point.x = x * (OuterRadius(hexSize) * 1.5f);
            point.y = 0f;
            point.z = (z + x * 0.5f - x / 2) * (InnerRadius(hexSize) * 2f);     // Determine offset by even or odd row
        }
        return point;
    }
}
