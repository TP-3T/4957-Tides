using System;
using TTT.Helpers;
using UnityEngine;

namespace TTT.Events
{
    public class FloodEventArgs : EventArgs
    {
        public float FloodIncrement { get; set; }
    }

    public class NewMapEventArgs : EventArgs
    {
        public TextAsset DataFile { get; set; }
    }

    public class TTTEvents : GenericSingleton<TTTEvents>
    {
        /// <summary>
        /// Invoked when a tile is clicked.
        /// </summary>
        public static EventHandler TileClickEvent;

        /// <summary>
        /// Event invoked when the next turn is requested.
        /// </summary>
        public static EventHandler NextTurnRequestedEvent;

        /// <summary>
        /// Called when a flood is happening
        /// </summary>
        public static EventHandler FloodEvent;

        /// <summary>
        /// Called when the Board State changes
        /// </summary>
        public static EventHandler ChangeBoardState;

        /// <summary>
        /// Called when the FloodIncrement is set in the Main Menu
        /// </summary>
        public static EventHandler FloodIncrementChangeEvent;

        public static EventHandler CreateNewMap;

        public static EventHandler LoadCustomMap;

        public static EventHandler FinishCreatingMap;

        /// <summary>
        /// Global "Quit the game" function.
        /// </summary>
        public static void QuitGame()
        {
            Application.Quit();
        }
    }
}
