using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TTT.DataClasses;
using TTT.DataClasses.HexData;
using TTT.DataClasses.States;
using TTT.DataClasses.TileFeatures;
using TTT.GameEvents;
using TTT.Hex;
using Unity.Netcode;
using UnityEngine;

namespace TTT.Managers
{
    /// <summary>
    /// Logic-y stuff for the grid shall live here.
    /// </summary>
    public partial class MapManager
    {
        [SerializeField]
        private GameEvent DestroyingFeatureEvent;

        [SerializeField]
        private GameEvent AudioEvent;

        [SerializeField]
        private FeatureRuntimeSet spawnedFeatures;

        /// <summary>
        /// The map's own cache of spawned features, updated only on flood.
        /// Not keeping it always updated is ok as it's currently only accessed during flooding.
        /// </summary>
        private Feature[] spawnedFeaturesCache = Array.Empty<Feature>();

        private void FloodCell(ref HexCell hc)
        {
            int index = GetCellIndexFromCubeCoordinates(hc.CellCubeCoordinates);
            hc.Flooded = true;

            HexCells[index] = hc;

            Vector3 cellPosition = hc.CellPosition;

            bool cellHasFeature = spawnedFeaturesCache.Any(feat =>
                feat.CellPosition == cellPosition
            );

            if (cellHasFeature)
            {
                RemoveFeatureClientRpc(hc.CellPosition);
            }
        }

        #region:Cell Management

        private void SetCellCenterVertex(HexCell hc, int cv)
        {
            int index = GetCellIndexFromCubeCoordinates(hc.CellCubeCoordinates);
            hc.CenterVertexIndex = cv;
            HexCells[index] = hc;
        }

        private CubeCoordinates GetCubeCoordinatesFromPosition(Vector3 position)
        {
            CubeCoordinatesF cf = HexMath.PositionToCubeF(
                MapManager.HexSize,
                position,
                MapManager.HexOrientation
            );
            CubeCoordinates cc = HexMath.RoundCube(cf);
            return cc;
        }

        private int GetCellIndexFromCubeCoordinates(CubeCoordinates hc)
        {
            if (MapManager.HexOrientation == HexOrientation.pointyTop)
            {
                return (
                    (Mathf.RoundToInt(hc.r / 2) + hc.q)
                    + (hc.r * _hexGridWidth.Value)
                );
            }
            else
            {
                throw new Exception(
                    "This math has lazily not been implemented yet, get on it you git!" // po: TODO
                );
            }
        }

        private int GetCellIndexFromPosition(Vector3 position)
        {
            CubeCoordinates cc = GetCubeCoordinatesFromPosition(position);
            int ci = GetCellIndexFromCubeCoordinates(cc);
            return ci;
        }

        private HexCell? GetCellFromCubeCoordinates(
            CubeCoordinates hc,
            out bool success
        )
        {
            if (MapManager.HexOrientation == HexOrientation.pointyTop)
            {
                int cubeCoordinateIndex = GetCellIndexFromCubeCoordinates(hc);
                // Debug.Log(cubeCoordinateIndex);
                if (
                    cubeCoordinateIndex < HexCells.Count
                    && cubeCoordinateIndex >= 0
                    && ((Mathf.RoundToInt(hc.r / 2) + hc.q) >= 0)
                ) // Prevent row wrap-around, enforce row constraint (r component / 2 + q component zeros out if this is a valid cell)
                {
                    success = true;
                    return HexCells[cubeCoordinateIndex];
                }
                else
                {
                    success = false;
                    return null;
                }
            }
            else
            {
                success = false;
                throw new Exception("The math for this is not implemented");
            }
        }

        public HexCell? GetCellFromPosition(Vector3 position, out bool success)
        {
            CubeCoordinates hc = GetCubeCoordinatesFromPosition(position);
            return GetCellFromCubeCoordinates(hc, out success);
        }

        public List<HexCell> GetCellNeighbours(HexCell c)
        {
            List<HexCell> neighbours = new List<HexCell>();

            foreach (CubeCoordinates dir in MapManager.NeighbourDirections)
            {
                CubeCoordinates neighborPos = c.CellCubeCoordinates + dir;
                HexCell? n = GetCellFromCubeCoordinates(
                    neighborPos,
                    out bool success
                );

                if (success)
                    neighbours.Add((HexCell)n);
            }

            return neighbours;
        }

        #region: Game OBJ Management

        private void SpawnGridMesh(GameObject hm)
        {
            // Only server spawns NetworkObjects
            if (!NetworkManager.Singleton.IsServer)
            {
                // Debug.Log("[MapManager] Client skipping hex mesh spawn (server will handle it)");
                return;
            }

            // Get reference to HexMesh prefab
            GameObject hexMeshGameObject = Instantiate(hm);
            HexMesh hexMeshInstance = hexMeshGameObject.GetComponent<HexMesh>();

            // Instance HexMesh prefab based off of the build data
            hexMeshInstance.GetComponent<NetworkObject>().Spawn();
            hexMeshInstance.transform.position += new Vector3(
                0.0f,
                -0.01f,
                0.0f
            );
            _hexMeshId.Value = hexMeshInstance.NetworkObjectId;
            // Debug.Log($"[MapManager] Server spawned hex mesh with ID: {_hexMeshId.Value}");

            // Server also triangulates for itself
            TriangulateHexMeshClientRpc();
        }

        private void SpawnSeaMesh(GameObject sm)
        {
            // Only server spawns NetworkObjects
            if (!NetworkManager.Singleton.IsServer)
            {
                // Debug.Log("[MapManager] Client skipping sea mesh spawn (server will handle it)");
                return;
            }

            // Get reference to SeaMesh prefab
            GameObject seaMeshGameObject = Instantiate(sm);
            SeaMesh seaMeshInstance = seaMeshGameObject.GetComponent<SeaMesh>();

            // Instance SeaMesh prefab based off of the
            seaMeshInstance.GetComponent<NetworkObject>().Spawn();
            _seaMeshId.Value = seaMeshInstance.NetworkObjectId;
            // Debug.Log($"[MapManager] Server spawned sea mesh with ID: {_seaMeshId.Value}");

            // Server also triangulates for itself
            TriangulateSeaMeshClientRpc();
        }

        #endregion

        #endregion

        #region: Coroutines

        /// <summary>
        /// Spawns the map objects.
        /// </summary>
        /// <returns></returns>
        private IEnumerator SpawnMapObjects()
        {
            yield return AssetLoader<GameObject>.Load(
                new("P_HexMesh"),
                SpawnGridMesh
            );
            yield return AssetLoader<GameObject>.Load(
                new("P_SeaMesh"),
                SpawnSeaMesh
            );
        }

        /// <summary>
        /// Spawns features onto the map.
        /// </summary>
        /// <returns></returns>
        private IEnumerator LoadFeatureTypes()
        {
            yield return AssetLoader<FeatureType>.LoadGroup(
                "building",
                CacheFeatureType
            );
            // _featuresLoaded = true;
            // Debug.Log($"Loaded {_featureTypesByUniqueId.Count} feature types");
        }

        /// <summary>
        /// Simulate rising on a per turn basis, not per frame.
        /// </summary>
        public IEnumerator RaiseSea()
        {
            // update spawned features cache before flooding
            spawnedFeaturesCache = spawnedFeatures.GetItems();

            while (true)
            {
                if (ToFlood.Count == 0)
                {
                    while (FloodQueue.Count > 0)
                    {
                        HexCell test = FloodQueue.Dequeue();

                        //! CB: No braces on if/else! Bad style :C
                        if (
                            test.CellPosition.y
                            <= GameManager.Instance.SeaLevel.Value
                        )
                            ToFlood.Enqueue(test);
                        else
                            AboveSeaLevelQueue.Enqueue(test);
                    }

                    while (AboveSeaLevelQueue.Count > 0)
                        FloodQueue.Enqueue(AboveSeaLevelQueue.Dequeue());

                    // Debug.Log("Flood fill cycle complete");

                    break;
                }

                List<HexCell> flooded = new();
                int cellCount = 0;
                while (ToFlood.Count > 0 && cellCount < CellsPerFrame)
                {
                    HexCell cell = ToFlood.Dequeue();
                    FloodCell(ref cell);
                    flooded.Add(cell);

                    List<HexCell> neighbours = GetCellNeighbours(cell);
                    foreach (HexCell neighbor in neighbours)
                    {
                        if (neighbor.Flooded)
                            continue;
                        if (
                            ToFlood.Contains(neighbor)
                            || FloodQueue.Contains(neighbor)
                        )
                            continue;
                        if (
                            neighbor.CellPosition.y
                            <= GameManager.Instance.SeaLevel.Value
                        )
                            ToFlood.Enqueue(neighbor);
                        else
                            FloodQueue.Enqueue(neighbor);
                    }

                    cellCount++;
                }

                TriangulateSeaMeshClientRpc(flooded.ToArray());
                AudioEvent.Raise(
                    new AudioEventArgs()
                    {
                        Type = AudioTypes.ONESHOT,
                        ToPlay = "water_rise",
                    }
                );
                yield return null;
            }

            onFloodEnded.Raise();
        }

        /// <summary>
        /// Coroutine to asynchronously spawn features onto the map.
        /// </summary>
        /// <returns></returns>
        private IEnumerator SpawnPendingFeaturesAsync()
        {
            if (_featureTypesByUniqueId.Keys.Count <= 0)
            {
                // Debug.LogWarning("feature types didn't load");
                yield break;
            }

            int spawnedCount = 0;
            int spawnsPerFrame = 5; // Spawn 50 buildings per frame for smooth-ish loading
            Dictionary<string, int> featureTypeCounts =
                new Dictionary<string, int>();

            // Debug.Log(
            //     $"Starting async spawn of {_pendingFeatures.Count} features..."
            // );

            foreach (var featureNet in _pendingFeaturesGoated)
            {
                var featureIdS = featureNet.FeatureId.ToString();
                if (
                    _featureTypesByUniqueId.TryGetValue(
                        featureNet.FeatureId.ToString(),
                        out FeatureType featureType
                    )
                )
                {
                    var args =
                        ScriptableObject.CreateInstance<BuildingFeatureArgs>();
                    args.Location = featureNet.FeaturePosition;
                    args.FeatureType = featureType;
                    args.OwnedByClient = false;
                    // _onFeatureBuild.Raise(args);

                    _onFeaturePlace.Raise(args);

                    spawnedCount++;

                    // Track counts by type
                    if (!featureTypeCounts.ContainsKey(featureIdS))
                        featureTypeCounts[featureIdS] = 0;
                    featureTypeCounts[featureIdS]++;

                    // Yield every X spawns to maintain framerate
                    //Kinda doesn't work :/
                    if (spawnedCount % spawnsPerFrame == 0)
                    {
                        yield return null; // Wait one frame
                    }
                }
                else
                {
                    // Debug.LogWarning(
                    //     $"skipped unknown feature '{featureIdS}' at {featureNet.FeaturePosition}"
                    // ); //THis basically never happens but I put this here just in case :/
                }
            }

            if (spawnedCount > 0)
            {
                // Debug.Log($"Finished spawning {spawnedCount} features:");
                // foreach (var kvp in featureTypeCounts)
                // {
                //     Debug.Log($"  {kvp.Key}: {kvp.Value}");
                // }
            }

            // _pendingFeatures.Clear();

            _mapLoadFinishEvent.Raise(
                new NewMapFinishedEventArgs()
                {
                    WasSuccessful = true,
                    MaxMapHeight = _hexMaxHeight,
                    SeaLevel = SeaLevel.Value,
                }
            );
        }
        #endregion
    }
}
