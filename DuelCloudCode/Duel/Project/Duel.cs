using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.Lobby.Model;
using Unity.Services.CloudSave.Model;

namespace Duel;

public class Duel
{
    // No constructor, no injection - just simple functions
    public Duel()
    {
    }

    [CloudCodeFunction("HostLobby")]
    public async Task<HostGameResponse> HostLobby(IExecutionContext context)
    {
        // Create client directly each time
        var gameApiClient = GameApiClient.Create();
        
        var lobbyResult = await gameApiClient.Lobby.CreateLobbyAsync(
            context, 
            context.AccessToken, 
            null, 
            null,
            new CreateRequest($"{context.PlayerId}'s Duel", 2, false, false, new Player(context.PlayerId))
        );
    
        // Initialize the reflex game data in Cloud Save when hosting
        string sessionId = $"SESSION_{lobbyResult.Data.LobbyCode}";
        string saveKey = "reflex_results";
    
        var initialGameData = new ReflexGameData
        {
            player1Id = "",
            player2Id = "",
            player1Time = 0,
            player2Time = 0
        };
    
        // Create the initial Cloud Save data structure
        await gameApiClient.CloudSaveData.SetCustomItemAsync(
            context, 
            context.ServiceToken, 
            context.ProjectId,
            sessionId,
            new SetItemBody(saveKey, System.Text.Json.JsonSerializer.Serialize(initialGameData))
        );
        
        return new HostGameResponse()
        {
            LobbyCode = lobbyResult.Data.LobbyCode,
        };
    }


    [CloudCodeFunction("JoinLobby")]
    public async Task<JoinGameResponse> JoinLobby(IExecutionContext context, string lobbyCode)
    {
        var gameApiClient = GameApiClient.Create();
        var pushClient = PushClient.Create();
            
        var joinLobbyResponse = await gameApiClient.Lobby.JoinLobbyByCodeAsync(
            context, 
            context.AccessToken,
            joinByCodeRequest: new JoinByCodeRequest(lobbyCode, new Player(context.PlayerId))
        );
            
        var lobbyData = joinLobbyResponse.Data;
        string opponentId = lobbyData.Players.First(p => p.Id != context.PlayerId).Id;
            
        await pushClient.SendPlayerMessageAsync(
            context,
            $"{{\"playerId\":\"{context.PlayerId}\"}}",
            "playerJoined",
            opponentId
        );
            
        return new JoinGameResponse()
        {
            Session = $"SESSION_{lobbyCode}",
            OpponentId = opponentId
        };
    }

    [CloudCodeFunction("SubmitReflexResult")]
    public async Task<ResultResponse> SubmitReflexResult(IExecutionContext context, string sessionId, int reactionTimeMs)
    {
        var gameApiClient = GameApiClient.Create();
        
        string saveKey = "reflex_results";
        
        // Try to get existing game data
        var saveResponse = await gameApiClient.CloudSaveData.GetCustomItemsAsync(
            context, 
            context.ServiceToken, 
            context.ProjectId,
            sessionId, 
            new List<string> { saveKey }
        );
        
        ReflexGameData gameData;
        if (saveResponse.Data?.Results?.Any() == true)
        {
            var result = saveResponse.Data.Results.Find(r => r.Key == saveKey);
            gameData = System.Text.Json.JsonSerializer.Deserialize<ReflexGameData>(result.Value.ToString());
        }
        else
        {
            gameData = new ReflexGameData();
        }
        
        // Store this player's result
        if (string.IsNullOrEmpty(gameData.player1Id))
        {
            gameData.player1Id = context.PlayerId;
            gameData.player1Time = reactionTimeMs;
        }
        else if (gameData.player1Id == context.PlayerId)
        {
            gameData.player1Time = reactionTimeMs;
        }
        else
        {
            gameData.player2Id = context.PlayerId;
            gameData.player2Time = reactionTimeMs;
        }
        
        // Save updated game data
        await gameApiClient.CloudSaveData.SetCustomItemAsync(
            context, 
            context.ServiceToken, 
            context.ProjectId,
            sessionId,
            new SetItemBody(saveKey, System.Text.Json.JsonSerializer.Serialize(gameData))
        );
        
        // Check if both players submitted
        if (gameData.player1Time > 0 && gameData.player2Time > 0)
        {
            // Determine winner
            string winner = "tie";
            if (gameData.player1Time < gameData.player2Time)
            {
                winner = gameData.player1Id == context.PlayerId ? "you" : "opponent";
            }
            else if (gameData.player2Time < gameData.player1Time)
            {
                winner = gameData.player2Id == context.PlayerId ? "you" : "opponent";
            }
            
            // Notify the other player
            var pushClient = PushClient.Create();
            string otherPlayerId = context.PlayerId == gameData.player1Id ? gameData.player2Id : gameData.player1Id;
            string otherPlayerWinner = winner == "you" ? "opponent" : (winner == "opponent" ? "you" : "tie");
            
            int otherPlayerTime = otherPlayerId == gameData.player1Id ? gameData.player1Time : gameData.player2Time;
            int currentPlayerTime = context.PlayerId == gameData.player1Id ? gameData.player1Time : gameData.player2Time;

            await pushClient.SendPlayerMessageAsync(
                context,
                $"{{\"winner\":\"{otherPlayerWinner}\",\"yourTime\":{otherPlayerTime},\"opponentTime\":{currentPlayerTime},\"gameOver\":true}}",
                messageType: "gameResult",
                playerId: otherPlayerId
            );
            
            return new ResultResponse
            {
                winner = winner,
                yourTime = reactionTimeMs,
                opponentTime = context.PlayerId == gameData.player1Id ? gameData.player2Time : gameData.player1Time,
                gameOver = true
            };
        }

        return new ResultResponse
        {
            winner = "waiting",
            yourTime = reactionTimeMs,
            opponentTime = 0,
            gameOver = false
        };
    }

}

public class HostGameResponse
{
    public string LobbyCode { get; set; }
}

public class JoinGameResponse
{
    public string Session { get; set; }
    public string OpponentId { get; set; }
}

public class ResultResponse
{
    public string winner { get; set; }
    public int yourTime { get; set; }
    public int opponentTime { get; set; }
    public bool gameOver { get; set; }
}

public class ReflexGameData
{
    public string player1Id { get; set; }
    public string player2Id { get; set; }
    public int player1Time { get; set; }
    public int player2Time { get; set; }
}
