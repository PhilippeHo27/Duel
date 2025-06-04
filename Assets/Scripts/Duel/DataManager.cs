// using Duel.Utilities;
// using UnityEngine;
//
// namespace Duel
// {
//     public class DataManager : IndestructibleSingletonBehaviour<DataManager>
//     {
//         // [SerializeField] private string lobbyID;
//         // [SerializeField] private string lobbyCode;
//         // [SerializeField] private string lobbyName;
//         // [SerializeField] private bool isHost;
//         // [SerializeField] private int playerCount;
//         // [SerializeField] private bool isInLobby;
//         //
//         // // Public getters only
//         // public string LobbyID => lobbyID;
//         // public string LobbyCode => lobbyCode;
//         // public string LobbyName => lobbyName;
//         // public bool IsHost => isHost;
//         // public int PlayerCount => playerCount;
//         // public bool IsInLobby => isInLobby;
//         //
//         // // Method to update lobby data from UgsResponse
//         // public void SetLobbyData(UnityGameServicesManager.UgsResponse response)
//         // {
//         //     if (response != null && response.Success)
//         //     {
//         //         lobbyID = response.LobbyID;
//         //         lobbyCode = response.LobbyCode;
//         //         lobbyName = response.LobbyName;
//         //         isHost = response.IsHost;
//         //         playerCount = response.PlayerCount;
//         //         isInLobby = true;
//         //     }
//         // }
//         //
//         // // Method to clear lobby data when leaving
//         // public void ClearLobbyData()
//         // {
//         //     lobbyID = null;
//         //     lobbyCode = null;
//         //     lobbyName = null;
//         //     isHost = false;
//         //     playerCount = 0;
//         //     isInLobby = false;
//         // }
//     }
// }