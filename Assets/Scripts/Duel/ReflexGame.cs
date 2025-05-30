using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;

namespace Duel
{
    public class ReflexGame : MonoBehaviour, IPointerClickHandler
    {
        [Header("UI Elements")]
        [SerializeField] private Image gameImage;
        [SerializeField] private TMP_Text instructionText;
        [SerializeField] private TMP_Text declareWinnerText;
        //[SerializeField] private Button backButton;

        [Header("Game Settings")]
        [SerializeField] private float minWaitTime = 2f;
        [SerializeField] private float maxWaitTime = 5f;

        private readonly Color _waitColor = new Color32(0xFF, 0x5E, 0x5E, 0xFF);
        private readonly Color _clickColor = new Color32(0x01, 0xFF, 0x76, 0xFF);
        private Color _defaultBackground = new Color32(0xB9, 0xCF, 0xC4, 0xFF);

        private const string InitialText = "Click anywhere to start";
        private const string WaitText = "Wait for change...";
        private const string ClickNowText = "CLICK NOW!";
        private const string TooSoonText = "Too soon! Click to try again";
        private const string ResultText = "Reaction: {0}ms - Click to try again";

        private bool _gameActive;
        private bool _waitingForClick;
        private float _gameStartTime;
        private bool _gameStarted;

        private ReflexGameUGS _reflexGameUgs;
        
        void Start()
        {
            _reflexGameUgs = new ReflexGameUGS(UnityGameServicesManager.Instance);
            if (gameImage != null) _defaultBackground = gameImage.color;
            SetupInitialState();
            //backButton?.onClick.AddListener(() => SceneLoader.Instance.UnloadAdditiveScene("ReflexGame", () => Debug.Log("yup")));
        }
        
        private void SetupInitialState()
        {
            if (gameImage != null)
                gameImage.color = _defaultBackground;
                
            if (instructionText != null)
                instructionText.text = InitialText;
                
            _gameStarted = false;
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.pointerCurrentRaycast.gameObject != gameImage.gameObject) 
                return;

            if (!_gameStarted)
            {
                StartGame();
                return;
            }
            
            if (!_gameActive)
            {
                StartGame();
                return;
            }
            
            if (!_waitingForClick)
            {
                EndGame(false, 0);
                return;
            }
            
            float reactionTime = Time.realtimeSinceStartup - _gameStartTime;
            int reactionTimeMs = Mathf.RoundToInt(reactionTime * 1000);
            EndGame(true, reactionTimeMs);
        }
        
        private void StartGame()
        {
            if (_gameActive) return;
            
            _gameStarted = true;
            StartCoroutine(GameSequence());
        }
        
        private IEnumerator GameSequence()
        {
            _gameActive = true;
            _waitingForClick = false;
            
            if (gameImage != null)
                gameImage.color = _waitColor;
                
            if (instructionText != null)
                instructionText.text = WaitText;
            
            float waitTime = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(waitTime);
            
            if (!_gameActive) yield break;
            
            if (gameImage != null)
                gameImage.color = _clickColor;
                
            if (instructionText != null)
                instructionText.text = ClickNowText;
                
            _waitingForClick = true;
            _gameStartTime = Time.realtimeSinceStartup;
        }
        
        private void EndGame(bool success, int reactionTimeMs)
        {
            _gameActive = false;
            _waitingForClick = false;
    
            if (gameImage != null)
                gameImage.color = _defaultBackground;

            if (success)
            {
                if (instructionText != null)
                    instructionText.text = string.Format(ResultText, reactionTimeMs);
    
                SubmitResultAsync(reactionTimeMs);
            }
            else
            {
                if (instructionText != null)
                    instructionText.text = TooSoonText;
            }
        }

        private async void SubmitResultAsync(int reactionTimeMs)
        {
            try
            {
                var result = await _reflexGameUgs.SubmitReflexResult(DataManager.Instance.lobbyName, reactionTimeMs);
                if (declareWinnerText != null)
                     declareWinnerText.text = result.Winner;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to submit result: {ex.Message}");
            }
        }

    }
}
