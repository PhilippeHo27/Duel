using Duel.Utilities;
using UnityEngine;

namespace Duel
{
    public class DataManager : IndestructibleSingletonBehaviour<DataManager>
    {
        [HideInInspector]
        public string lobbyName;
    }
}
