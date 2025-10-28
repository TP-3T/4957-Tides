using System;
using TTT.Helpers;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Networking;

namespace TTT.Managers
{
    [RequireComponent(typeof(NetworkManager), typeof(UnityTransport))]
    public class NetworkingManager : GenericSingleton<NetworkingManager>
    {
        [SerializeField]
        private NetworkManager networkManager;

        [SerializeField]
        private UnityTransport unityTransport;

        public override void Awake()
        {
            base.Awake();
            if (networkManager == null)
            {
                networkManager = NetworkManager.Singleton;
            }
            if (unityTransport == null)
            {
                unityTransport = networkManager.GetComponent<UnityTransport>();
            }
            networkManager.OnClientConnectedCallback += this.OnClientConnected;
        }

        private void OnClientConnected(ulong ClientId)
        {
            NetworkClient client = networkManager.ConnectedClients[ClientId];
            Console.Write(client);
        }

        // // Start is called once before the first execution of Update after the MonoBehaviour is created
        // void Start()
        // {
        //     networkManager.StartHost();
        // }

        // Update is called once per frame
        void Update() { }
    }
}
