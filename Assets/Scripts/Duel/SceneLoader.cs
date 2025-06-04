using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using Duel.Utilities;

namespace Duel
{
    public class SceneLoader : IndestructibleSingletonBehaviour<SceneLoader>
    {
        [SerializeField] private bool debugLogging = true;
        private string _currentAdditiveScene = "";
        public string CurrentAdditiveScene { get => _currentAdditiveScene; set => _currentAdditiveScene = value; }

        private readonly Dictionary<string, AsyncOperation> _activeOperations = new Dictionary<string, AsyncOperation>();
        
        public void LoadSceneAdditive(string sceneName, Action onStart = null, Action<float> onProgress = null, Action onComplete = null)
        {
            if (IsSceneLoaded(sceneName))
            {
                LogDebug($"Scene '{sceneName}' is already loaded");
                onComplete?.Invoke();
                return;
            }
            
            if (_activeOperations.ContainsKey(sceneName))
            {
                LogDebug($"Scene '{sceneName}' is already being loaded");
                return;
            }
            
            StartCoroutine(LoadSceneAdditiveCoroutine(sceneName, onStart, onProgress, onComplete));
        }
        
        public void UnloadAdditiveScene(string sceneName, Action onComplete = null)
        {
            if (!IsSceneLoaded(sceneName))
            {
                LogDebug($"Scene '{sceneName}' is not loaded or cannot be unloaded");
                onComplete?.Invoke();
                return;
            }
            
            StartCoroutine(UnloadSceneCoroutine(sceneName, onComplete));
        }
        
        public void ToggleScene(string sceneName, Action onStart = null, Action<float> onProgress = null, Action onComplete = null)
        {
            if (IsSceneLoaded(sceneName))
            {
                UnloadAdditiveScene(sceneName, onComplete);
            }
            else
            {
                LoadSceneAdditive(sceneName, onStart, onProgress, onComplete);
            }
        }
        
        public void UnloadAllAdditiveScenes(Action onComplete = null)
        {
            List<string> scenesToUnload = GetLoadedScenes();
            if (scenesToUnload.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }
            
            StartCoroutine(UnloadAllScenesCoroutine(scenesToUnload, onComplete));
        }
        
        public bool IsAnyOperationInProgress()
        {
            return _activeOperations.Count > 0;
        }
        
        private bool IsSceneLoaded(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.name == sceneName && scene.isLoaded)
                {
                    return true;
                }
            }
            return false;
        }
        
        private List<string> GetLoadedScenes()
        {
            List<string> scenes = new List<string>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded && scene.buildIndex != 0)
                {
                    scenes.Add(scene.name);
                }
            }
            return scenes;
        }
        
        private void LogDebug(string message)
        {
            if (debugLogging)
            {
                Debug.Log($"[SceneLoader] {message}");
            }
        }
        
        private IEnumerator LoadSceneAdditiveCoroutine(string sceneName, Action onStart, Action<float> onProgress, Action onComplete)
        {
            LogDebug($"Starting to load scene: {sceneName}");
            onStart?.Invoke();
    
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
    
            if (asyncLoad == null)
            {
                LogDebug($"Failed to start loading scene: {sceneName}");
                yield break;
            }
    
            _activeOperations[sceneName] = asyncLoad;
    
            while (!asyncLoad.isDone)
            {
                onProgress?.Invoke(asyncLoad.progress);
                yield return null;
            }
    
            _activeOperations.Remove(sceneName);
            _currentAdditiveScene = sceneName;
            LogDebug($"Scene loaded successfully: {sceneName}");
            onComplete?.Invoke();
        }
        
        private IEnumerator UnloadSceneCoroutine(string sceneName, Action onComplete)
        {
            LogDebug($"Starting to unload scene: {sceneName}");
    
            AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(sceneName);
    
            if (asyncUnload == null)
            {
                LogDebug($"Failed to start unloading scene: {sceneName}");
                onComplete?.Invoke();
                yield break;
            }
    
            while (!asyncUnload.isDone)
            {
                yield return null;
            }
    
            LogDebug($"Scene unloaded successfully: {sceneName}");
            yield return Resources.UnloadUnusedAssets();
            onComplete?.Invoke();
        }

        
        private IEnumerator UnloadAllScenesCoroutine(List<string> scenesToUnload, Action onComplete)
        {
            foreach (string sceneName in scenesToUnload)
            {
                yield return StartCoroutine(UnloadSceneCoroutine(sceneName, null));
            }
            onComplete?.Invoke();
        }
    }
}
