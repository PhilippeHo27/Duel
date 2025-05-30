using UnityEngine;

namespace Duel
{
    [CreateAssetMenu(fileName = "MinigameList", menuName = "Duel/Minigame List")]
    public class MinigameList : ScriptableObject
    {
        [System.Serializable]
        public class MinigameEntry
        {
            public string sceneName;
            
            // TODO: Future additions for polish
            // public string displayName;
            // public Sprite thumbnail;
            // public int maxPlayers;
            // public bool isUnlocked;
        }
        
        public MinigameEntry[] miniGames;
    }
}