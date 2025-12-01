using System.Collections.Generic;
using TTT.DataClasses.HexData;
using TTT.DataClasses.PlayerResources;
using UnityEngine;

namespace TTT.DataClasses.States
{
    [CreateAssetMenu(
        fileName = "PlayerStats",
        menuName = "Scriptable Objects/Player Stats"
    )]
    public class PlayerStats : ScriptableObject
    {
        [Header("Player Resources")]
        [field: SerializeField]
        public PlayerResource money { get; set; }

        [field: SerializeField]
        public PlayerResource power { get; set; }

        [field: SerializeField]
        public PlayerResource population { get; set; }

        [Header("Starting Resources")]
        [SerializeField]
        private static readonly int startingMoney = 500;

        [SerializeField]
        private static readonly int startingPower = 0;

        [SerializeField]
        private static readonly int startingPopulation = 0;

        // [Header("Sea Level Calculation")]
        // [Tooltip(
        //     "How much pollution contributes to sea level rise (default: 0.001 = 1mm per ppm)"
        // )]
        // [SerializeField]
        // private float pollutionToSeaLevelFactor = 0.001f;

        // [Tooltip(
        //     "Base rate of sea level rise per year regardless of pollution (meters)"
        // )]
        // [SerializeField]
        // private float baseSeaLevelRiseRate = 0.05f;

        public HexCell? selectedHexCell;
        public TileData selectedTileData;
        public string selectedTileJson;

        public void SetSelectedTile(HexCell hexCell, TileData tileData)
        {
            selectedHexCell = hexCell;
            selectedTileData = tileData;
            selectedTileJson = JsonUtility.ToJson(tileData, true);
        }

        public void SetSelectedTile(HexCell hexCell)
        {
            selectedHexCell = hexCell;
            selectedTileData = null;
            selectedTileJson = null;
        }

        public void ClearSelectedTile()
        {
            selectedHexCell = null;
            selectedTileData = null;
            selectedTileJson = null;
        }

        public List<PlayerResource> GetAllResources()
        {
            return new List<PlayerResource> { money, power, population };
        }

        public bool CanAfford(Dictionary<PlayerResource, int> costs)
        {
            foreach (var cost in costs)
            {
                if (cost.Key.AmountOwned < cost.Value)
                    return false;
            }
            return true;
        }

        public void SpendResources(Dictionary<PlayerResource, int> costs)
        {
            foreach (var cost in costs)
            {
                cost.Key.ApplyChange(-cost.Value);
            }
        }

        public void AddResources(Dictionary<PlayerResource, int> gains)
        {
            foreach (var gain in gains)
            {
                gain.Key.ApplyChange(gain.Value);
            }
        }

        public void ResetResources()
        {
            money?.Set(startingMoney);
            power?.Set(startingPower);
            population?.Set(startingPopulation);
        }

        public void InitializeResources()
        {
            money?.Set(startingMoney);
            power?.Set(startingPower);
            population?.Set(startingPopulation);
        }
    }
}
