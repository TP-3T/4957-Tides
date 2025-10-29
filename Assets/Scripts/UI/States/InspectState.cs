using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TTT.GameEvents;
using UnityEngine;

namespace TTT.UI
{
    /// <summary>
    /// A UI state contains a list of what UI components are enabled for that state.
    /// </summary>
    [CreateAssetMenu(
        fileName = "InspectState",
        menuName = "Scriptable Objects/UI States/Inspect State"
    )]
    public class InspectState : UIState
    {
        [Tooltip("Event to raise on a right click.")]
        [SerializeField]
        private GameEvent RightClickEvent;

        public void OnRightClick(Vector3 mousePos)
        {
            ClickEventArgs args = CreateInstance<ClickEventArgs>();
            args.mousePos = mousePos;
            RightClickEvent.Raise(args);
        }
    }
}
