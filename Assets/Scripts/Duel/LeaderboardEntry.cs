using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Duel
{
    public class LeaderboardEntry : MonoBehaviour
    {
        [SerializeField] private TMP_Text rankText;
        [SerializeField] private TMP_Text playerNameText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private Image backgroundImage;
    
        public void SetData(int rank, string playerName, string scoreDisplay, bool isCurrentPlayer = false)
        {
            rankText.text = $"#{rank}";
            playerNameText.text = playerName;
            scoreText.text = scoreDisplay; // Now accepts the formatted string
        
            // Highlight current player
            if (isCurrentPlayer)
            {
                backgroundImage.color = new Color32(255, 255, 0, 50); // Light yellow
            }
            else
            {
                backgroundImage.color = Color.clear; // Reset to transparent for non-current players
            }
        }
    }
}