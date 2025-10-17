using System;
using UnityEngine;

namespace TTT.Hex
{
    public enum HexOrientation
    {
        flatTop,
        pointyTop,
    }

    [Serializable]
    public struct OffsetCoordinates
    {
        public int x;
        public int z;

        public OffsetCoordinates(int x, int z)
        {
            this.x = x;
            this.z = z;
        }

        public override string ToString()
        {
            return $"({x}, {z})";
        }
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

        public CubeCoordinates(int q, int r)
        {
            this.q = q;
            this.r = r;
            this.s = (-q - r);
        }

        public override string ToString()
        {
            return $"({q}, {r}, {s})";
        }

        public static CubeCoordinates operator +(CubeCoordinates one, CubeCoordinates two)
        {
            return new CubeCoordinates(one.q + two.q, one.r + two.r, one.s + two.s);
        }
    }

    public struct CubeCoordinatesF
    {
        public float q { get; set; }
        public float r { get; set; }
        public float s { get; set; }

        public CubeCoordinatesF(float q, float r, float s)
        {
            this.q = q;
            this.r = r;
            this.s = s;
        }

        public CubeCoordinatesF(float q, float r)
        {
            this.q = q;
            this.r = r;
            this.s = -q - r;
        }

        public override string ToString()
        {
            return $"({q}, {r}, {s})";
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

        public static Vector3 GetHexCorner(
            float hexSize,
            int cornerIndex,
            HexOrientation orientation
        )
        {
            Vector3 corner;
            float angle = cornerIndex * 60f;

            if (orientation == HexOrientation.pointyTop)
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
        public static Vector3 GetHexCenter(
            float hexSize,
            int x,
            int y,
            int z,
            HexOrientation orientation
        )
        {
            Vector3 point;
            if (orientation == HexOrientation.pointyTop)
            {
                point.x = (x + z * 0.5f - z / 2) * (InnerRadius(hexSize) * 2f); // Determine offset by even or odd row
                point.y = y;
                point.z = z * (OuterRadius(hexSize) * 1.5f);
            }
            else
            {
                point.x = x * (OuterRadius(hexSize) * 1.5f);
                point.y = y;
                point.z = (z + x * 0.5f - x / 2) * (InnerRadius(hexSize) * 2f); // Determine offset by even or odd row
            }
            return point;
        }

        /// <summary>
        /// No height to the center, automatically 0f in the Y component.
        /// </summary>
        /// <param name="hexSize"></param>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <param name="orientation"></param>
        /// <returns></returns>
        public static Vector3 GetHexCenter(float hexSize, int x, int z, HexOrientation orientation)
        {
            return GetHexCenter(hexSize, x, 0, z, orientation);
        }

        /// <summary>
        /// Use the map tile position as a parameter.
        /// </summary>
        /// <param name="hexSize"></param>
        /// <param name="position"></param>
        /// <param name="hexOrientation"></param>
        /// <returns></returns>
        public static Vector3 GetHexCenter(
            float hexSize,
            int height,
            OffsetCoordinates position,
            HexOrientation hexOrientation
        )
        {
            return GetHexCenter(hexSize, position.x, height, position.z, hexOrientation);
        }

        /// <summary>
        /// Rounds fractional cube coordinates to the integer cube coordinates.
        /// </summary>
        /// <param name="cubeF"></param>
        /// <param name="hexSize"></param>
        /// <param name="hexOrientation"></param>
        /// <returns></returns>
        public static CubeCoordinates RoundCube(CubeCoordinatesF cubeF)
        {
            int q = Mathf.RoundToInt(cubeF.q);
            int r = Mathf.RoundToInt(cubeF.r);
            int s = Mathf.RoundToInt(cubeF.s);

            float dQ = Mathf.Abs(q - cubeF.q);
            float dR = Mathf.Abs(r - cubeF.r);
            float dS = Mathf.Abs(s - cubeF.s);

            if (dQ > dR && dQ > dS)
            {
                q = -r - s;
            }
            else if (dR > dS)
            {
                r = -q - s;
            }
            else
            {
                s = -q - r;
            }

            return new CubeCoordinates(q, r, s);
        }

        /// <summary>
        /// Calculates cube coordiantes from a given position.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="hexOrientation"></param>
        /// <returns></returns>
        public static CubeCoordinates OddOffsetToCube(
            OffsetCoordinates position,
            HexOrientation hexOrientation
        )
        {
            int q,
                r;

            if (hexOrientation == HexOrientation.pointyTop)
            {
                q = position.x - (position.z - (position.z & 1)) / 2;
                r = position.z;
            }
            else
            {
                q = position.x;
                r = position.z - (position.x - (position.x & 1)) / 2;
            }

            return new CubeCoordinates(q, r);
        }

        /// <summary>
        /// Calculates the offset coordinates from cube coordinates.
        /// </summary>
        /// <param name="cubeCoordinates"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public static OffsetCoordinates CubeToOddOffset(
            CubeCoordinates cubeCoordinates,
            HexOrientation hexOrientation
        )
        {
            int x,
                z;

            if (hexOrientation == HexOrientation.pointyTop)
            {
                x = cubeCoordinates.q + (cubeCoordinates.r - (cubeCoordinates.r & 1)) / 2;
                z = cubeCoordinates.r;
            }
            else
            {
                x = cubeCoordinates.q;
                z = cubeCoordinates.r + (cubeCoordinates.q - (cubeCoordinates.q & 1)) / 2;
            }

            return new OffsetCoordinates(x, z);
        }

        /// <summary>
        /// Converts cartesian coordinate points into fractional cube coordinates.
        /// </summary>
        /// <param name="hexSize"></param>
        /// <param name="position"></param>
        /// <param name="hexOrientation"></param>
        /// <returns></returns>
        public static CubeCoordinatesF PositionToCubeF(
            float hexSize,
            Vector3 position,
            HexOrientation hexOrientation
        )
        {
            if (hexOrientation == HexOrientation.pointyTop)
            {
                float x = position.x / hexSize;
                float z = position.z / hexSize;

                float q = (0.57735027f * x) - (1f / 3 * z);
                float r = (2f / 3 * z);

                return new CubeCoordinatesF(q, r);
            }
            else
            {
                float x = position.x / hexSize;
                float z = position.z / hexSize;

                float q = (2f / 3 * x);
                float r = (-1f / 3 * x) + (0.57735027f * z);

                return new CubeCoordinatesF(q, r);
            }
        }
    }
}
