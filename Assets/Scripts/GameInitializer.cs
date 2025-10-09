using UnityEngine;
using Unity.Netcode;

public class GameInitializer : MonoBehaviour
{
    public NetworkObject HexGridPrefab;
    public NetworkObject SeaPrefab;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        NetworkManager.Singleton.OnServerStarted += SpawnGrid;

    }

    private void SpawnGrid()
    {
        NetworkObject gridInstance = Instantiate(HexGridPrefab);
        NetworkObject seaInstance = Instantiate(SeaPrefab);
        gridInstance.Spawn();
        seaInstance.Spawn();
    }
}
