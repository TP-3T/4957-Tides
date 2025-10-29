using System.Collections;
using TTT.Helpers;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TTT.Managers
{
    public class GameManager : GenericSingleton<GameManager>
    {
        [SerializeField]
        private TextAsset LevelFile;

        private AssetReference SeaPrefab = new("P_Sea");

        private AssetReference HexGrid = new("P_HexGrid");

        private GameObject sea;
        private GameObject hexGrid;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            NetworkManager.Singleton.OnServerStarted += ServerStartHandler;
        }

        private void ServerStartHandler()
        {
            StartCoroutine(LoadAssets());
        }

        private IEnumerator LoadAssets()
        {
            yield return AssetLoader<GameObject>.Load(HexGrid, SpawnGrid);
            yield return AssetLoader<GameObject>.Load(SeaPrefab, SpawnSea);
        }

        private void SpawnSea(GameObject obj)
        {
            sea = Instantiate(obj);
            sea.GetComponent<NetworkObject>().Spawn();
        }

        private void SpawnGrid(GameObject obj)
        {
            hexGrid = Instantiate(obj);
            hexGrid.GetComponent<NetworkObject>().Spawn();
        }
    }
}
