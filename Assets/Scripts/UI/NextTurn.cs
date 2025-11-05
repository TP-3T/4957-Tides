using TTT.Hex;
using UnityEngine;
using UnityEngine.UI;
using TTT.GameEvents;
using System;

namespace TTT.UI
{
    [RequireComponent(typeof(Image))]
    /// <summary>
    /// Handles the functionality of the "Next Turn" button in the game UI.
    /// </summary>
    public class NextTurn : MonoBehaviour
    {
        private HexGrid hg;
        // private Sea s; //removed for events

        public static event Action<NextTurn> OnNextTurnClicked;

        void Start()
        {
            this.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;

            // s = FindFirstObjectByType<Sea>();
        }

        /// <summary>
        /// Handles the button click event to proceed to the next turn.
        /// </summary>
        public void OnClick()
        {
            OnNextTurnClicked?.Invoke(this);
        }
    }
}