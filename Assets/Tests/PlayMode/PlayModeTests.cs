using System;
using System.Collections;
using System.Reflection;
using JetBrains.Annotations;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using TTT.DataClasses.HexData;
using TTT.DataClasses.ModularData;
using TTT.DataClasses.Terrain;
using TTT.GameEvents;
using TTT.Helpers;
using TTT.Hex;
using TTT.Managers;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.TestTools;

/*
                                      ,---,
                                   ,`--.' |
    ,---,                          |   :  :  ___
  .'  .' `\                        |   |  ',--.'|_
,---.'     \    ,---.        ,---, '   :  ||  | :,'
|   |  .`\  |  '   ,'\   ,-+-. /  |;   |.' :  : ' :
:   : |  '  | /   /   | ,--.'|'   |'---' .;__,'  /
|   ' '  ;  :.   ; ,. :|   |  ,"' |      |  |   |
'   | ;  .  |'   | |: :|   | /  | |      :__,'| :
|   | :  |  ''   | .; :|   | |  | |        '  : |__
'   : | /  ; |   :    ||   | |  |/         |  | '.'|
|   | '` ,/   \   \  / |   | |--'          ;  :    ;
;   :  .'      `----'  |   |/              |  ,   /
|   ,.'                '---'                ---`-'
'---'
I wouldn't recommend trying to implement any playmode tests.
Unless you really know both the networking and event system we're using.
And are prepared to waste a lot of time.
*/

public class PlayModeTests
{
    private static IEnumerator WaitForCondition(
        Func<bool> condition,
        float timeoutSeconds,
        string errorMessage
    )
    {
        float elapsed = 0f;
        while (!condition() && elapsed < timeoutSeconds)
        {
            yield return null;
            elapsed += Time.deltaTime;
        }

        if (!condition())
        {
            Assert.Fail($"{errorMessage} (timeout after {timeoutSeconds:F1}s)");
        }
    }

    [UnitySetUp]
    public IEnumerator Setup()
    {
        var nm = EnsureNetworkManager();

        Assert.IsNotNull(nm.NetworkConfig, "NetworkConfig must be assigned");
        Assert.IsNotNull(
            nm.NetworkConfig.NetworkTransport,
            "NetworkTransport must be assigned"
        );

        // Start host directly or via your GameManager flow
        var started = nm.StartHost();
        Assert.IsTrue(started, "StartHost failed");

        // Wait up to 5 seconds for host to become active
        yield return WaitForCondition(
            () =>
                NetworkManager.Singleton != null
                && NetworkManager.Singleton.IsHost,
            5f,
            "Host did not start"
        );
    }

    [UnityTearDown]
    public IEnumerator Teardown()
    {
        if (NetworkManager.Singleton && NetworkManager.Singleton.IsListening)
            NetworkManager.Singleton.Shutdown();
        if (NetworkManager.Singleton)
            UnityEngine.Object.DestroyImmediate(
                NetworkManager.Singleton.gameObject
            );
        yield return null;
    }

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
                NetworkManager.Singleton.NetworkConfig.NetworkTransport =
                    existingTransport;
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
            Assert.IsNotNull(
                testComponent,
                "Unity component system should be functional"
            );

            UnityEngine.Object.DestroyImmediate(testObj);
            var testScriptableObj = new MapData();
            Assert.IsNotNull(testScriptableObj, "MapData creation should work");

            var testCoords = new CubeCoordinates(1, 2, -3);
            Assert.AreEqual(1, testCoords.q, "Data classes should be usable");

            Type mapManagerType = typeof(MapManager);
            Assert.IsNotNull(
                mapManagerType,
                "Manager types should be compiled correctly"
            );

            // Assert
            Assert.IsFalse(
                encounteredError,
                $"Application should run without errors. Error encountered: {errorMessage}"
            );

            Assert.Pass(
                "Application core systems are functional and run without errors"
            );
        }
        finally
        {
            Application.logMessageReceived -= (
                condition,
                stackTrace,
                type
            ) => { };
        }
    }
    #endregion
}
