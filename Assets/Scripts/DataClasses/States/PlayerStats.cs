using System.Collections.Generic;
using TTT.DataClasses.HexData;
using TTT.DataClasses.PlayerResources;
using UnityEngine;
using UnityEngine.Events;
namespace TTT.DataClasses.States
{
    [CreateAssetMenu(
        fileName = "PlayerStats",
        menuName = "Scriptable Objects/Player Stats"
    )]
    public class PlayerStats : ScriptableObject
    {
        [Header("Player Resources")]
        [SerializeField]
        public PlayerResource money { get; set; }

        [SerializeField]
        public PlayerResource power{ get; set; }

        [SerializeField]
        public PlayerResource population { get; set; }

        public HexCell? selectedHexCell;
        public TileData selectedTileData;
        public string selectedTileJson;

        public UnityEvent OnResourcesChanged;
        public UnityEvent OnTileSelected;

        private void OnEnable()
        {
            OnResourcesChanged ??= new UnityEvent();
            OnTileSelected ??= new UnityEvent();
        }

        public void SetSelectedTile(HexCell hexCell, TileData tileData)
        {
            selectedHexCell = hexCell;
            selectedTileData = tileData;
            selectedTileJson = JsonUtility.ToJson(tileData, true);
            OnTileSelected?.Invoke();
        }

        public void SetSelectedTile(HexCell hexCell)
        {
            selectedHexCell = hexCell;
            selectedTileData = null;
            selectedTileJson = null;
            OnTileSelected?.Invoke();
        }

        public void ClearSelectedTile()
        {
            selectedHexCell = null;
            selectedTileData = null;
            selectedTileJson = null;
            OnTileSelected?.Invoke();
        }

        public void NotifyResourcesChanged()
        {
            OnResourcesChanged?.Invoke();
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
            NotifyResourcesChanged();
        }

        public void AddResources(Dictionary<PlayerResource, int> gains)
        {
            foreach (var gain in gains)
            {
                gain.Key.ApplyChange(gain.Value);
            }
            NotifyResourcesChanged();
        }

        public void ResetResources()
        {
            money?.Set(0);
            power?.Set(0);
            population?.Set(0);
            NotifyResourcesChanged();
        }
    }
}
