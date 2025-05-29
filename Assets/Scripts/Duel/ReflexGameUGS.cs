using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudCode;
using UnityEngine;

namespace Duel
{
    public class ReflexGameUGS
    {
        private UnityGameServicesManager _ugsManager;
        private string _moduleName;
        
        public ReflexGameUGS(UnityGameServicesManager ugsManager)
        {
            _ugsManager = ugsManager;
            _ugsManager.ReflexGameUgs = this;
            _moduleName = _ugsManager.moduleName;
        }
        
        public async Task<ResultResponse> SubmitReflexResult(string sessionId, int reactionTimeMs)
        {
            Debug.Log($"Attempting to call {_moduleName}/SubmitReflexResult...");
            try
            {
                var parameters = new Dictionary<string, object>
                {
                    { "sessionId", sessionId },
                    { "reactionTimeMs", reactionTimeMs }
                };
        
                var result = await CloudCodeService.Instance.CallModuleEndpointAsync<ResultResponse>(
                    _moduleName,
                    "SubmitReflexResult",
                    parameters
                );
        
                Debug.Log($"Submitted reflex result - Time: {reactionTimeMs}ms, Winner: {result.winner}");
                return result;
            }
            catch (CloudCodeException e)
            {
                Debug.LogError($"Failed to submit reflex result (CloudCodeException): {e.Message} | Error Code: {e.ErrorCode} | Reason: {e.Reason}");
                Debug.LogError($"Details: {e}");
                return null;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to submit reflex result (System.Exception): {e.Message}");
                Debug.LogError($"Details: {e}");
                return null;
            }
        }

    }
    
    public class ResultResponse
    {
        public string winner;
        public int yourTime;
        public int opponentTime;
        public bool gameOver;
    }
}
