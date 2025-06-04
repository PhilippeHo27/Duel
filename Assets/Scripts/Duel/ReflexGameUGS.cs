using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudCode;
using UnityEngine;

namespace Duel
{
    public class ReflexGameUGS
    {
        private readonly string _moduleName = "Duel";
        
        public async Task SubmitReflexResult(string lobbyId, int reactionTimeMs, string playerNameFromClient)
        {
            var parameters = new Dictionary<string, object>
            {
                { "lobbyId", lobbyId },
                { "reactionTimeMs", reactionTimeMs },
                { "playerName", playerNameFromClient }

            };

            await CloudCodeService.Instance.CallModuleEndpointAsync(
                _moduleName,
                "SubmitReflexResult", 
                parameters
            );
    
            Debug.Log($"Score submitted: {reactionTimeMs}ms");
        }
    }
}
