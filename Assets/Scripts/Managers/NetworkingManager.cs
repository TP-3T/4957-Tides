using System;
using TTT.Helpers;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEditor.PackageManager;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.Networking;

namespace TTT.Managers
{
    [RequireComponent(typeof(NetworkManager), typeof(UnityTransport))]
    public class NetworkingManager : GenericSingleton<NetworkingManager>
    {
        [SerializeField]
        private NetworkManager networkManager = NetworkManager.Singleton;

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
                unityTransport = new UnityTransport();
                networkManager.NetworkConfig.NetworkTransport = unityTransport;
            }
            networkManager.OnClientConnectedCallback += this.OnClientConnected;
        }

        private void OnClientConnected(ulong ClientId)
        {
            NetworkClient client = networkManager.ConnectedClients[ClientId];
            Console.Write(client);
        }
    }
}
