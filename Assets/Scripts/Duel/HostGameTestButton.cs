using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using TMPro;

namespace Duel
{
    public class HostGameTestButton : MonoBehaviour
    {
        public Button hostLobbyButton;
        public Button joinLobbyButton;
        public Button joinGlobalLobbyButton;
        public TMP_InputField inputField;
        public TMP_InputField usernameField;
        public TMP_Text confirmedJoinText;

        void Start()
        {
            hostLobbyButton?.onClick.AddListener(() => OnHostLobbyButtonClickAsync().ConfigureAwait(false));
            joinLobbyButton?.onClick.AddListener(() => OnJoinLobbyButtonClickAsync().ConfigureAwait(false));
            joinGlobalLobbyButton?.onClick.AddListener(() => OnJoinGlobalLobbyButtonClickAsync().ConfigureAwait(false));
        }

        private async Task OnHostLobbyButtonClickAsync()
        {
            var response = await UnityGameServicesManager.Instance.HostLobby();
            if (response != null)
            {
                inputField.text = response.lobbyCode;
                DataManager.Instance.lobbyName = $"SESSION_{response.lobbyCode}";
            }
        }

        private async Task OnJoinLobbyButtonClickAsync()
        {
            string lobbyCode = inputField?.text;
            if (string.IsNullOrEmpty(lobbyCode)) return;

            var response = await UnityGameServicesManager.Instance.JoinLobby(lobbyCode);
            if (response?.session != null)
            {
                confirmedJoinText.text = $"Joined lobby: {lobbyCode}";
                DataManager.Instance.lobbyName = response.session;
            }
            else
            {
                confirmedJoinText.text = "Failed to join lobby";
            }
        }

        private async Task OnJoinGlobalLobbyButtonClickAsync()
        {
            string username = usernameField?.text ?? "Player123";
            
            var response = await UnityGameServicesManager.Instance.JoinGlobalLobby(username);
            if (response?.Success == true)
            {
                confirmedJoinText.text = $"Joined {response.LobbyName} as {username} ({response.PlayerCount} players)";
            }
            else
            {
                confirmedJoinText.text = "Failed to join global lobby";
            }
        }

        void OnDestroy()
        {
            hostLobbyButton?.onClick.RemoveAllListeners();
            joinLobbyButton?.onClick.RemoveAllListeners();
            joinGlobalLobbyButton?.onClick.RemoveAllListeners();
        }
    }
}
