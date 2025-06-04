
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.Services.Authentication;
using UnityEngine.UI;

namespace Duel
{
    public class GameLeaderboard : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform contentParent;
        [SerializeField] private GameObject leaderboardEntryPrefab;
        [SerializeField] private Button closeButton;
        
        [Header("Display Settings")]
        [SerializeField] private string scoreUnit = "ms";
        [SerializeField] private bool lowerIsBetter = true;
        
        private readonly List<LeaderboardEntry> _entryInstances = new List<LeaderboardEntry>();
        private readonly List<PlayerScore> _playerScores = new List<PlayerScore>();
        
        [System.Serializable]
        public class PlayerScore
        {
            public string playerId;
            public string playerName;
            public int score;
            public bool isCurrentPlayer;
            
            public PlayerScore(string id, string name, int playerScore, bool isCurrent = false)
            {
                playerId = id;
                playerName = name;
                score = playerScore;
                isCurrentPlayer = isCurrent;
            }
        }
        
        void Start()
        {
            //closeButton?.onClick.AddListener(Hide);
            //Hide(); // Start hidden
        }
        
        public void AddOrUpdatePlayerScore(string playerId, string playerName, int score)
        {
            var existingScore = _playerScores.FirstOrDefault(s => s.playerId == playerId);
            string currentPlayerId = AuthenticationService.Instance.PlayerId;
            bool isCurrentPlayer = playerId == currentPlayerId;
            
            if (existingScore != null)
            {
                // Update existing score if this one is better
                bool isBetter = lowerIsBetter ? score < existingScore.score : score > existingScore.score;
                
                if (isBetter)
                {
                    existingScore.score = score;
                }
            }
            else
            {
                // Add new score
                _playerScores.Add(new PlayerScore(playerId, playerName, score, isCurrentPlayer));
            }
            
            RefreshDisplay();
        }
        
        public void ClearAllScores()
        {
            _playerScores.Clear();
            RefreshDisplay();
        }
        
        public void RemovePlayer(string playerId)
        {
            _playerScores.RemoveAll(s => s.playerId == playerId);
            RefreshDisplay();
        }
        
        private void RefreshDisplay()
        {
            // Clear existing entries
            foreach (var entry in _entryInstances)
            {
                if (entry != null) Destroy(entry.gameObject);
            }
            _entryInstances.Clear();
            
            // Sort scores based on game type
            var sortedScores = lowerIsBetter 
                ? _playerScores.OrderBy(s => s.score).ToList()
                : _playerScores.OrderByDescending(s => s.score).ToList();
            
            // Create new entries
            for (int i = 0; i < sortedScores.Count; i++)
            {
                var score = sortedScores[i];
                var entryObj = Instantiate(leaderboardEntryPrefab, contentParent);
                var entryComponent = entryObj.GetComponent<LeaderboardEntry>();
                
                string scoreText = $"{score.score}{scoreUnit}";
                entryComponent.SetData(i + 1, score.playerName, scoreText, score.isCurrentPlayer);
                _entryInstances.Add(entryComponent);
            }
        }
        
        public void Show()
        {
            gameObject.SetActive(true);
        }
        
        public void Hide()
        {
            gameObject.SetActive(false);
        }
        
        public void Toggle()
        {
            if (gameObject.activeSelf)
                Hide();
            else
                Show();
        }
        
        // Get current standings for external use
        public List<PlayerScore> GetSortedScores()
        {
            return lowerIsBetter 
                ? _playerScores.OrderBy(s => s.score).ToList()
                : _playerScores.OrderByDescending(s => s.score).ToList();
        }
    }

}
