using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using TMPro;
using DG.Tweening;
using UnityEngine.Serialization;

namespace Duel
{
    public class HomeGUI : MonoBehaviour
    {
        [FormerlySerializedAs("joinGlobalLobbyButton")] public Button confirmNameButton;
        public TMP_InputField usernameField;
        
        [Header("Minigame Buttons and Menu")]
        [SerializeField] private Button[] minigameButtons;
        [SerializeField] private MinigameList minigameList;
        [SerializeField] private GameObject minigameButtonPrefab; 
        [SerializeField] private Transform minigameGrid;
        
        [Header("Minigame Overlay")]
        [SerializeField] private Button backButton;
        [SerializeField] private Button miniGameMenuButton;
        
        [Header("Lobby Buttons")]
        [SerializeField] private Button hostButton;
        [SerializeField] private Button joinButton;
        [SerializeField] private Button copyButton;
        [SerializeField] private Button quickMatchButton;
        [SerializeField] private TMP_InputField lobbyCodeInputField;
        [SerializeField] private TMP_Text outputText;
        [SerializeField] private ScrollRect statusScrollRect;

        private enum LobbyState { Default, Hosting, Joined, Searching }
        private Sequence _loadingSequence;


        [Header("Canvas Groups")]
        [SerializeField] private CanvasGroup welcomeGroup;
        [SerializeField] private CanvasGroup minigameGroup;
        [SerializeField] private CanvasGroup minigameOverlay;
        
        private CanvasGroup[] _allGroups;

        void Start()
        {
            _allGroups = new[] { welcomeGroup, minigameGroup, minigameOverlay };
            SwitchToState(welcomeGroup);

            confirmNameButton?.onClick.AddListener(() => SwitchToState(minigameGroup));
            miniGameMenuButton?.onClick.AddListener(() => ReturnToMinigameMenu().ConfigureAwait(false));
            hostButton?.onClick.AddListener(() => OnHostButtonClickAsync().ConfigureAwait(false));
            joinButton?.onClick.AddListener(() => OnJoinButtonClickAsync().ConfigureAwait(false));
            quickMatchButton?.onClick.AddListener(() => OnQuickMatchButtonClickAsync().ConfigureAwait(false));
            
            PopulateMinigameButtons();
            backButton?.onClick.AddListener(BackButton);
            copyButton?.onClick.AddListener(() => GUIUtility.systemCopyBuffer = lobbyCodeInputField.text);

            
            string savedUsername = PlayerPrefs.GetString("Username", "abc123");
            usernameField.text = savedUsername;
            usernameField.onEndEdit.AddListener(OnUsernameChanged);
        }
        
        private void OnUsernameChanged(string newUsername)
        {
            if (!string.IsNullOrEmpty(newUsername))
            {
                PlayerPrefs.SetString("Username", newUsername);
                PlayerPrefs.Save();
            }
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
        
        // private void ReturnToMinigameMenu()
        // {
        //     // Unload current additive scene if one is loaded
        //     string currentScene = SceneLoader.Instance.CurrentAdditiveScene;
        //     if (!string.IsNullOrEmpty(currentScene))
        //     {
        //         SceneLoader.Instance.UnloadAdditiveScene(currentScene, () => {
        //             // After scene is unloaded, switch to minigame menu
        //             SwitchToState(minigameGroup);
        //         });
        //     }
        //     else
        //     {
        //         // No scene to unload, just switch to menu
        //         SwitchToState(minigameGroup);
        //     }
        // }

        private async Task ReturnToMinigameMenu()
        {
            // Always unload the current additive scene since this button only appears in minigames
            string currentScene = SceneLoader.Instance.CurrentAdditiveScene;
            
            // Leave lobby if we're in one - using the singleton!
            if (!string.IsNullOrEmpty(UnityGameServicesManager.Instance.LobbyID))
            {
                await UnityGameServicesManager.Instance.LeaveLobby(UnityGameServicesManager.Instance.LobbyID);
            }

    
            SceneLoader.Instance.UnloadAdditiveScene(currentScene, () => {
                // After scene is unloaded, reset everything and switch to minigame menu
                SetLobbyState(LobbyState.Default);
        
                // Clear the lobby code input field
                if (lobbyCodeInputField != null)
                {
                    lobbyCodeInputField.text = "";
                    lobbyCodeInputField.readOnly = false;
                }
        
                SwitchToState(minigameGroup);
            });
        }

        
        private async Task OnHostButtonClickAsync()
        {
            var response = await UnityGameServicesManager.Instance.HostLobby(SceneLoader.Instance.CurrentAdditiveScene);
            if (response != null)
            {
                Debug.Log(response.LobbyCode);
                lobbyCodeInputField.text = response.LobbyCode;
                SetLobbyState(LobbyState.Hosting);
                UnityGameServicesManager.Instance.ReflexGameRef.Ready();
            }
        }

        private async Task OnJoinButtonClickAsync()
        {
            string lobbyCode = lobbyCodeInputField?.text ?? "";
            if (string.IsNullOrEmpty(lobbyCode)) return;

            var response = await UnityGameServicesManager.Instance.JoinLobby(lobbyCode);
            if (response?.Success == true)
            {
                SetLobbyState(LobbyState.Joined);
                UnityGameServicesManager.Instance.ReflexGameRef.Ready();
            }
        }
        private async Task OnQuickMatchButtonClickAsync()
        {
            SetLobbyState(LobbyState.Searching);

            var response = await UnityGameServicesManager.Instance.QuickMatch(SceneLoader.Instance.CurrentAdditiveScene);
    
            if (response?.Success == true)
            {
                if (UnityGameServicesManager.Instance.IsHost)
                {
                    Debug.Log($"Created QuickMatch lobby: {UnityGameServicesManager.Instance.LobbyID}");
                    AddStatusMessage($"Created QuickMatch lobby: {UnityGameServicesManager.Instance.LobbyID}");
                }
                else
                {
                    Debug.Log($"Joined QuickMatch lobby: {UnityGameServicesManager.Instance.LobbyID}");
                    AddStatusMessage($"Joined QuickMatch lobby: {UnityGameServicesManager.Instance.LobbyID}");
                }
                UnityGameServicesManager.Instance.ReflexGameRef.Ready();
            }
            else
            {
                Debug.LogError("QuickMatch failed");
                SetLobbyState(LobbyState.Default);
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
                    copyButton.gameObject.SetActive(false);
                    break;
            
                case LobbyState.Hosting:
                    hostButton.gameObject.SetActive(true);
                    lobbyCodeInputField.gameObject.SetActive(true);
                    copyButton.gameObject.SetActive(true);
                    joinButton.gameObject.SetActive(false);
                    lobbyCodeInputField.readOnly = true;
                    break;
                
                case LobbyState.Joined:
                    hostButton.gameObject.SetActive(false);
                    lobbyCodeInputField.gameObject.SetActive(false);
                    copyButton.gameObject.SetActive(false);
                    joinButton.gameObject.SetActive(false);
                    lobbyCodeInputField.readOnly = true;
                    //setactive something that says " joined "
                    break;
            
                case LobbyState.Searching:
                    hostButton.gameObject.SetActive(false);
                    lobbyCodeInputField.gameObject.SetActive(false);
                    joinButton.gameObject.SetActive(false);
                    copyButton.gameObject.SetActive(false);
                    break;
            }
        }
        
        public void AddStatusMessage(string message)
        {
            outputText.text += $"\n{message}";
            Canvas.ForceUpdateCanvases();
            statusScrollRect.verticalNormalizedPosition = 0f;
        }
        
        public void SetQuickMatchButtonState(bool show)
        {
            quickMatchButton.interactable = show;
        }

        public void HideQuickMatchButton()
        {
            quickMatchButton.gameObject.SetActive(false);
        }
        
        #region  Animations

        public void UpdateStatusText(string message)
        {
            if (outputText != null)
            {
                AddStatusMessage(message);
            }
            else
            {
                Debug.LogWarning("Status text UI element is not assigned!");
            }
        }
        
        public void StartLoadingAnimation(string baseMessage)
        {
            StopLoadingAnimation();

            string[] dots = { "", ".", "..", "..." };
            _loadingSequence = DOTween.Sequence();

            AddStatusMessage(baseMessage);
    
            for (int i = 0; i < dots.Length; i++)
            {
                int currentIndex = i;
                _loadingSequence.AppendCallback(() => {
                    UpdateLastStatusLine(baseMessage + dots[currentIndex]);
                });
                _loadingSequence.AppendInterval(0.5f);
            }

            _loadingSequence.SetLoops(-1);
            _loadingSequence.Play();
        }
        
        public void UpdateLastStatusLine(string message)
        {
            string[] lines = outputText.text.Split('\n');
            if (lines.Length > 0)
            {
                lines[lines.Length - 1] = message;
                outputText.text = string.Join("\n", lines);
            }
        }
    
        public void StopLoadingAnimation()
        {
            if (_loadingSequence != null)
            {
                _loadingSequence.Kill();
                _loadingSequence = null;
            }
        }

        #endregion

        private void OnDestroy()
        {
            confirmNameButton?.onClick.RemoveAllListeners();
        }
    }
}
