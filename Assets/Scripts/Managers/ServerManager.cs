using System;
using TTT.Helpers;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace TTT.Managers
{
    [RequireComponent(typeof(NetworkManager), typeof(UnityTransport))]
    public class ServerManager : GenericSingleton<ServerManager>
    {
        [SerializeField]
        private NetworkManager networkManager = NetworkManager.Singleton;

        public override void Awake()
        {
            base.Awake();
            if (networkManager == null)
            {
                networkManager = NetworkManager.Singleton;
            }
            if (networkManager.NetworkConfig.NetworkTransport == null)
            {
                networkManager.NetworkConfig.NetworkTransport = new UnityTransport();
            }
            networkManager.OnClientConnectedCallback += this.OnClientConnected;
        }

        private void OnClientConnected(ulong ClientId)
        {
            NetworkClient client = networkManager.ConnectedClients[ClientId];
            Debug.Log(client);
        }
    }
}
