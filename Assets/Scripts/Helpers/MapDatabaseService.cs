using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
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
                    OffsetCoordinates = new UnityEngine.Vector2Int(apiTile.x_coord, apiTile.z_coord),
                    
                    TileType = apiTile.tile_data.tile_type,
                    
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
        
        // pass the data to other components

    }
}
