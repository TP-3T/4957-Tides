using System;
using PlasticGui.WorkspaceWindow;
using PlasticPipe.PlasticProtocol.Messages;
using UnityEngine;

namespace TTT.GameEvents.Assets.Scripts.GameEvents.Args
{
    public class ConnectedCilentsEventArgs : UnityEngine.Object
    {
        public ulong[] clientIds { get; set; }
    }
}
