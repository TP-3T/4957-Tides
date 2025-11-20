using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using TTT.DataClasses;
using TTT.DataClasses.HexData;
using UnityEngine;
using UnityEngine.Networking;

namespace TTT.Helpers
{
    /// <summary>
    /// This service handles talking to the database to retrieve maps and their associated details.
    /// </summary>
    public static class MapDatabaseService
    {
        // api enpoints
        private const string BASE_URL = "https://3tdb.coreybuchan.com";
        private const string MAPS_ENDPOINT = "/maps";
        private const string MAP_BY_ID_ENDPOINT = "/maps/mapId";
        private const string MAP_BY_STEAMID_ENDPOINT = "/maps/steamId";
        private const string MAP_BY_NAME_ENDPOINT = "/maps/mapName";

        #region API REQUESTS

        /// <summary>
        /// Fetches the list of available maps from the database
        /// </summary>
        public static IEnumerator FetchAllMaps(Action<string> onSuccess, Action<string> onError)
        {
            string url = BASE_URL + MAPS_ENDPOINT;

            UnityWebRequest request = UnityWebRequest.Get(url);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                onSuccess?.Invoke(jsonResponse);
            }
            else
            {
                onError?.Invoke($"Request failed: {request.error}");
            }

            request.Dispose();
        }

        /// <summary>
        /// Fetches a map by its map ID
        /// </summary>
        public static IEnumerator FetchMapByMapId(int mapId, Action<string> onSuccess, Action<string> onError)
        {
            string url = $"{BASE_URL}{MAP_BY_ID_ENDPOINT}/{mapId}";

            UnityWebRequest request = UnityWebRequest.Get(url);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                onSuccess?.Invoke(jsonResponse);
            }
            else
            {
                onError?.Invoke($"Request failed: {request.error}");
            }

            request.Dispose();
        }

        /// <summary>
        /// Fetches all maps associated with the given steamID
        /// </summary>
        public static IEnumerator FetchAllMapsBySteamId(string steamId, Action<string> onSuccess, Action<string> onError)
        {
            string url = $"{BASE_URL}{MAP_BY_STEAMID_ENDPOINT}/{steamId}";

            UnityWebRequest request = UnityWebRequest.Get(url);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                onSuccess?.Invoke(jsonResponse);
            }
            else
            {
                onError?.Invoke($"Request failed: {request.error}");
            }
        }

        /// <summary>
        /// Fetches a map by its map name 
        /// </summary>
        public static IEnumerator FetchMapByMapName(string mapName, Action<string> onSuccess, Action<string> onError)
        {
            string url = $"{BASE_URL}{MAP_BY_NAME_ENDPOINT}/{mapName}";

            UnityWebRequest request = UnityWebRequest.Get(url);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                onSuccess?.Invoke(jsonResponse);
            }
            else
            {
                onError?.Invoke($"Request failed: {request.error}");
            }
        }

        #endregion
        
        #region DATA CLASSES

        /// <summary>
        /// Represents a map with its details from the requests that return lists of maps
        /// </summary>
        [Serializable]
        private class ApiMapListItem
        {
            public int map_id;
            public string map_name;
            public string steam_id;
        }

        /// <summary>
        /// Represents a single tile's data
        /// </summary>
        [Serializable]
        private class ApiTileData
        {
            public int tile_data_id;
            public int tile_type;
            public int elevation;
        }

        /// <summary>
        /// Represents a single tile from the map tile array
        /// </summary>
        [Serializable]
        private class ApiMapTile
        {
            public int map_id;
            public int tile_data_id;
            public int z_coord;
            public int x_coord;
            public int owner;
            public string label;
            public ApiTileData tile_data;
        }

        /// <summary>
        /// Public class containing basic info about a map
        /// Used for displaying map lists in UI
        /// </summary>
        [Serializable]
        public class MapInfo
        {
            public int MapId;
            public string MapName;
            public string SteamId;

            public MapInfo(int mapId, string mapName, string steamId)
            {
                MapId = mapId;
                MapName = mapName;
                SteamId = steamId;
            }
        }

        #endregion

        #region CONVERT MAPDATA

        /// <summary>
        /// Converts the API response into MapData
        /// </summary>
        /// <param name="apiTiles">List of tiles from the API</param>
        /// <param name="mapName">Name for the map</param>
        /// <returns>MapData ready to use in the game</returns>
        private static MapData ConvertToMapData(List<ApiMapTile> apiTiles, string mapName)
        {
            int maxX = 0;
            int maxZ = 0;

            foreach (var tile in apiTiles)
            {
                if (tile.x_coord > maxX) { maxX = tile.x_coord; }
                if (tile.z_coord > maxZ) { maxZ = tile.z_coord; }
            }

            int width = maxX + 1;
            int height = maxZ + 1;

            List<MapTileData> gameTiles = new List<MapTileData>();

            foreach (var apiTile in apiTiles)
            {
                MapTileData gameTile = new MapTileData
                {
                    OffsetCoordinates = new OffsetCoordinates(apiTile.x_coord, apiTile.z_coord),
                    
                    TileType = (TerrainTypeId)apiTile.tile_data.tile_type,
                    
                    Height = apiTile.tile_data.elevation
                };

                gameTiles.Add(gameTile);
            }

            return new MapData
            {
                Name = mapName,
                Width = width,
                Height = height,
                MapTilesData = gameTiles
            };
        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Fetches and parses the list of available maps
        /// Returns a list of MapInfo objects ready for UI display
        /// </summary>
        public static IEnumerator GetMapList(Action<List<MapInfo>> onSuccess, Action<string> onError)
        {
            string jsonResponse = null;
            string error = null;

            yield return FetchAllMaps(
                json => jsonResponse = json,
                err => error = err
            );

            if (error != null)
            {
                onError?.Invoke(error);
                yield break;
            }

            try
            {
                List<ApiMapListItem> apiMaps = JsonConvert.DeserializeObject<List<ApiMapListItem>>(jsonResponse);

                List<MapInfo> mapInfoList = new List<MapInfo>();
                foreach (var apiMap in apiMaps)
                {
                    mapInfoList.Add(new MapInfo(apiMap.map_id, apiMap.map_name, apiMap.steam_id));
                }

                onSuccess?.Invoke(mapInfoList);
            }
            catch (Exception e)
            {
                onError?.Invoke($"Failed to parse map list: {e.Message}");
            }
        }

        /// <summary>
        /// Fetches a map by ID, parses it, and converts it to MapData
        /// Returns complete MapData ready for the game to use
        /// </summary>
        public static IEnumerator GetMapDataById(int mapId, string mapName, Action<MapData> onSuccess, Action<string> onError)
        {
            string jsonResponse = null;
            string error = null;

            yield return FetchMapByMapId(
                mapId,
                json => jsonResponse = json,
                err => error = err
            );

            if (error != null)
            {
                onError?.Invoke(error);
                yield break;
            }

            try
            {
                List<ApiMapTile> apiTiles = JsonConvert.DeserializeObject<List<ApiMapTile>>(jsonResponse);

                MapData mapData = ConvertToMapData(apiTiles, mapName);

                onSuccess?.Invoke(mapData);
            }
            catch (Exception e)
            {
                onError?.Invoke($"Failed to parse map data: {e.Message}");
            }
        }

        #endregion

    }
}
