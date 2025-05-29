using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.CloudCode;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Collections.Generic;
using Duel.Utilities;

namespace Duel
{
    public class UnityGameServicesManager : IndestructibleSingletonBehaviour<UnityGameServicesManager>
    {
        public string moduleName = "Duel";
        public ReflexGameUGS ReflexGameUgs { get; set; }
        
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
                Debug.Log("Unity Services Initialized and Signed In Anonymously.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to initialize Unity Services: {ex.Message}");
            }
        }
        
        public async Task<JoinGlobalLobbyResponse> JoinGlobalLobby(string username)
        {
            Debug.Log($"Attempting to call {moduleName}/JoinGlobalLobby with username: {username}");
            try
            {
                var result = await CloudCodeService.Instance.CallModuleEndpointAsync<JoinGlobalLobbyResponse>(
                    moduleName,
                    "JoinGlobalLobby",
                    new Dictionary<string, object> { { "username", username } }
                );
                Debug.Log($"Joined global lobby: {result.LobbyName} with {result.PlayerCount} players");
                return result;
            }
            catch (CloudCodeException e)
            {
                Debug.LogError($"Failed to join global lobby (CloudCodeException): {e.Message} | Error Code: {e.ErrorCode} | Reason: {e.Reason}");
                Debug.LogError($"Details: {e.ToString()}");
                return null;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to join global lobby (System.Exception): {e.Message}");
                Debug.LogError($"Details: {e.ToString()}");
                return null;
            }
        }
        
        public async Task<HostGameResponse> HostLobby()
        {
            Debug.Log($"Attempting to call {moduleName}/HostLobby...");
            try
            {
                var result = await CloudCodeService.Instance.CallModuleEndpointAsync<HostGameResponse>(
                    moduleName,
                    "HostLobby"
                );
                Debug.Log($"Hosted real Unity lobby with code: {result.lobbyCode}");
                return result;
            }
            catch (CloudCodeException e)
            {
                Debug.LogError($"Failed to host lobby (CloudCodeException): {e.Message} | Error Code: {e.ErrorCode} | Reason: {e.Reason}");
                Debug.LogError($"Details: {e.ToString()}");
                return null;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to host lobby (System.Exception): {e.Message}");
                Debug.LogError($"Details: {e.ToString()}");
                return null;
            }
        }

        public async Task<JoinGameResponse> JoinLobby(string lobbyCode)
        {
            Debug.Log($"Attempting to call {moduleName}/JoinLobby...");
            try
            {
                var result = await CloudCodeService.Instance.CallModuleEndpointAsync<JoinGameResponse>(
                    moduleName,
                    "JoinLobby",
                    new Dictionary<string, object> { { "lobbyCode", lobbyCode } }
                );
                Debug.Log($"Joined real Unity lobby: {result.session}");
                return result;
            }
            catch (CloudCodeException e)
            {
                Debug.LogError($"Failed to join lobby (CloudCodeException): {e.Message} | Error Code: {e.ErrorCode} | Reason: {e.Reason}");
                Debug.LogError($"Details: {e.ToString()}");
                return null;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to join lobby (System.Exception): {e.Message}");
                Debug.LogError($"Details: {e.ToString()}");
                return null;
            }
        }

        [Serializable]
        public class JoinGlobalLobbyResponse
        {
            public string LobbyId;
            public string LobbyName;
            public int PlayerCount;
            public bool Success;
        }

        [Serializable]
        public class HostGameResponse
        {
            public string lobbyCode;
        }

        [Serializable]
        public class JoinGameResponse
        {
            public string session;
            public string opponentId;
        }
    }
}
