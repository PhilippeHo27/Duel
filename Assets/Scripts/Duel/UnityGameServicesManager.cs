using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.CloudCode;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Collections.Generic;
using Duel.Utilities;
using Newtonsoft.Json;
using Unity.Services.CloudCode.Subscriptions;

namespace Duel
{
    public class UnityGameServicesManager : IndestructibleSingletonBehaviour<UnityGameServicesManager>
    {
        [Header("References")]
        [SerializeField] HomeGUI homeGUI;
        [SerializeField] private ReflexGame reflexGameRef;

        [Header("Lobby Data")]
        [SerializeField] private string lobbyID;
        [SerializeField] private string lobbyCodeRef;
        [SerializeField] private string lobbyName;
        [SerializeField] private bool isHost;
        [SerializeField] private int playerCount;
        [SerializeField] private bool isInLobby;
        [SerializeField] private string moduleName = "Duel"; // this can be moved to some kind of global namespace const

        public string ModuleName { get => moduleName; set => moduleName = value; }
        public ReflexGame ReflexGameRef { get => reflexGameRef; set => reflexGameRef = value; }
        public string LobbyID { get => lobbyID; set => lobbyID = value; }
        public string LobbyCode { get => lobbyCodeRef; set => lobbyCodeRef = value; }
        public string LobbyName { get => lobbyName; set => lobbyName = value; }
        public bool IsHost { get => isHost; set => isHost = value; }
        public int PlayerCount { get => playerCount; set => playerCount = value; }
        public bool IsInLobby { get => isInLobby; set => isInLobby = value; }
        private void SetLobbyData(UgsResponse response)
        {
            if (response != null && response.Success)
            {
                lobbyID = response.LobbyID;
                lobbyCodeRef = response.LobbyCode;
                lobbyName = response.LobbyName;
                isHost = response.IsHost;
                playerCount = response.PlayerCount;
                isInLobby = true;
            }
        }

        public void ClearLobbyData()
        {
            lobbyID = null;
            lobbyCodeRef = null;
            lobbyName = null;
            isHost = false;
            playerCount = 0;
            isInLobby = false;
        }
        
        private ISubscriptionEvents _playerSubscription;

        void Start()
        {
            InitializeServicesAsync().ConfigureAwait(false);
        }
        
        private async Task InitializeServicesAsync()
        {
            try
            {
                await UnityServices.InitializeAsync();
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                homeGUI?.AddStatusMessage("Unity Services Initialized");
            }
            catch (Exception ex)
            {
                homeGUI?.AddStatusMessage($"Auth failed: {ex.Message}");
                Debug.LogError($"Failed to initialize Unity Services: {ex.Message}");
            }
        }
        

        private async Task SubscribeToPlayerMessages()
        {
            if (_playerSubscription != null)
            {
                await _playerSubscription.UnsubscribeAsync();
            }
            
            var callbacks = new SubscriptionEventCallbacks();
            callbacks.MessageReceived += @event =>
            {
                switch (@event.MessageType)
                {
                    case "playerJoined":
                        var matchData = JsonConvert.DeserializeObject(@event.Message);
                        OnMatchFound(matchData);
                        break;
                    case "reflexScoreUpdate":
                        var reflexData = JsonConvert.DeserializeObject(@event.Message);
                        if (reflexGameRef != null) reflexGameRef.OnOpponentScoreReceived(reflexData);
                        break;
                    default:
                        Debug.Log($"Got unsupported player message: {@event.MessageType}");
                        break;
                }
            };

            callbacks.ConnectionStateChanged += @event =>
            {
                Debug.Log($"Player subscription state changed: {@event}");
            };

            callbacks.Error += @event =>
            {
                Debug.LogError($"Player subscription error: {JsonConvert.SerializeObject(@event)}");
            };

            _playerSubscription = await CloudCodeService.Instance.SubscribeToPlayerMessagesAsync(callbacks);
        }
        
        public async Task<UgsResponse> HostLobby(string gameType)
        {
            Debug.Log($"Attempting to host lobby for {gameType}");
            try
            {
                var result = await CloudCodeService.Instance.CallModuleEndpointAsync<UgsResponse>(
                    ModuleName,
                    "HostLobby",
                    new Dictionary<string, object> { 
                        { "gameType", gameType },
                        { "username", PlayerPrefs.GetString("Username") }
                    }
                );

                SetLobbyData(result);
                await SubscribeToPlayerMessages();
                return result;
            }
            catch (CloudCodeException e)
            {
                Debug.LogError($"Failed to host lobby (CloudCodeException): {e.Message} | Error Code: {e.ErrorCode} | Reason: {e.Reason}");
                Debug.LogError($"Details: {e}");
                return null;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to host lobby (System.Exception): {e.Message}");
                Debug.LogError($"Details: {e}");
                return null;
            }
        }


        public async Task<UgsResponse> JoinLobby(string lobbyCode)
        {
            try
            {
                var result = await CloudCodeService.Instance.CallModuleEndpointAsync<UgsResponse>(
                    ModuleName,
                    "JoinLobby",
                    new Dictionary<string, object> 
                    { 
                        { "lobbyCode", lobbyCode },
                        { "userName", PlayerPrefs.GetString("Username" )}
                    }
                );
                
                SetLobbyData(result);
                await SubscribeToPlayerMessages();
                return result;
            }
            catch (CloudCodeException e)
            {
                Debug.LogError($"Failed to join lobby (CloudCodeException): {e.Message} | Error Code: {e.ErrorCode} | Reason: {e.Reason}");
                Debug.LogError($"Details: {e}");
                return null;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to join lobby (System.Exception): {e.Message}");
                Debug.LogError($"Details: {e}");
                return null;
            }
        }
        
        public async Task<UgsResponse> LeaveLobby(string lobbyId)
        {
            try
            {
                var result = await CloudCodeService.Instance.CallModuleEndpointAsync<UgsResponse>(
                    ModuleName,
                    "LeaveLobby",
                    new Dictionary<string, object> 
                    { 
                        { "lobbyId", lobbyId }
                    }
                );
                
                ClearLobbyData();
                return result;
            }
            catch (CloudCodeException e)
            {
                Debug.LogError($"Failed to leave lobby (CloudCodeException): {e.Message} | Error Code: {e.ErrorCode} | Reason: {e.Reason}");
                Debug.LogError($"Details: {e}");
                return null;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to leave lobby (System.Exception): {e.Message}");
                Debug.LogError($"Details: {e}");
                return null;
            }
        }
        
        public async Task<UgsResponse> QuickMatch(string gameType, int timeoutSeconds = 30)
        {
            homeGUI?.StartLoadingAnimation("Searching opponent");
            homeGUI?.SetQuickMatchButtonState(false);
            
            try
            {
                // Create timeout task
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(timeoutSeconds));
                var quickMatchTask = CloudCodeService.Instance.CallModuleEndpointAsync<UgsResponse>(
                    ModuleName,
                    "QuickMatch",
                    new Dictionary<string, object> { 
                        { "gameType", gameType },
                        { "username", PlayerPrefs.GetString("Username") }
                    }
                );

                // Wait for either completion or timeout
                var completedTask = await Task.WhenAny(quickMatchTask, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    homeGUI?.StopLoadingAnimation();
                    homeGUI?.UpdateLastStatusLine("Search timed out");
                    homeGUI?.SetQuickMatchButtonState(true);
                    return null;
                }

                // Get the result
                var result = await quickMatchTask;
                
                SetLobbyData(result);
                await SubscribeToPlayerMessages();

                if (result.IsHost)
                {
                    // Host waits for opponent - subscribe to notifications
                    homeGUI?.StopLoadingAnimation();
                    homeGUI?.UpdateLastStatusLine("Waiting for opponent...");
                }
                else
                {
                    // Joiner found match immediately
                    homeGUI?.StopLoadingAnimation();
                    homeGUI?.UpdateLastStatusLine("Found match!");
                    homeGUI?.HideQuickMatchButton();
                    StartGame(result);
                }

                return result;
            }
            catch (CloudCodeException e)
            {
                homeGUI?.StopLoadingAnimation();
                homeGUI?.UpdateLastStatusLine("Search failed");
                homeGUI?.SetQuickMatchButtonState(true);
                Debug.LogError($"Failed QuickMatch (CloudCodeException): {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                homeGUI?.StopLoadingAnimation();
                homeGUI?.UpdateLastStatusLine("Search failed");
                homeGUI?.SetQuickMatchButtonState(true);
                Debug.LogError($"Failed QuickMatch (Exception): {e.Message}");
                return null;
            }
        }
        
        private void OnMatchFound(object matchDataObj)
        {
            homeGUI?.StopLoadingAnimation();
            homeGUI?.UpdateLastStatusLine("Match found!");
            homeGUI?.HideQuickMatchButton();

            // Cast to JObject to access the data
            var matchData = (Newtonsoft.Json.Linq.JObject)matchDataObj;
    
            // Debug line to print everything in matchData:
            Debug.Log($"Full matchData content: {matchData}");
    
            // Or access individual properties:
            Debug.Log($"PlayerId: {matchData["PlayerId"]}");
            Debug.Log($"PlayerName: {matchData["PlayerName"]}");
            Debug.Log($"GameType: {matchData["GameType"]}");
        }


        private void StartGame(UgsResponse response)
        {

        }
    }
    
    public class UgsResponse
    {
        public string LobbyID { get; set; }
        public string LobbyCode { get; set; }
        public string LobbyName { get; set; }
        public bool IsHost { get; set;}
        public int PlayerCount { get; set;}
        public bool Success { get; set; }
    }
}
