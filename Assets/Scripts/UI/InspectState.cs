using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        public override void OnClick()
        {
            throw new System.NotImplementedException();
        }
    }
}
