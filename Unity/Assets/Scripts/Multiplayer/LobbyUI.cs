using UnityEngine;
using UnityEngine.UI;

namespace BurgerRush.Multiplayer
{
    /// <summary>17_CoopLobby sahnesinin arayüzü: host ol / katıl / hazır.</summary>
    public class LobbyUI : MonoBehaviour
    {
        [SerializeField] private Button hostButton;
        [SerializeField] private Button joinButton;
        [SerializeField] private Button leaveButton;
        [SerializeField] private Text statusText;
        [SerializeField] private string gameplayScene = "18_Gameplay_Coop";

        private void Awake()
        {
            if (hostButton) hostButton.onClick.AddListener(OnHost);
            if (joinButton) joinButton.onClick.AddListener(OnJoin);
            if (leaveButton) leaveButton.onClick.AddListener(OnLeave);
        }

        private void OnEnable() => NetworkManager.Instance.OnStateChanged += HandleState;
        private void OnDisable() { if (NetworkManager.Instance) NetworkManager.Instance.OnStateChanged -= HandleState; }

        private void OnHost() => NetworkManager.Instance.StartHost();
        private void OnJoin() => NetworkManager.Instance.StartClient();
        private void OnLeave() => NetworkManager.Instance.Disconnect();

        private void HandleState(NetworkManager.ConnectionState state)
        {
            if (statusText) statusText.text = state switch
            {
                NetworkManager.ConnectionState.Connected => "Bağlı — Co-op başlatılıyor…",
                NetworkManager.ConnectionState.Connecting => "Bağlanıyor…",
                NetworkManager.ConnectionState.Failed => "Bağlantı başarısız",
                _ => "Bağlı değil",
            };

            if (state == NetworkManager.ConnectionState.Connected)
                Core.SceneLoader.Load(gameplayScene);
        }
    }
}
