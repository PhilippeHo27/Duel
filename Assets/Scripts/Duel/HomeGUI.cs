using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using TMPro;

namespace Duel
{
    public class HomeGUI : MonoBehaviour
    {
        public Button joinGlobalLobbyButton;
        public TMP_InputField usernameField;
        public TMP_Text confirmedJoinText;
        
        [Header("Minigame Buttons and Menu")]
        [SerializeField] private Button[] minigameButtons;
        [SerializeField] private MinigameList minigameList;
        [SerializeField] private GameObject minigameButtonPrefab; 
        [SerializeField] private Transform minigameGrid;
        
        [Header("Minigame Overlay")]
        [SerializeField] private Button backButton;
        [Header("Lobby Buttons")]
        [SerializeField] private Button hostButton;
        [SerializeField] private Button joinButton;
        [SerializeField] private Button quickMatchButton;
        [SerializeField] private TMP_InputField lobbyCodeInputField;
        [SerializeField] private TMP_Text searchingText;
        private enum LobbyState { Default, Hosting, Searching }

        [Header("Canvas Groups")]
        [SerializeField] private CanvasGroup welcomeGroup;
        [SerializeField] private CanvasGroup minigameGroup;
        [SerializeField] private CanvasGroup minigameOverlay;
        
        private CanvasGroup[] _allGroups;

        void Start()
        {
            _allGroups = new[] { welcomeGroup, minigameGroup, minigameOverlay };

            joinGlobalLobbyButton?.onClick.AddListener(() => OnJoinGlobalLobbyButtonClickAsync().ConfigureAwait(false));
            
            hostButton?.onClick.AddListener(() => OnHostButtonClickAsync().ConfigureAwait(false));
            joinButton?.onClick.AddListener(() => OnJoinButtonClickAsync().ConfigureAwait(false));
            quickMatchButton?.onClick.AddListener(() => OnQuickMatchButtonClickAsync().ConfigureAwait(false));
            
            PopulateMinigameButtons();
            backButton?.onClick.AddListener(BackButton);
        }

        private async Task OnJoinGlobalLobbyButtonClickAsync()
        {
            string username = usernameField?.text ?? "Player123";
            
            var response = await UnityGameServicesManager.Instance.JoinGlobalLobby(username);
            if (response?.success == true)
            {
                confirmedJoinText.text = $"Joined {response.lobbyName} as {username} ({response.playerCount} players)";
            }
            else
            {
                confirmedJoinText.text = "Failed to join global lobby";
            }
            
            SwitchToState(minigameGroup);
        }
        
        private void PopulateMinigameButtons()
        {
            // Create buttons for each minigame
            for (int i = 0; i < minigameList.miniGames.Length; i++)
            {
                GameObject buttonObj = Instantiate(minigameButtonPrefab, minigameGrid);
                Button button = buttonObj.GetComponent<Button>();
        
                string sceneName = minigameList.miniGames[i].sceneName;
        
                // Set button text
                TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
                if (buttonText != null)
                {
                    buttonText.text = sceneName;
                }
        
                button.onClick.AddListener(() => ChooseMinigame(sceneName));
            }
        }
        
        private void SwitchToState(CanvasGroup targetGroup)
        {
            foreach (var group in _allGroups)
            {
                bool isTarget = group == targetGroup;
                SetCanvasGroupActive(group, isTarget);
            }
            Canvas.ForceUpdateCanvases();
        }

        private void SetCanvasGroupActive(CanvasGroup canvasGroup, bool active)
        {
            canvasGroup.alpha = active ? 1f : 0f;
            canvasGroup.interactable = active;
            canvasGroup.blocksRaycasts = active;
        }
        
        private void HideAllGroups()
        {
            foreach (var group in _allGroups)
            {
                SetCanvasGroupActive(group, false);
            }
        }

        private void ChooseMinigame(string sceneName)
        {
            SwitchToState(minigameOverlay);
            SceneLoader.Instance.LoadSceneAdditive(sceneName);
        }

        private void BackButton()
        {
            SwitchToState(minigameGroup);
            SceneLoader.Instance.UnloadAllAdditiveScenes();
        }
        
        private async Task OnHostButtonClickAsync()
        {
            Debug.Log("Host button clicked");
    
            var response = await UnityGameServicesManager.Instance.HostLobby("reflex");
            if (response != null)
            {
                lobbyCodeInputField.text = response.lobbyCode;
                DataManager.Instance.lobbyName = $"SESSION_{response.lobbyCode}";
                SetLobbyState(LobbyState.Hosting);
            }
        }

        private async Task OnJoinButtonClickAsync()
        {
            Debug.Log("Join button clicked");
            string lobbyCode = lobbyCodeInputField?.text ?? "";
            if (string.IsNullOrEmpty(lobbyCode)) return;

            var response = await UnityGameServicesManager.Instance.JoinLobby(lobbyCode, "reflex");
            if (response?.session != null)
            {
                // confirmedJoinText.text = $"Joined lobby: {lobbyCode}";  // Add this if you have status text
                DataManager.Instance.lobbyName = response.session;
                // TODO: Set appropriate state after joining
            }
        }
        private async Task OnQuickMatchButtonClickAsync()
        {
            Debug.Log("Quick match button clicked");
            SetLobbyState(LobbyState.Searching);
    
            var response = await UnityGameServicesManager.Instance.QuickMatch("reflex");
            if (response != null)
            {
                DataManager.Instance.lobbyName = response.session;
        
                if (response.isHost)
                {
                    Debug.Log($"Created QuickMatch lobby: {response.session}, waiting for opponent");
                    // Maybe show "Waiting for opponent..." UI
                }
                else
                {
                    Debug.Log($"Joined QuickMatch lobby: {response.session}, opponent: {response.opponentId}");
                    // Both players connected, ready to start game
                }
            }
            else
            {
                Debug.LogError("QuickMatch failed");
                SetLobbyState(LobbyState.Default); // Reset UI on failure
            }
        }


        private void SetLobbyState(LobbyState state)
        {
            switch(state)
            {
                case LobbyState.Default:
                    hostButton.gameObject.SetActive(true);
                    lobbyCodeInputField.gameObject.SetActive(true);
                    joinButton.gameObject.SetActive(true);
                    lobbyCodeInputField.readOnly = false;
                    searchingText.gameObject.SetActive(false);
                    break;
            
                case LobbyState.Hosting:
                    hostButton.gameObject.SetActive(true);
                    lobbyCodeInputField.gameObject.SetActive(true);
                    joinButton.gameObject.SetActive(false);
                    lobbyCodeInputField.readOnly = true;
                    break;
            
                case LobbyState.Searching:
                    hostButton.gameObject.SetActive(false);
                    lobbyCodeInputField.gameObject.SetActive(false);
                    joinButton.gameObject.SetActive(false);
                    searchingText.gameObject.SetActive(true);
                    break;
            }
        }
        
        private void OnDestroy()
        {
            joinGlobalLobbyButton?.onClick.RemoveAllListeners();
        }
    }
}
