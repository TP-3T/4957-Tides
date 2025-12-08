using TTT.GameEvents;
using TTT.Helpers;
using UnityEngine;
using TMPro;

public class NetworkButton : MonoBehaviour
{
    [SerializeField]
    public GameEvent newMapEvent;

    [SerializeField]
    private MainMenu MainMenu;

    [SerializeField]
    private GameEvent startNetworkEvent;

    [SerializeField]
    private LobbyMenu lobbyMenu;

    /// <summary>
    /// Connects as client by extracting IP and port from lobbyMenu TextMeshPro components.
    /// </summary>
    public void connectClient()
    {
        // Get IP address from IpAddress GameObject
        string ipAddress = null;
        if (lobbyMenu.IpAddress != null)
        {
            var ipTmp = lobbyMenu.IpAddress.GetComponent<TextMeshProUGUI>();
            if (ipTmp != null)
            {
                string ipRaw = ipTmp.text;
                var ipBuilder = new System.Text.StringBuilder();
                for (int i = 0; i < ipRaw.Length; i++)
                {
                    char c = ipRaw[i];
                    if (char.IsDigit(c) || c == '.')
                    {
                        ipBuilder.Append(c);
                    }
                }
                ipAddress = ipBuilder.ToString().Trim();
            }
        }

        // Get port from Port GameObject
        ushort port = 0;
        if (lobbyMenu.Port != null)
        {
            var portTmp = lobbyMenu.Port.GetComponent<TextMeshProUGUI>();
            if (portTmp != null)
            {
                string portRaw = portTmp.text;
                var portBuilder = new System.Text.StringBuilder();
                for (int i = 0; i < portRaw.Length; i++)
                {
                    char c = portRaw[i];
                    if (char.IsDigit(c))
                    {
                        portBuilder.Append(c);
                    }
                }
                string cleanPort = portBuilder.ToString().Trim();
                ushort.TryParse(cleanPort, out port);
            }
        }

        // Debug.Log($"[NetworkButton] Connecting to {ipAddress}:{port}");
        StartClient(ipAddress, port);
    }

    /// <summary>
    /// Raises startNetworkEvent and starts as client with given IP and port.
    /// </summary>
    public void StartClient(string ip, ushort port)
    {
        if (startNetworkEvent != null)
        {
            var args = ScriptableObject.CreateInstance<StartNetworkEventArgs>();
            args.IsHost = false;
            args.Ip = ip;
            args.Port = port;
            startNetworkEvent.Raise(args);
        }
    }
    public void OnMapLoad()
    {
        gameObject.SetActive(false);
        
        // Load the map from the path specified in MainMenu.selectedMap
        if (!string.IsNullOrEmpty(MainMenu.selectedMap))
        {
            if (LoadExternalJson.TryGetMapJson(MainMenu.selectedMap, out TextAsset loadedMap))
            {
                newMapEvent.Raise(new NewMapEventArgs() { DataFile = loadedMap });
            }
            else
            {
                Debug.LogError($"[NetworkButton] Failed to load map from path: {MainMenu.selectedMap}");
            }
        }
        else
        {
            Debug.LogError("[NetworkButton] No map path selected in MainMenu");
        }
    }
}
