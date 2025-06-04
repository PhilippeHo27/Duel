using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using System.Threading.Tasks;

namespace Duel.DebugScripts
{
    [System.Serializable]
    public class UnityServicesDebugInfo : MonoBehaviour
    {
        [Header("Unity Services Debug Info")]
        [Space(10)]
    
        [SerializeField] private string projectId = "Not Retrieved";
        [SerializeField] private string playerId = "Not Retrieved";
        [SerializeField] private string accessToken = "Not Retrieved";
    
        [Header("Authentication Status")]
        [SerializeField] private bool isSignedIn = false;
        [SerializeField] private bool sessionTokenExists = false;
        [SerializeField] private string sessionTokenStatus = "Not Retrieved";
        [SerializeField] private string playerName = "Not Retrieved";
    
        [Header("Additional Info")]
        [SerializeField] private string applicationVersion = "";
        [SerializeField] private string unityVersion = "";
        
        [Header("Runtime Controls")]
        [SerializeField] private bool autoRefresh = false;
        [SerializeField] private float refreshInterval = 2f;
        
        private float lastRefreshTime;

        async void Start()
        {
            SetConnectingStatus();
            await Task.Delay(1000); // Short delay before connecting
            await InitializeAndRefresh();
        }
        
        void Update()
        {
            if (autoRefresh && Time.time - lastRefreshTime > refreshInterval)
            {
                ManualRefresh();
                lastRefreshTime = Time.time;
            }
        }

        private void SetConnectingStatus()
        {
            projectId = "Connecting...";
            playerId = "Connecting...";
            accessToken = "Connecting...";
            isSignedIn = false;
            sessionTokenStatus = "Connecting...";
            playerName = "Connecting...";
        }

        public async Task InitializeAndRefresh()
        {
            try
            {
                Debug.Log("Initializing Unity Services...");
                
                if (UnityServices.State != ServicesInitializationState.Initialized)
                {
                    await UnityServices.InitializeAsync();
                    Debug.Log("Unity Services initialized successfully");
                }
                
                RefreshDebugInfo();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to initialize Unity Services: {ex.Message}");
                SetErrorStatus();
            }
        }

        private void SetErrorStatus()
        {
            projectId = "Connection Failed";
            playerId = "Connection Failed";
            accessToken = "Connection Failed";
            sessionTokenStatus = "Connection Failed";
            playerName = "Connection Failed";
        }
    
        public void RefreshDebugInfo()
        {
            try
            {
                applicationVersion = Application.version;
                unityVersion = Application.unityVersion;
                projectId = Application.cloudProjectId ?? "Not Available";
        
                if (UnityServices.State == ServicesInitializationState.Initialized && 
                    AuthenticationService.Instance != null)
                {
                    isSignedIn = AuthenticationService.Instance.IsSignedIn;
            
                    if (isSignedIn)
                    {
                        playerId = AuthenticationService.Instance.PlayerId ?? "Null";
                        accessToken = AuthenticationService.Instance.AccessToken ?? "Null";
                        sessionTokenExists = AuthenticationService.Instance.SessionTokenExists;
                        sessionTokenStatus = sessionTokenExists ? "Session Token Exists" : "No Session Token";
                        playerName = PlayerPrefs.GetString("Username", "No Username Set");
                    }
                    else
                    {
                        playerId = "Not Signed In";
                        accessToken = "Not Signed In";
                        sessionTokenExists = false;
                        sessionTokenStatus = "Not Signed In";
                        playerName = "Not Signed In";
                    }
                }
                else
                {
                    isSignedIn = false;
                    playerId = "Services Not Initialized";
                    accessToken = "Services Not Initialized";
                    sessionTokenExists = false;
                    sessionTokenStatus = "Services Not Initialized";
                    playerName = "Services Not Initialized";
                }
        
                Debug.Log("Unity Services Debug Info Refreshed");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to refresh debug info: {ex.Message}");
            }
        }

    
        public void ManualRefresh()
        {
            RefreshDebugInfo();
        }

        
        public void ToggleAutoRefresh()
        {
            autoRefresh = !autoRefresh;
            Debug.Log($"Auto-refresh {(autoRefresh ? "enabled" : "disabled")}");
        }
        
        [ContextMenu("Log All Values")]
        public void LogAllValues()
        {
            Debug.Log("=== UNITY SERVICES DEBUG INFO ===");
            Debug.Log($"Project ID: {projectId}");
            Debug.Log($"Player ID: {playerId}");
            Debug.Log($"Access Token: {accessToken}");
            Debug.Log($"Is Signed In: {isSignedIn}");
            Debug.Log($"Player Name: {playerName}");
            Debug.Log($"Application Version: {applicationVersion}");
            Debug.Log($"Unity Version: {unityVersion}");
            Debug.Log($"Session Token Exists: {sessionTokenExists}");
            Debug.Log($"Session Token Status: {sessionTokenStatus}");
            Debug.Log("=== END DEBUG INFO ===");
        }
        
        [ContextMenu("Copy Player ID")]
        public void CopyPlayerID()
        {
            GUIUtility.systemCopyBuffer = playerId;
            Debug.Log($"Copied Player ID: {playerId}");
        }
        
        [ContextMenu("Manual Refresh")]
        public void ContextMenuRefresh()
        {
            ManualRefresh();
        }
    }
}
