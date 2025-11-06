using Unity.Netcode;
using UnityEngine;

//TODO CB: We should delete this file. It's responsibilities will be handled by other managers.
namespace TTT.Managers
{
    public class GameInitializer : MonoBehaviour
    {
        public NetworkObject HexGridPrefab;
        public NetworkObject SeaPrefab;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            NetworkManager.Singleton.OnServerStarted += SpawnGrid;
        }

        private void SpawnGrid()
        {
            // NetworkObject gridInstance = Instantiate(HexGridPrefab);
            // NetworkObject seaInstance = Instantiate(SeaPrefab);
            // gridInstance.Spawn();
            // seaInstance.Spawn();
        }
    }
}
