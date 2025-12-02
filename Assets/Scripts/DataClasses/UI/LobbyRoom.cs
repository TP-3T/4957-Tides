using UnityEngine;

namespace TTT.DataClasses.UI
{
    [CreateAssetMenu(
        fileName = "LobbyRoom",
        menuName = "Scriptable Objects/LobbyRoom"
    )]
    public class LobbyRoom : ScriptableObject
    {
        public int lobbyCode;
    }
}
