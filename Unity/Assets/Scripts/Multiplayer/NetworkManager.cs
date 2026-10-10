using System;
using UnityEngine;

namespace BurgerRush.Multiplayer
{
    /// <summary>
    /// Ağ soyutlaması: Netcode for GameObjects veya Photon Fusion takılabilir (GDD 7 / 3.1).
    /// </summary>
    public class NetworkManager : Utils.Singleton<NetworkManager>
    {
        public enum Role { Offline, Host, Client }
        public enum ConnectionState { Disconnected, Connecting, Connected, Failed }

        public Role LocalRole { get; private set; } = Role.Offline;
        public ConnectionState State { get; private set; } = ConnectionState.Disconnected;

        public event Action<ConnectionState> OnStateChanged;

        public bool IsConnected => State == ConnectionState.Connected;

        public void StartHost()
        {
            LocalRole = Role.Host;
            SetState(ConnectionState.Connecting);
            // NetworkManager.Singleton.StartHost();
            SetState(ConnectionState.Connected);
        }

        public void StartClient()
        {
            LocalRole = Role.Client;
            SetState(ConnectionState.Connecting);
            // NetworkManager.Singleton.StartClient();
            SetState(ConnectionState.Connected);
        }

        public void Disconnect()
        {
            LocalRole = Role.Offline;
            // NetworkManager.Singleton.Shutdown();
            SetState(ConnectionState.Disconnected);
        }

        private void SetState(ConnectionState state)
        {
            State = state;
            OnStateChanged?.Invoke(state);
        }
    }
}
