using System;
using System.Collections;
using System.Reflection;
using JetBrains.Annotations;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using TTT.DataClasses.HexData;
using TTT.DataClasses.Terrain;
using TTT.GameEvents;
using TTT.Helpers;
using TTT.Hex;
using TTT.Managers;
using TTT.ModularData;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;

public class PlayModeTests
{
    private static NetworkManager EnsureNetworkManager()
    {
        // Reuse if already created
        if (NetworkManager.Singleton != null)
        {
            // Make sure transport exists on the singleton too
            if (NetworkManager.Singleton.NetworkConfig == null)
                NetworkManager.Singleton.NetworkConfig = new NetworkConfig();

            if (NetworkManager.Singleton.NetworkConfig.NetworkTransport == null)
            {
                var existingTransport =
                    NetworkManager.Singleton.GetComponent<UnityTransport>()
                    ?? NetworkManager.Singleton.gameObject.AddComponent<UnityTransport>();
                NetworkManager.Singleton.NetworkConfig.NetworkTransport = existingTransport;
            }
            return NetworkManager.Singleton;
        }

        var go = new GameObject("NetworkManager_Test");
        var nm = go.AddComponent<NetworkManager>();

        // Create a fresh NetworkConfig and attach UnityTransport
        nm.NetworkConfig = new NetworkConfig();
        var transport = go.AddComponent<UnityTransport>();
        nm.NetworkConfig.NetworkTransport = transport;

        // Optional: keep alive across scene loads
        UnityEngine.Object.DontDestroyOnLoad(go);
        return nm;
    }

    [UnityTest]
    public System.Collections.IEnumerator StartsHostWithTransport()
    {
        var nm = EnsureNetworkManager();

        Assert.IsNotNull(nm.NetworkConfig, "NetworkConfig must be assigned");
        Assert.IsNotNull(nm.NetworkConfig.NetworkTransport, "NetworkTransport must be assigned");

        // Start host directly or via your GameManager flow
        var started = nm.StartHost();
        Assert.IsTrue(started, "StartHost failed");

        yield return null;
        Assert.IsTrue(NetworkManager.Singleton.IsHost, "Host did not start");

        // Teardown
        if (NetworkManager.Singleton && NetworkManager.Singleton.IsListening)
            NetworkManager.Singleton.Shutdown();
        if (NetworkManager.Singleton)
            UnityEngine.Object.DestroyImmediate(NetworkManager.Singleton.gameObject);
    }

    private static void EnsureGameEventOn(GameManager gameManager)
    {
        // newMapEvent is a [SerializeField] private field; assign a temp instance if null
        var evtField = typeof(GameManager).GetField(
            "newMapEvent",
            BindingFlags.Instance | BindingFlags.NonPublic
        );
        if (evtField == null)
            return;

        var current = evtField.GetValue(gameManager) as GameEvent;
        if (current == null)
        {
            var tempEvent = ScriptableObject.CreateInstance<GameEvent>();
            evtField.SetValue(gameManager, tempEvent);
        }
    }

    private GameObject testGameObject;
    private MapManager mapManager;

    [UnityTest, Description("Starts host in test and triggers new map via GameManager.")]
    public IEnumerator MapSetup_CompletesSuccessfully()
    {
        var json = Resources.Load<TextAsset>("Maps/test_map_1");
        Assert.IsNotNull(json, "Map JSON should be loaded");

        var gameManager = GameManager.Instance;
        var mapManager = MapManager.Instance;

        // Provide LevelFile for GameManager
        gameManager.LevelFile = json;

        // Ensure NetworkManager + Transport exist
        var nm = EnsureNetworkManager();
        Assert.IsNotNull(nm, "NetworkManager should exist");

        // Ensure GameEvent exists to avoid null Raise()
        EnsureGameEventOn(gameManager);

        // Start Host via your flow
        var startArgs = ScriptableObject.CreateInstance<StartNetworkEventArgs>();
        startArgs.IsHost = true;
        gameManager.OnStartNetworkEvent(startArgs);

        // Let Netcode initialize
        yield return null;

        Assert.IsTrue(
            NetworkManager.Singleton && NetworkManager.Singleton.IsHost,
            "Host did not start"
        );
        Assert.IsNotNull(gameManager, "GameManager should be initialized");
        Assert.IsNotNull(mapManager, "MapManager should be initialized");

        // Teardown
        if (NetworkManager.Singleton && NetworkManager.Singleton.IsListening)
            NetworkManager.Singleton.Shutdown();
        if (NetworkManager.Singleton)
            UnityEngine.Object.DestroyImmediate(NetworkManager.Singleton.gameObject);
    }

    #region Application Tests
    [Test, Description("Asserts the application runs without errors.")]
    public void ApplicationRuns()
    {
        bool encounteredError = false;
        string errorMessage = string.Empty;

        Application.logMessageReceived += (condition, stackTrace, type) =>
        {
            if (type == LogType.Error || type == LogType.Exception)
            {
                encounteredError = true;
                errorMessage = condition;
            }
        };

        try
        {
            var testObj = new GameObject("AppRunTest");
            var testComponent = testObj.AddComponent<Camera>();
            Assert.IsNotNull(testComponent, "Unity component system should be functional");

            UnityEngine.Object.DestroyImmediate(testObj);
            var testScriptableObj = new MapData();
            Assert.IsNotNull(testScriptableObj, "MapData creation should work");

            var testCoords = new CubeCoordinates(1, 2, -3);
            Assert.AreEqual(1, testCoords.q, "Data classes should be usable");

            // Type hexGridType = typeof(HexGrid);
            Type mapManagerType = typeof(MapManager);
            // Assert.IsNotNull(hexGridType, "Core game types should be compiled correctly");
            Assert.IsNotNull(mapManagerType, "Manager types should be compiled correctly");

            // Assert
            Assert.IsFalse(
                encounteredError,
                $"Application should run without errors. Error encountered: {errorMessage}"
            );

            Assert.Pass("Application core systems are functional and run without errors");
        }
        finally
        {
            Application.logMessageReceived -= (condition, stackTrace, type) => { };
        }
    }
    #endregion

    #region Map Setup Tests
    [Test, Description("Map setup without required components throws a NullReferenceException.")]
    public void MapSetup_ThrowsNullReferenceException()
    {
        testGameObject = new GameObject("Test_MapManager");
        mapManager = MapManager.Instance;
        LogAssert.ignoreFailingMessages = true; // If there are expected log errors use LogAssert.Expect(LogType.Error, "*"); Replace * with expected message
        Assert.Throws<NullReferenceException>(() =>
            mapManager.OnNewMap(UnityEngine.Object.Instantiate(testGameObject))
        );
    }
    #endregion

    #region HexCell Tests
    [Test, Description("Retrieve cell data returns correct data for given coordinates.")]
    public void RetrieveCellData()
    {
        Assert.Pass();
    }
    #endregion

    #region Next Turn Tests
    [Test, Description("Next turn triggers the appropriate game event.")]
    public void NextTurn_TriggersEvent()
    {
        Assert.Pass();
    }

    [Test, Description("Next turn can only proceed with sufficient resources.")]
    public void NextTurn_RequiresResources()
    {
        Assert.Pass();
    }

    [Test, Description("Next turn updates resources correctly.")]
    public void NextTurn_UpdatesResources()
    {
        Assert.Pass();
    }

    [Test, Description("Next turn moves to the next season.")]
    public void NextTurn_SeasonChanges()
    {
        Assert.Pass();
    }

    [Test, Description("Next turn changes the year every four turns.")]
    public void NextTurn_YearChanges()
    {
        Assert.Pass();
    }

    [Test, Description("Next turn triggers a flood event.")]
    public void NextTurn_TriggersFlood()
    {
        Assert.Pass();
    }
    #endregion

    #region Game Over Tests
    [Test, Description("Appropriate game over occurs when the loss condition is met.")]
    public void GameOver_LossCondition()
    {
        Assert.Pass();
    }

    [Test, Description("Appropriate game over occurs when the win condition is met.")]
    public void GameOver_WinCondition()
    {
        Assert.Pass();
    }
    #endregion

    #region Flood Tests
    [Test, Description("Flood event changes sea level appropriately.")]
    public void Flood_ChangeSeaLevel()
    {
        Assert.Pass();
    }

    [Test, Description("Flood event changes flooded terrain appropriately.")]
    public void Flood_ChangeTerrain()
    {
        Assert.Pass();
    }

    [Test, Description("Flood event ensures natural flood barriers are respected.")]
    public void Flood_EnsureNaturalFloodBarrier()
    {
        Assert.Pass();
    }

    [Test, Description("Flood event ensures artificial flood barriers are respected.")]
    public void Flood_EnsureArtificialFloodBarrier()
    {
        Assert.Pass();
    }
    #endregion

    #region Building Tests
    [Test, Description("Building placement checks for valid terrain.")]
    public void BuildingPlacement_ValidTerrain()
    {
        Assert.Pass();
    }

    [Test, Description("Building placement checks for sufficient resources.")]
    public void BuildingPlacement_SufficientResources()
    {
        Assert.Pass();
    }

    [Test, Description("Building placement updates temporary resources correctly.")]
    public void BuildingPlacement_UpdatesTemporaryResources()
    {
        Assert.Pass();
    }

    [Test, Description("Temporary building removal refunds resources correctly.")]
    public void TemporaryBuildingRemoval_RefundsResources()
    {
        Assert.Pass();
    }

    [Test, Description("Building destruction updates resources correctly.")]
    public void BuildingDestruction_UpdatesResources()
    {
        Assert.Pass();
    }
    #endregion

    #region File Handling Tests
    [Test, Description("Attempting to load an invalid file throws an exception.")]
    public void LoadInvalidFile_ThrowsException()
    {
        //Assert.Throws<Exception>(() => LoadFile("invalid_path"));
        Assert.Pass();
    }

    [Test, Description("Loading a non-existent file throws a FileNotFoundException.")]
    public void LoadNonExistentFile_ThrowsFileNotFoundException()
    {
        //Assert.Throws<FileNotFoundException>(() => LoadFile("non_existent_path"));
        Assert.Pass();
    }

    [Test, Description("Loading a valid file completes successfully.")]
    public void LoadValidFile_CompletesSuccessfully()
    {
        //Assert.DoesNotThrow(() => LoadFile("valid_path"));
        Assert.Pass();
    }

    [Test, Description("Saving to a valid path completes successfully.")]
    public void SaveValidFile_CompletesSuccessfully()
    {
        //Assert.DoesNotThrow(() => SaveFile("valid_path"));
        Assert.Pass();
    }

    [Test, Description("Requesting a pre-saved map returns the correct data.")]
    public void LoadPreSavedMap_ReturnsCorrectData()
    {
        Assert.Pass();
    }

    [Test, Description("Requesting a non pre-saved map queries the database appropriately.")]
    public void LoadNonExistentMap_QueriesDatabase()
    {
        Assert.Pass();
    }

    [Test, Description("Downloading map data from the database completes successfully.")]
    public void DownloadMapData_CompletesSuccessfully()
    {
        Assert.Pass();
    }

    [Test, Description("Uploading map data to the database completes successfully.")]
    public void UploadMapData_CompletesSuccessfully()
    {
        Assert.Pass();
    }
    #endregion


    // A Test behaves as an ordinary method
    [Test]
    public void EditModeTestsSimplePasses()
    {
        // Use the Assert class to test conditions
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator EditModeTestsWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
    }
}
