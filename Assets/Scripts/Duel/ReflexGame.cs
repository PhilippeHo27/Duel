using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Threading.Tasks;
using TMPro;

namespace Duel
{
    public class ReflexGame : MonoBehaviour
    {
        [Header("UI Elements")]
        public Image gameImage;
        public Button startButton;
        public TMP_Text instructionText;
        public TMP_Text resultText;
        public TMP_Text declareWinnerText;
        
        [Header("Game Settings")]
        public Color waitColor = Color.red;
        public Color clickColor = Color.green;
        public float minWaitTime = 2f;
        public float maxWaitTime = 5f;
        
        private bool gameActive = false;
        private bool waitingForClick = false;
        private float gameStartTime;
        private int lastReactionTimeMs = 0;
        
        void Start()
        {
            SetupInitialState();
            
            // Add listeners
            if (startButton != null)
                startButton.onClick.AddListener(() => StartGameAsync().ConfigureAwait(false));
                
            if (gameImage != null)
                gameImage.GetComponent<Button>().onClick.AddListener(OnImageClicked);
        }
        
        private void SetupInitialState()
        {
            if (gameImage != null)
            {
                gameImage.color = waitColor;
                // Make sure the image has a Button component
                if (gameImage.GetComponent<Button>() == null)
                {
                    gameImage.gameObject.AddComponent<Button>();
                }
            }
            
            if (instructionText != null)
                instructionText.text = "Click 'Start Game' to begin!";
                
            if (resultText != null)
                resultText.text = "";
        }
        
        private async Task StartGameAsync()
        {
            if (gameActive) return;
            
            gameActive = true;
            waitingForClick = false;
            
            // Reset UI
            if (gameImage != null)
                gameImage.color = waitColor;
                
            if (instructionText != null)
                instructionText.text = "Wait for GREEN... Don't click yet!";
                
            if (resultText != null)
                resultText.text = "";
                
            if (startButton != null)
                startButton.interactable = false;
            
            // Wait random time before turning green
            float waitTime = Random.Range(minWaitTime, maxWaitTime);
            await Task.Delay((int)(waitTime * 1000));
            
            // Check if game is still active (user might have clicked early)
            if (!gameActive) return;
            
            // Turn green and start timing
            if (gameImage != null)
                gameImage.color = clickColor;
                
            if (instructionText != null)
                instructionText.text = "CLICK NOW!";
                
            waitingForClick = true;
            gameStartTime = Time.realtimeSinceStartup;
        }
        
        private void OnImageClicked()
        {
            if (!gameActive) return;
            
            if (!waitingForClick)
            {
                // Clicked too early
                EndGame(false, 0);
                return;
            }
            
            // Calculate reaction time
            float reactionTime = Time.realtimeSinceStartup - gameStartTime;
            int reactionTimeMs = Mathf.RoundToInt(reactionTime * 1000);
            
            EndGame(true, reactionTimeMs);
        }
        
        private async void EndGame(bool success, int reactionTimeMs)
        {
            gameActive = false;
            waitingForClick = false;
            lastReactionTimeMs = reactionTimeMs;
    
            if (startButton != null)
                startButton.interactable = true;
    
            if (success)
            {
                if (instructionText != null)
                    instructionText.text = "Great job! Submitting result...";
            
                if (resultText != null)
                    resultText.text = $"Reaction Time: {reactionTimeMs}ms";
        
                // Submit result to server (singleton call)
                var result = await UnityGameServicesManager.Instance.SubmitReflexResult(DataManager.Instance.lobbyName, reactionTimeMs);
                declareWinnerText.text = result.winner;
                Debug.Log($"Reaction time: {reactionTimeMs}ms");
            }
            else
            {
                if (instructionText != null)
                    instructionText.text = "Too early! Wait for GREEN next time.";
            
                if (resultText != null)
                    resultText.text = "False start!";
            
                Debug.Log("False start - clicked too early!");
            }
    
            // Reset image color
            if (gameImage != null)
                gameImage.color = waitColor;
        }

        
        /// <summary>
        /// Returns the last recorded reaction time in milliseconds
        /// Returns 0 if no valid reaction time has been recorded
        /// </summary>
        public int GetLastReactionTimeMs()
        {
            return lastReactionTimeMs;
        }
        
        /// <summary>
        /// Returns true if a game is currently in progress
        /// </summary>
        public bool IsGameActive()
        {
            return gameActive;
        }
        
        void OnDestroy()
        {
            if (startButton != null)
                startButton.onClick.RemoveAllListeners();
                
            if (gameImage != null && gameImage.GetComponent<Button>() != null)
                gameImage.GetComponent<Button>().onClick.RemoveAllListeners();
        }
    }
}
