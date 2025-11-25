using UnityEngine;

namespace TTT.DataClasses.UI
{
    [CreateAssetMenu(
        fileName = "LobbyEntry",
        menuName = "Scriptable Objects/LobbyEntry"
    )]
    /// <summary>
    /// Represents an entry for a multiplayer lobby in the UI.
    /// </summary>
    public class LobbyEntry : ScriptableObject
    {
        [field: Tooltip("The name of the lobby.")]
        [field: SerializeField]
        public string LobbyName { get; set; }

        [field: Tooltip("The unique identifier of the lobby.")]
        [field: SerializeField]
        public string LobbyID { get; set; }

        [field: Tooltip("The current number of players in the lobby.")]
        [field: SerializeField]
        public int CurrentPlayers { get; set; }

        public int Ping { get; set; }

        [field: Tooltip("Whether the game in the lobby has started.")]
        [field: SerializeField]
        public bool IsStarted { get; set; }
    }
}
