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
        private const string ModuleName = "Duel";

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

        public async Task<HostGameResponse> HostDuelGame()
        {
            Debug.Log($"Attempting to call {ModuleName}/HostDuelGame via CallModuleEndpointAsync...");
            try
            {
                var result = await CloudCodeService.Instance.CallModuleEndpointAsync<HostGameResponse>(
                    ModuleName,
                    "HostDuelGame"
                );
                Debug.Log($"Hosted game with lobby code: {result.lobbyCode}");
                return result;
            }
            catch (CloudCodeException e)
            {
                Debug.LogError($"Failed to host game (CloudCodeException): {e.Message} | Error Code: {e.ErrorCode} | Reason: {e.Reason}");
                Debug.LogError($"Details: {e.ToString()}");
                return null;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to host game (System.Exception): {e.Message}");
                Debug.LogError($"Details: {e.ToString()}");
                return null;
            }
        }

        public async Task<JoinGameResponse> JoinDuelGame(string lobbyCode)
        {
            Debug.Log($"Attempting to call {ModuleName}/JoinDuelGame...");
            try
            {
                // Updated to pass lobbyCode as parameter instead of dictionary
                var result = await CloudCodeService.Instance.CallModuleEndpointAsync<JoinGameResponse>(
                    ModuleName,
                    "JoinDuelGame",
                    new Dictionary<string, object> { { "lobbyCode", lobbyCode } }
                );
                Debug.Log($"Joined game: {result.session}");
                return result;
            }
            catch (CloudCodeException e)
            {
                Debug.LogError($"Failed to join game (CloudCodeException): {e.Message} | Error Code: {e.ErrorCode} | Reason: {e.Reason}");
                Debug.LogError($"Details: {e.ToString()}");
                return null;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to join game (System.Exception): {e.Message}");
                Debug.LogError($"Details: {e.ToString()}");
                return null;
            }
        }

        public async Task<HostGameResponse> HostLobby()
        {
            Debug.Log($"Attempting to call {ModuleName}/HostLobby...");
            try
            {
                var result = await CloudCodeService.Instance.CallModuleEndpointAsync<HostGameResponse>(
                    ModuleName,
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
            Debug.Log($"Attempting to call {ModuleName}/JoinLobby...");
            try
            {
                // Updated to pass lobbyCode as parameter instead of dictionary
                var result = await CloudCodeService.Instance.CallModuleEndpointAsync<JoinGameResponse>(
                    ModuleName,
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

        public async Task<ResultResponse> SubmitReflexTime(string session, int reactionTimeMs)
        {
            Debug.Log($"Attempting to call {ModuleName}/SubmitReflexTime...");
            try
            {
                var parameters = new Dictionary<string, object>
                {
                    { "session", session },
                    { "reactionTimeMs", reactionTimeMs }
                };
                var result = await CloudCodeService.Instance.CallModuleEndpointAsync<ResultResponse>(
                    ModuleName,
                    "SubmitReflexTime",
                    parameters
                );
                Debug.Log($"Submitted time: {reactionTimeMs}ms");
                return result;
            }
            catch (CloudCodeException e)
            {
                Debug.LogError($"Failed to submit time (CloudCodeException): {e.Message} | Error Code: {e.ErrorCode} | Reason: {e.Reason}");
                Debug.LogError($"Details: {e.ToString()}");
                return null;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to submit time (System.Exception): {e.Message}");
                Debug.LogError($"Details: {e.ToString()}");
                return null;
            }
        }

        public async Task<ResultResponse> SubmitReflexResult(string sessionId, int reactionTimeMs)
        {
            Debug.Log($"Attempting to call {ModuleName}/SubmitReflexResult...");
            try
            {
                var parameters = new Dictionary<string, object>
                {
                    { "sessionId", sessionId },
                    { "reactionTimeMs", reactionTimeMs }
                };
        
                var result = await CloudCodeService.Instance.CallModuleEndpointAsync<ResultResponse>(
                    ModuleName,
                    "SubmitReflexResult",
                    parameters
                );
        
                Debug.Log($"Submitted reflex result - Time: {reactionTimeMs}ms, Winner: {result.winner}");
                return result;
            }
            catch (CloudCodeException e)
            {
                Debug.LogError($"Failed to submit reflex result (CloudCodeException): {e.Message} | Error Code: {e.ErrorCode} | Reason: {e.Reason}");
                Debug.LogError($"Details: {e.ToString()}");
                return null;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to submit reflex result (System.Exception): {e.Message}");
                Debug.LogError($"Details: {e.ToString()}");
                return null;
            }
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

        [Serializable]
        public class ResultResponse
        {
            public string winner;
            public int yourTime;
            public int opponentTime;
            public bool gameOver;
        }
    }
}
