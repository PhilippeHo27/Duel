using System;
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
    private static readonly Dictionary<string, Func<IExecutionContext, string, Task>> GameInitializers = new()
    {
        { "reflex", InitializeReflexGame }
    };
    
    [CloudCodeFunction("JoinGlobalLobby")]
    public async Task<JoinGlobalLobbyResponse> JoinGlobalLobby(IExecutionContext context, string username)    
    {
        var gameApiClient = GameApiClient.Create();
        
        await UpdatePlayerUsername(context, username);
        
        // Create player with username data
        var playerWithUsername = new Player(
            id: context.PlayerId!,
            data: new Dictionary<string, PlayerDataObject>
            {
                { "username", new PlayerDataObject(
                    value: username,
                    visibility: PlayerDataObject.VisibilityEnum.Member
                )}
            }
        );

        var queryResponse = await gameApiClient.Lobby.QueryLobbiesAsync(
            context,
            context.AccessToken,
            queryRequest: new QueryRequest
            {
                Filter = new List<QueryFilter>
                {
                    new (
                        field: QueryFilter.FieldEnum.Name,
                        value: "Global_1",
                        op: QueryFilter.OpEnum.EQ
                    ),
                    new (
                        field: QueryFilter.FieldEnum.AvailableSlots,
                        value: "0",
                        op: QueryFilter.OpEnum.GT
                    )
                },
                Order = new List<QueryOrder>
                {
                    new (
                        field: QueryOrder.FieldEnum.Created,
                        asc: true
                    )
                }
            }
        );
        
        // Try to join existing global lobby
        if (queryResponse.Data.Results.Any())
        {
            var globalLobby = queryResponse.Data.Results.First();
            try
            {
                var joinResponse = await gameApiClient.Lobby.JoinLobbyByIdAsync(
                    context,
                    context.AccessToken,
                    globalLobby.Id,
                    player: playerWithUsername  // Use playerWithUsername instead
                );
                
                return new JoinGlobalLobbyResponse
                {
                    LobbyId = globalLobby.Id,
                    LobbyName = globalLobby.Name,
                    PlayerCount = joinResponse.Data.Players.Count,
                    Success = true
                };
            }
            catch
            {
                // Lobby might have filled up, fall through to create new one
            }
        }
        
        // Create new lobby with username
        var createResponse = await gameApiClient.Lobby.CreateLobbyAsync(
            context,
            context.AccessToken,
            createRequest: new CreateRequest(
                name: "Global_1",
                maxPlayers: 150,
                isPrivate: false,
                isLocked: false,
                player: playerWithUsername  // Use playerWithUsername here too
            )
        );

        return new JoinGlobalLobbyResponse
        {
            LobbyId = createResponse.Data.Id,
            LobbyName = createResponse.Data.Name,
            PlayerCount = 1,
            Success = true
        };
    }

    [CloudCodeFunction("HostLobby")]
    public async Task<HostGameResponse> HostLobby(IExecutionContext context, string gameType)
    {
        var gameApiClient = GameApiClient.Create();

        if (context.PlayerId == null)
        {
            return new HostGameResponse { LobbyCode = null }; // or throw exception
        }

        var lobbyResult = await gameApiClient.Lobby.CreateLobbyAsync(
            context, 
            context.AccessToken, 
            null, 
            null,
            new CreateRequest($"{context.PlayerId}'s {gameType}", 2, false, false, new Player(context.PlayerId))
        );

        string sessionId = $"SESSION_{lobbyResult.Data.LobbyCode}";

        if (GameInitializers.TryGetValue(gameType.ToLower(), out var initializer))
        {
            await initializer(context, sessionId);
        }

        return new HostGameResponse()
        {
            LobbyCode = lobbyResult.Data.LobbyCode,
        };
    }
    
    [CloudCodeFunction("JoinLobby")]
    public async Task<JoinGameResponse> JoinLobby(IExecutionContext context, string lobbyCode, string gameType)
    {
        var gameApiClient = GameApiClient.Create();

        if (context.PlayerId == null)
        {
            return new JoinGameResponse { Session = null, OpponentId = null };
        }

        var result = await gameApiClient.Lobby.JoinLobbyByCodeAsync(
            context,
            context.AccessToken,
            joinByCodeRequest: new JoinByCodeRequest(lobbyCode, new Player(context.PlayerId))
        );

        string sessionId = $"SESSION_{lobbyCode}";

        // Initialize game-specific data (same function as HostLobby)
        if (GameInitializers.TryGetValue(gameType.ToLower(), out var initializer))
        {
            await initializer(context, sessionId);
        }

        return new JoinGameResponse
        {
            Session = result.Data.Id,
            OpponentId = result.Data.Players.FirstOrDefault(p => p.Id != context.PlayerId)?.Id
        };
    }
    
    [CloudCodeFunction("QuickMatch")]
    public async Task<QuickMatchResponse> QuickMatch(IExecutionContext context, string gameType)
    {
        var gameApiClient = GameApiClient.Create();

        if (context.PlayerId == null)
        {
            return new QuickMatchResponse { Session = null, OpponentId = null, IsHost = false };
        }

        // Look for existing lobbies with 1 player waiting for QuickMatch of the same game type
        var queryResponse = await gameApiClient.Lobby.QueryLobbiesAsync(
            context,
            context.AccessToken,
            queryRequest: new QueryRequest
            {
                Filter = new List<QueryFilter>
                {
                    new (field: QueryFilter.FieldEnum.AvailableSlots, value: "1", op: QueryFilter.OpEnum.EQ),
                    new (field: QueryFilter.FieldEnum.MaxPlayers, value: "2", op: QueryFilter.OpEnum.EQ),
                    new (field: QueryFilter.FieldEnum.IsLocked, value: "false", op: QueryFilter.OpEnum.EQ),
                    new (field: QueryFilter.FieldEnum.S1, value: gameType, op: QueryFilter.OpEnum.EQ) // Filter by game type
                }
            }
        );

        // Try to join existing QuickMatch lobby
        if (queryResponse.Data.Results.Any())
        {
            var availableLobby = queryResponse.Data.Results.First();
            var joinResponse = await gameApiClient.Lobby.JoinLobbyByIdAsync(
                context, context.AccessToken, availableLobby.Id, player: new Player(context.PlayerId)
            );

            string sessionId = $"SESSION_{availableLobby.LobbyCode}";
            
            // Initialize game-specific data
            if (GameInitializers.TryGetValue(gameType.ToLower(), out var initializer))
            {
                await initializer(context, sessionId);
            }

            string? opponentId = joinResponse.Data.Players.FirstOrDefault(p => p.Id != context.PlayerId)?.Id;
            return new QuickMatchResponse { Session = sessionId, OpponentId = opponentId, IsHost = false };
        }

        // Create new QuickMatch lobby and wait
        var createResponse = await gameApiClient.Lobby.CreateLobbyAsync(
            context, context.AccessToken,
            createRequest: new CreateRequest(
                name: $"QuickMatch_{gameType}_{context.PlayerId}", 
                maxPlayers: 2, 
                isPrivate: false, 
                player: new Player(context.PlayerId),
                data: new Dictionary<string, DataObject>
                {
                    ["S1"] = new (gameType, DataObject.VisibilityEnum.Public)
                }
            )
        );

        string newSessionId = $"SESSION_{createResponse.Data.LobbyCode}";
        
        // Initialize game-specific data for new lobby
        if (GameInitializers.TryGetValue(gameType.ToLower(), out var newInitializer))
        {
            await newInitializer(context, newSessionId);
        }

        return new QuickMatchResponse { Session = newSessionId, OpponentId = null, IsHost = true };
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
        if (saveResponse.Data.Results.Any())
        {
            var result = saveResponse.Data.Results.Find(r => r.Key == saveKey);
            gameData = System.Text.Json.JsonSerializer.Deserialize<ReflexGameData>(result.Value.ToString());
        }
        else
        {
            gameData = new ReflexGameData();
        }
        
        // Store this player's result
        if (string.IsNullOrEmpty(gameData.Player1Id))
        {
            gameData.Player1Id = context.PlayerId;
            gameData.Player1Time = reactionTimeMs;
        }
        else if (gameData.Player1Id == context.PlayerId)
        {
            gameData.Player1Time = reactionTimeMs;
        }
        else
        {
            gameData.Player2Id = context.PlayerId;
            gameData.Player2Time = reactionTimeMs;
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
        if (gameData.Player1Time > 0 && gameData.Player2Time > 0)
        {
            // Determine winner
            string winner = "tie";
            if (gameData.Player1Time < gameData.Player2Time)
            {
                winner = gameData.Player1Id == context.PlayerId ? "you" : "opponent";
            }
            else if (gameData.Player2Time < gameData.Player1Time)
            {
                winner = gameData.Player2Id == context.PlayerId ? "you" : "opponent";
            }
            
            // Notify the other player
            var pushClient = PushClient.Create();
            string? otherPlayerId = context.PlayerId == gameData.Player1Id ? gameData.Player2Id : gameData.Player1Id;
            string otherPlayerWinner = winner == "you" ? "opponent" : (winner == "opponent" ? "you" : "tie");
            
            int otherPlayerTime = (otherPlayerId == gameData.Player1Id ? gameData.Player1Time : gameData.Player2Time);
            int currentPlayerTime = (context.PlayerId == gameData.Player1Id ? gameData.Player1Time : gameData.Player2Time);
            
            await pushClient.SendPlayerMessageAsync(
                context,
                $"{{\"winner\":\"{otherPlayerWinner}\",\"yourTime\":{otherPlayerTime},\"opponentTime\":{currentPlayerTime},\"gameOver\":true}}",
                messageType: "gameResult",
                playerId: otherPlayerId
            );
            
            return new ResultResponse
            {
                Winner = winner,
                YourTime = reactionTimeMs,
                OpponentTime = context.PlayerId == gameData.Player1Id ? gameData.Player2Time : gameData.Player1Time,
                GameOver = true
            };
        }

        return new ResultResponse
        {
            Winner = "waiting",
            YourTime = reactionTimeMs,
            OpponentTime = 0,
            GameOver = false
        };
    }

    #region PrivateMethods

    private async Task UpdatePlayerUsername(IExecutionContext context, string username)
    {
        var gameApiClient = GameApiClient.Create();
    
        // Unity Cloud Save handles atomic operations PER PLAYER
        // Each player's data is separate - no cross-player conflicts
        await gameApiClient.CloudSaveData.SetCustomItemAsync(
            context,
            context.ServiceToken,
            context.ProjectId,
            context.PlayerId,  // Key = player's unique ID
            new SetItemBody("username", username)
        );
    }

    private static async Task InitializeReflexGame(IExecutionContext context, string sessionId)
    {
        var gameApiClient = GameApiClient.Create();
        var initialGameData = new ReflexGameData
        {
            Player1Id = "",
            Player2Id = "",
            Player1Time = 0,
            Player2Time = 0
        };
    
        await gameApiClient.CloudSaveData.SetCustomItemAsync(
            context, context.ServiceToken, context.ProjectId, sessionId,
            new SetItemBody("reflex_results", System.Text.Json.JsonSerializer.Serialize(initialGameData))
        );
    }

    #endregion

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

public class QuickMatchResponse
{
    public string Session { get; set; }
    public string OpponentId { get; set; }
    public bool IsHost { get; set; }
}


public class ResultResponse
{
    public string Winner { get; set; }
    public int YourTime { get; set; }
    public int OpponentTime { get; set; }
    public bool GameOver { get; set; }
}

public class ReflexGameData
{
    public string Player1Id { get; set; }
    public string Player2Id { get; set; }
    public int Player1Time { get; set; }
    public int Player2Time { get; set; }
}

public class JoinGlobalLobbyResponse
{
    public string LobbyId { get; set; }
    public string LobbyName { get; set; }
    public int PlayerCount { get; set; }
    public bool Success { get; set; }
}
