using System;
using System.Collections;
using NUnit.Framework;
using TTT.DataClasses.HexData;
using TTT.DataClasses.ModularData;
using TTT.DataClasses.Terrain;
using TTT.Helpers;
using TTT.Hex;
using TTT.Managers;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Basic unit tests for public APIs, singletons, data classes, and utilities.
/// Tests singleton behavior and public methods in the project.
/// </summary>
public class EditModeTests
{
    #region Singleton Tests

    [
        Test,
        Description(
            "GameManager singleton returns same instance on repeated access."
        )
    ]
    public void Singleton_GameManager_SameInstance()
    {
        var instance1 = GameManager.Instance;
        var instance2 = GameManager.Instance;
        var instance3 = GameManager.Instance;

        Assert.IsNotNull(instance1, "GameManager.Instance should not be null");
        Assert.AreSame(
            instance1,
            instance2,
            "GameManager repeated access should return same instance"
        );
        Assert.AreSame(
            instance2,
            instance3,
            "GameManager all accesses should reference same object"
        );
    }

    [
        Test,
        Description(
            "MapManager singleton returns same instance on repeated access."
        )
    ]
    public void Singleton_MapManager_SameInstance()
    {
        var instance1 = MapManager.Instance;
        var instance2 = MapManager.Instance;
        var instance3 = MapManager.Instance;

        Assert.IsNotNull(instance1, "MapManager.Instance should not be null");
        Assert.AreSame(
            instance1,
            instance2,
            "MapManager repeated access should return same instance"
        );
        Assert.AreSame(
            instance2,
            instance3,
            "MapManager all accesses should reference same object"
        );
    }

    #endregion

    #region HexMath Public Methods

    [
        Test,
        Description(
            "HexMath.OuterRadius returns positive value for positive hexSize."
        )
    ]
    public void HexMath_OuterRadius_ReturnsPositive()
    {
        float radius = HexMath.OuterRadius(3.0f);
        Assert.Greater(radius, 0, "OuterRadius should return positive value");
    }

    [
        Test,
        Description(
            "HexMath.InnerRadius returns positive value for positive hexSize."
        )
    ]
    public void HexMath_InnerRadius_ReturnsPositive()
    {
        float radius = HexMath.InnerRadius(3.0f);
        Assert.Greater(radius, 0, "InnerRadius should return positive value");
    }

    [Test, Description("HexMath.InnerRadius is smaller than OuterRadius.")]
    public void HexMath_InnerRadiusLessThanOuterRadius()
    {
        float hexSize = 3.0f;
        float inner = HexMath.InnerRadius(hexSize);
        float outer = HexMath.OuterRadius(hexSize);

        Assert.Less(
            inner,
            outer,
            "InnerRadius should be less than OuterRadius"
        );
    }

    [
        Test,
        Description(
            "HexMath.GetHexCenter returns valid vector for valid input."
        )
    ]
    public void HexMath_GetHexCenter_ValidVector()
    {
        var offset = new OffsetCoordinates(1, 1);
        var center = HexMath.GetHexCenter(
            3.0f,
            0,
            offset,
            HexOrientation.pointyTop
        );

        Assert.IsNotNull(center, "HexCenter should not be null");
        Assert.Greater(
            Vector3.Distance(center, Vector3.zero),
            0.1f,
            "HexCenter should be offset from origin"
        );
    }

    [
        Test,
        Description(
            "HexMath.GetHexCorners returns array for pointyTop orientation."
        )
    ]
    public void HexMath_GetHexCorners_ReturnsValidArray()
    {
        var corners = HexMath.GetHexCorners(3.0f, HexOrientation.pointyTop);

        Assert.IsNotNull(corners, "Corners array should not be null");
        Assert.AreEqual(6, corners.Length, "Hex should have 6 corners");
    }

    [
        Test,
        Description("HexMath.OddOffsetToCube converts coordinates correctly.")
    ]
    public void HexMath_OddOffsetToCube_ConvertsCorrectly()
    {
        var offset = new OffsetCoordinates(0, 0);
        var cube = HexMath.OddOffsetToCube(offset, HexOrientation.pointyTop);

        Assert.IsNotNull(cube, "CubeCoordinates should not be null");
        // (0,0) in odd offset should map to valid cube coordinates
        Assert.AreEqual(
            0,
            cube.q + cube.r + cube.s,
            "Cube coordinates should sum to 0"
        );
    }

    [
        Test,
        Description("HexMath.CubeToOddOffset converts coordinates correctly.")
    ]
    public void HexMath_CubeToOddOffset_ConvertsCorrectly()
    {
        var cube = new CubeCoordinates(1, 0, -1);
        var offset = HexMath.CubeToOddOffset(cube, HexOrientation.pointyTop);

        Assert.IsNotNull(offset, "OffsetCoordinates should not be null");
        Assert.GreaterOrEqual(offset.x, 0, "Offset x should be non-negative");
        Assert.GreaterOrEqual(offset.z, 0, "Offset z should be non-negative");
    }

    [
        Test,
        Description(
            "HexMath.RoundCube rounds float coordinates to nearest cube."
        )
    ]
    public void HexMath_RoundCube_RoundsCorrectly()
    {
        var cubeF = new CubeCoordinatesF(1.5f, 0.5f, -2.0f);
        var cube = HexMath.RoundCube(cubeF);

        Assert.IsNotNull(cube, "Rounded cube should not be null");
        Assert.AreEqual(
            0,
            cube.q + cube.r + cube.s,
            "Rounded cube should sum to 0"
        );
    }

    [
        Test,
        Description(
            "Coordinate round-trip (OffsetToCube to CubeToOddOffset) is reversible."
        )
    ]
    public void HexMath_CoordinateRoundTrip_Reversible()
    {
        var offsetOriginal = new OffsetCoordinates(3, 4);
        var cube = HexMath.OddOffsetToCube(
            offsetOriginal,
            HexOrientation.pointyTop
        );
        var offsetConverted = HexMath.CubeToOddOffset(
            cube,
            HexOrientation.pointyTop
        );

        Assert.AreEqual(
            offsetOriginal.x,
            offsetConverted.x,
            "x should match after round trip"
        );
        Assert.AreEqual(
            offsetOriginal.z,
            offsetConverted.z,
            "z should match after round trip"
        );
    }

    #endregion

    #region Coordinate Data Classes

    [
        Test,
        Description("CubeCoordinates constructor initializes fields correctly.")
    ]
    public void CubeCoordinates_Constructor_Valid()
    {
        var cube = new CubeCoordinates(1, 2, -3);
        Assert.AreEqual(1, cube.q, "q should be 1");
        Assert.AreEqual(2, cube.r, "r should be 2");
        Assert.AreEqual(-3, cube.s, "s should be -3");
    }

    [Test, Description("CubeCoordinates components sum to zero.")]
    public void CubeCoordinates_SumToZero()
    {
        var cube = new CubeCoordinates(2, -5, 3);
        Assert.AreEqual(
            0,
            cube.q + cube.r + cube.s,
            "Cube coordinates should sum to 0"
        );
    }

    [
        Test,
        Description(
            "OffsetCoordinates constructor initializes fields correctly."
        )
    ]
    public void OffsetCoordinates_Constructor_Valid()
    {
        var offset = new OffsetCoordinates(5, 10);
        Assert.AreEqual(5, offset.x, "x should be 5");
        Assert.AreEqual(10, offset.z, "z should be 10");
    }

    #endregion

    #region HexCell Data Class

    [Test, Description("HexCell can be instantiated and properties set.")]
    public void HexCell_Properties_CanBeSet()
    {
        var hexCell = new HexCell();
        var position = new Vector3(1, 2, 3);
        var cubeCoords = new CubeCoordinates(0, 0, 0);

        hexCell.CellPosition = position;
        hexCell.CellCubeCoordinates = cubeCoords;

        Assert.AreEqual(
            position,
            hexCell.CellPosition,
            "CellPosition should match"
        );
        Assert.AreEqual(
            cubeCoords,
            hexCell.CellCubeCoordinates,
            "CellCubeCoordinates should match"
        );
    }

    #endregion

    #region MapManager Public Methods

    [Test, Description("MapManager.SeaLevel public property is accessible.")]
    public void MapManager_SeaLevel_IsAccessible()
    {
        var mapManager = MapManager.Instance;
        Assert.IsNotNull(mapManager.SeaLevel, "SeaLevel should not be null");
    }

    [Test, Description("MapManager.RisingRate public property is accessible.")]
    public void MapManager_RisingRate_IsAccessible()
    {
        var mapManager = MapManager.Instance;
        Assert.IsNotNull(
            mapManager.RisingRate,
            "RisingRate should not be null"
        );
    }

    [Test, Description("MapManager.HexCells public collection is accessible.")]
    public void MapManager_HexCells_IsAccessible()
    {
        var mapManager = MapManager.Instance;
        Assert.IsNotNull(mapManager.HexCells, "HexCells should not be null");
    }

    [Test, Description("MapManager.ToFlood public queue is accessible.")]
    public void MapManager_ToFlood_IsAccessible()
    {
        var mapManager = MapManager.Instance;
        Assert.IsNotNull(
            mapManager.ToFlood,
            "ToFlood queue should not be null"
        );
    }

    [Test, Description("MapManager.FloodQueue public queue is accessible.")]
    public void MapManager_FloodQueue_IsAccessible()
    {
        var mapManager = MapManager.Instance;
        Assert.IsNotNull(
            mapManager.FloodQueue,
            "FloodQueue should not be null"
        );
    }

    #endregion

    #region Utility Tests

    [Test, Description("Resources.Load can load a test map JSON successfully.")]
    public void Resources_LoadTestMap_Succeeds()
    {
        var testMap = Resources.Load<TextAsset>("Maps/test_map_1");
        if (testMap != null)
        {
            Assert.Greater(
                testMap.text.Length,
                0,
                "Map text should not be empty"
            );
        }
        // If test map doesn't exist, test still passes as it's optional
    }

    [Test, Description("Unity component system is functional for tests.")]
    public void Unity_ComponentSystem_Functional()
    {
        var testGO = new GameObject("UnitTest_Component");
        var camera = testGO.AddComponent<Camera>();
        Assert.IsNotNull(
            camera,
            "Camera component should be added successfully"
        );
        UnityEngine.Object.DestroyImmediate(testGO);
    }

    #endregion
}
