using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.Lobby.Model;
using Unity.Services.CloudSave.Model;
using Newtonsoft.Json;

namespace Duel;

public class Duel
{
    private readonly IPushClient _pushClient = PushClient.Create();
    // private static readonly Dictionary<string, Func<IExecutionContext, string, Task>> GameInitializers = new() { { "reflex", InitializeReflexGame } };

    [CloudCodeFunction("HostLobby")]
    public async Task<UgsResponse> HostLobby(IExecutionContext context, string gameType, string userName)
    {
        var gameApiClient = GameApiClient.Create();

        var response = await gameApiClient.Lobby.CreateLobbyAsync(
            context,
            context.AccessToken,
            createRequest: new CreateRequest(
                name: $"{gameType}Game initiated by {userName}",
                maxPlayers: 150,
                isPrivate: false,
                isLocked: false,
                player: CreatePlayerProfile(context, userName),
                data: new Dictionary<string, DataObject>
                {
                    {
                        "S1", new DataObject(
                            value: gameType,
                            visibility: DataObject.VisibilityEnum.Public,
                            index: DataObject.IndexEnum.S1
                        )
                    }
                }
            )
        );

        return new UgsResponse
        {
            LobbyID = response.Data.Id,
            LobbyCode = response.Data.LobbyCode,
            LobbyName = response.Data.Name,
            IsHost = true,
            PlayerCount = response.Data.Players.Count,
            Success = true
        };
    }

    [CloudCodeFunction("JoinLobby")]
    public async Task<UgsResponse> JoinLobby(IExecutionContext context, string lobbyCode, string userName)
    {
        var gameApiClient = GameApiClient.Create();

        var response = await gameApiClient.Lobby.JoinLobbyByCodeAsync(
            context,
            context.AccessToken,
            joinByCodeRequest: new JoinByCodeRequest(lobbyCode, CreatePlayerProfile(context, userName))
        );

        return new UgsResponse
        {
            LobbyID = response.Data.Id,
            LobbyCode = response.Data.LobbyCode,
            LobbyName = response.Data.Name,
            IsHost = response.Data.HostId == context.PlayerId,
            PlayerCount = response.Data.Players.Count,
            Success = true
        };
    }
    
    [CloudCodeFunction("LeaveLobby")]
    public async Task<UgsResponse> LeaveLobby(IExecutionContext context, string lobbyId)
    {
        var gameApiClient = GameApiClient.Create();

        await gameApiClient.Lobby.RemovePlayerAsync(
            context,
            context.AccessToken,
            lobbyId,
            context.PlayerId!
        );

        return new UgsResponse
        {
            Success = true
        };
    }
    
    [CloudCodeFunction("QuickMatch")]
    public async Task<UgsResponse> QuickMatch(IExecutionContext context, string gameType, string userName)
    {
        var gameApiClient = GameApiClient.Create();
        var playerWithUsername = CreatePlayerProfile(context, userName); 

        // Look for existing lobbies
        var response = await gameApiClient.Lobby.QueryLobbiesAsync(
            context,
            context.AccessToken,
            queryRequest: new QueryRequest
            {
                Filter = new List<QueryFilter>
                {
                    // new (field: QueryFilter.FieldEnum.S1, value: gameType, op: QueryFilter.OpEnum.CONTAINS),
                    new(field: QueryFilter.FieldEnum.AvailableSlots, value: "0", op: QueryFilter.OpEnum.GT),
                    new(field: QueryFilter.FieldEnum.IsLocked, value: "false", op: QueryFilter.OpEnum.EQ)
                }
            }
        );

        if (response.Data.Results.Any())
        {
            // Join an existing lobby
            var availableLobby = response.Data.Results.First();

            var joinResponse = await gameApiClient.Lobby.JoinLobbyByIdAsync(
                context, context.AccessToken, availableLobby.Id,
                player: playerWithUsername
            );

            var playerJoinedData = new
            {
                PlayerId = context.PlayerId,
                PlayerName = userName,
                GameType = gameType // Just add this one line
            };

            // Get all players except yourself
            var existingPlayerIds = joinResponse.Data.Players
                .Where(p => p.Id != context.PlayerId)
                .Select(p => p.Id);

            foreach (string playerId in existingPlayerIds)
            {
                await _pushClient.SendPlayerMessageAsync(
                    context,
                    JsonConvert.SerializeObject(playerJoinedData),
                    "playerJoined",
                    playerId
                );
            }

            return new UgsResponse
            {
                LobbyID = joinResponse.Data.Id,
                LobbyCode = joinResponse.Data.LobbyCode,
                LobbyName = joinResponse.Data.Name,
                IsHost = joinResponse.Data.HostId == context.PlayerId,
                PlayerCount = joinResponse.Data.Players.Count,
                Success = true
            };
        }

        // Create new lobby if we can't join one
        var createResponse = await gameApiClient.Lobby.CreateLobbyAsync(
            context,
            context.AccessToken,
            createRequest: new CreateRequest(
                name: $"{gameType}Game initiated by {userName}",
                maxPlayers: 150,
                isPrivate: false,
                isLocked: false,
                player: playerWithUsername,
                data: new Dictionary<string, DataObject>
                {
                    {
                        "S1", new DataObject(
                            value: gameType,
                            visibility: DataObject.VisibilityEnum.Public,
                            index: DataObject.IndexEnum.S1
                        )
                    }
                }
            )
        );

        return new UgsResponse
        {
            LobbyID = createResponse.Data.Id,
            LobbyCode = createResponse.Data.LobbyCode,
            LobbyName = createResponse.Data.Name,
            IsHost = createResponse.Data.HostId == context.PlayerId,
            PlayerCount = createResponse.Data.Players.Count,
            Success = true
        };
    }

    [CloudCodeFunction("SubmitReflexResult")]
    public async Task SubmitReflexResult(IExecutionContext context, string lobbyId, int reactionTimeMs, string playerName)        
    {
        var gameApiClient = GameApiClient.Create();

        // Get lobby info 
        var lobbyResponse = await gameApiClient.Lobby.GetLobbyAsync(context, context.AccessToken, lobbyId);
        
        // Get ALL other players (everyone except current player)
        var otherPlayerIds = lobbyResponse.Data.Players
            .Where(p => p.Id != context.PlayerId)
            .Select(p => p.Id);

        // Create score data to broadcast
        var scoreData = new 
        {
            PlayerId = context.PlayerId!,
            PlayerName = playerName,
            ReactionTimeMs = reactionTimeMs
        };
        
        // Broadcast to all other players in the lobby
        foreach (var playerId in otherPlayerIds)
        {
            await _pushClient.SendPlayerMessageAsync(
                context,
                JsonConvert.SerializeObject(scoreData),
                "reflexScoreUpdate",
                playerId
            );
        }
    }

    private Player CreatePlayerProfile(IExecutionContext context, string username)
    {
        return new Player(
            id: context.PlayerId!,
            data: new Dictionary<string, PlayerDataObject>
            {
                {
                    "username", new PlayerDataObject(
                        value: username,
                        visibility: PlayerDataObject.VisibilityEnum.Member
                    )
                }
            }
        );
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
