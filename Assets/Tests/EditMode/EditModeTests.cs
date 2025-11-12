using System;
using System.Collections;
using NUnit.Framework;
using TTT.DataClasses.HexData;
using TTT.DataClasses.Terrain;
using TTT.Helpers;
using TTT.Hex;
using TTT.Managers;
using TTT.ModularData;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Basic unit tests for public APIs, singletons, data classes, and utilities.
/// No networking, no ScriptableObject events, no complex async setup.
/// </summary>
public class EditModeTests
{
    #region Singleton Tests

    [
        Test,
        Description(
            "HexMath utility methods compute correct cube coordinates from offset coordinates."
        )
    ]
    public void HexMath_OddOffsetToCube_ReturnsCorrectCoordinates()
    {
        var offset = new OffsetCoordinates(0, 0);
        var cube = HexMath.OddOffsetToCube(offset, HexOrientation.pointyTop);
        Assert.IsNotNull(cube, "CubeCoordinates should not be null");
        Assert.AreEqual(0, cube.q, "Cube q coordinate should be 0");
    }

    [Test, Description("GenericSingleton ensures repeated access returns the same instance.")]
    public void GenericSingleton_RepeatedAccess_ReturnsSameInstance()
    {
        // Multiple accesses to GameManager.Instance should return the same reference
        var instance1 = GameManager.Instance;
        var instance2 = GameManager.Instance;
        var instance3 = GameManager.Instance;

        Assert.IsNotNull(instance1, "GameManager.Instance should not be null");
        Assert.AreSame(
            instance1,
            instance2,
            "First and second access should return the same instance"
        );
        Assert.AreSame(
            instance1,
            instance3,
            "First and third access should return the same instance"
        );
    }

    [Test, Description("HexMath computes correct hex center position from offset coordinates.")]
    public void HexMath_GetHexCenter_ReturnsValidVector()
    {
        var offset = new OffsetCoordinates(1, 1);
        var center = HexMath.GetHexCenter(3.0f, 0, offset, HexOrientation.pointyTop);
        Assert.IsNotNull(center, "Hex center should not be null");
        Assert.Greater(
            Vector3.Distance(center, Vector3.zero),
            0,
            "Hex center should be offset from origin"
        );
    }

    [Test, Description("MapManager singleton ensures repeated access returns the same instance.")]
    public void Singleton_MapManager_RepeatedAccess_ReturnsSameInstance()
    {
        var instance1 = MapManager.Instance;
        var instance2 = MapManager.Instance;
        var instance3 = MapManager.Instance;

        Assert.IsNotNull(instance1, "MapManager.Instance should not be null");
        Assert.AreSame(
            instance1,
            instance2,
            "Multiple accesses to MapManager should return the same instance"
        );
        Assert.AreSame(
            instance2,
            instance3,
            "All accesses to MapManager should reference the same object"
        );
    }

    [Test, Description("GameManager singleton ensures repeated access returns the same instance.")]
    public void Singleton_GameManager_RepeatedAccess_ReturnsSameInstance()
    {
        var instance1 = GameManager.Instance;
        var instance2 = GameManager.Instance;
        var instance3 = GameManager.Instance;

        Assert.IsNotNull(instance1, "GameManager.Instance should not be null");
        Assert.AreSame(
            instance1,
            instance2,
            "Multiple accesses to GameManager should return the same instance"
        );
        Assert.AreSame(
            instance2,
            instance3,
            "All accesses to GameManager should reference the same object"
        );
    }

    [
        Test,
        Description(
            "ResourcesController singleton ensures repeated access returns the same instance."
        )
    ]
    public void Singleton_ResourcesController_RepeatedAccess_ReturnsSameInstance()
    {
        var instance1 = ResourcesController.Instance;
        var instance2 = ResourcesController.Instance;
        var instance3 = ResourcesController.Instance;

        Assert.IsNotNull(instance1, "ResourcesController.Instance should not be null");
        Assert.AreSame(
            instance1,
            instance2,
            "Multiple accesses to ResourcesController should return the same instance"
        );
        Assert.AreSame(
            instance2,
            instance3,
            "All accesses to ResourcesController should reference the same object"
        );
    }

    [Test, Description("CubeCoordinates constructor initializes with correct values.")]
    public void CubeCoordinates_Constructor_InitializesCorrectly()
    {
        var cube = new CubeCoordinates(1, 2, -3);
        Assert.AreEqual(1, cube.q, "q should be 1");
        Assert.AreEqual(2, cube.r, "r should be 2");
        Assert.AreEqual(-3, cube.s, "s should be -3");
    }

    [Test, Description("OffsetCoordinates constructor initializes with correct values.")]
    public void OffsetCoordinates_Constructor_InitializesCorrectly()
    {
        var offset = new OffsetCoordinates(5, 10);
        Assert.AreEqual(5, offset.x, "x should be 5");
        Assert.AreEqual(10, offset.z, "z should be 10");
    }

    [Test, Description("MapData can be created and properties set.")]
    public void MapData_Constructor_InitializesWithDefaultValues()
    {
        var mapData = new MapData();
        Assert.IsNotNull(mapData, "MapData should not be null");
        Assert.AreEqual(0, mapData.Width, "Default width should be 0");
        Assert.AreEqual(0, mapData.Height, "Default height should be 0");
        Assert.IsNotNull(mapData.MapTilesData, "MapTilesData list should exist");
    }

    [Test, Description("MapTileData can be created with valid terrain type.")]
    public void MapTileData_Constructor_InitializesCorrectly()
    {
        var tile = new MapTileData
        {
            TileType = "coastal",
            Height = 1,
            OffsetCoordinates = new OffsetCoordinates(0, 0),
        };
        Assert.AreEqual("coastal", tile.TileType, "TileType should be 'coastal'");
        Assert.AreEqual(1, tile.Height, "Height should be 1");
        Assert.AreEqual(0, tile.OffsetCoordinates.x, "OffsetCoordinates x should be 0");
    }

    #endregion

    #region Data Class Tests

    [Test, Description("HexCell can be instantiated and properties set.")]
    public void HexCell_PropertiesCanBeSet()
    {
        var hexCell = new HexCell();
        var position = new Vector3(1, 2, 3);
        var cubeCoords = new CubeCoordinates(0, 0, 0);

        hexCell.CellPosition = position;
        hexCell.CellCubeCoordinates = cubeCoords;

        Assert.AreEqual(position, hexCell.CellPosition, "CellPosition should be set correctly");
        Assert.AreEqual(
            cubeCoords,
            hexCell.CellCubeCoordinates,
            "CellCubeCoordinates should be set correctly"
        );
    }

    [Test, Description("TerrainType properties are accessible.")]
    public void TerrainType_PropertiesAccessible()
    {
        // TerrainType is a ScriptableObject with read-only Color and UniqueID
        // We test that we can read them (they exist and don't throw)
        var terrain = ScriptableObject.CreateInstance<TerrainType>();
        Assert.IsNotNull(terrain, "TerrainType instance should be created");
        // Color and UniqueID are read-only public properties
        Color terrainColor = terrain.Color;
        string terrainUid = terrain.UniqueID;
        Assert.IsNotNull(terrainColor, "Color should be accessible");
    }

    #endregion

    #region Coordinate Conversion Tests

    [Test, Description("Coordinate conversions are reversible (round-trip).")]
    public void CoordinateConversion_RoundTrip_Reversible()
    {
        var offsetOriginal = new OffsetCoordinates(3, 4);
        var cube = HexMath.OddOffsetToCube(offsetOriginal, HexOrientation.pointyTop);
        var offsetConverted = HexMath.CubeToOddOffset(cube, HexOrientation.pointyTop);

        Assert.AreEqual(offsetOriginal.x, offsetConverted.x, "x should match after round trip");
        Assert.AreEqual(offsetOriginal.z, offsetConverted.z, "z should match after round trip");
    }

    #endregion

    #region Utility Tests

    [Test, Description("Resources.Load can load a test map JSON.")]
    public void Resources_LoadTestMap_Succeeds()
    {
        var testMap = Resources.Load<TextAsset>("Maps/test_map_1");
        Assert.IsNotNull(testMap, "Test map should load successfully");
        Assert.Greater(testMap.text.Length, 0, "Map text should not be empty");
    }

    [Test, Description("Unity component system is functional.")]
    public void Unity_ComponentSystem_Functional()
    {
        var testGO = new GameObject("UnitTest_Component");
        var camera = testGO.AddComponent<Camera>();
        Assert.IsNotNull(camera, "Camera component should be added");
        UnityEngine.Object.DestroyImmediate(testGO);
    }

    #endregion
}
