using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using TMPro; // Added for TextMeshPro support

namespace Duel
{
    public class HostGameTestButton : MonoBehaviour
    {
        public Button hostLobbyButton;
        public Button joinLobbyButton;
        public TMP_InputField inputField;
        public TMP_Text confirmedJoinText;

        void Start()
        {
            if (hostLobbyButton != null)
            {
                hostLobbyButton.onClick.AddListener(() => OnHostLobbyButtonClickAsync().ConfigureAwait(false));
            }
            
            if (joinLobbyButton != null)
            {
                joinLobbyButton.onClick.AddListener(() => OnJoinLobbyButtonClickAsync().ConfigureAwait(false));
            }
        }

        private async Task OnHostLobbyButtonClickAsync()
        {
            try
            {
                var response = await UnityGameServicesManager.Instance.HostLobby();

                if (response != null)
                {
                    Debug.Log($"Successfully hosted lobby! Lobby Code: {response.lobbyCode}");
                    if (inputField != null)
                    {
                        inputField.text = response.lobbyCode; // Good for display
                    }
                    // Store the correctly formatted session ID for Cloud Save operations
                    DataManager.Instance.lobbyName = $"SESSION_{response.lobbyCode}";
                    Debug.Log($"[Host] DataManager.Instance.lobbyName set to: {DataManager.Instance.lobbyName}");
                }
                else
                {
                    Debug.LogError("Failed to host lobby. Check console for errors from GameServicesManager.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error hosting lobby: {ex.Message}");
            }
        }
        private async Task OnJoinLobbyButtonClickAsync()
        {
            try
            {
                string lobbyCode = inputField != null ? inputField.text : "BUDDY123";

                if (string.IsNullOrEmpty(lobbyCode))
                {
                    Debug.LogError("Please enter a lobby code!");
                    return;
                }

                var response = await UnityGameServicesManager.Instance.JoinLobby(lobbyCode);

                if (response != null && !string.IsNullOrEmpty(response.session)) // Check response and session
                {
                    Debug.Log($"Successfully joined lobby with code: {lobbyCode}! Session ID: {response.session}");
                    confirmedJoinText.text = $"Congratulations, you joined lobby for code: {lobbyCode}!";
                    // Store the session ID received from the server (this is already "SESSION_{lobbyCode}")
                    DataManager.Instance.lobbyName = response.session;
                    Debug.Log($"[Join] DataManager.Instance.lobbyName set to: {DataManager.Instance.lobbyName}");
                }
                else
                {
                    Debug.LogError("Failed to join lobby or received invalid session. Check console for errors from GameServicesManager.");
                    confirmedJoinText.text = $"Failed to join lobby. Check console for errors from GameServicesManager.";
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error joining lobby: {ex.Message}");
            }
        }

        void OnDestroy()
        {
            if (hostLobbyButton != null)
            {
                hostLobbyButton.onClick.RemoveListener(() => OnHostLobbyButtonClickAsync().ConfigureAwait(false));
            }
            
            if (joinLobbyButton != null)
            {
                joinLobbyButton.onClick.RemoveListener(() => OnJoinLobbyButtonClickAsync().ConfigureAwait(false));
            }
        }
    }
}
