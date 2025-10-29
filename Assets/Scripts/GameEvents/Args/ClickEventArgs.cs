using UnityEngine;

namespace TTT.GameEvents
{
    [CreateAssetMenu(
        fileName = "Click Event Args",
        menuName = "Scriptable Objects/GameEvents/Click Event Args"
    )]
    public class ClickEventArgs : ScriptableObject
    {
        /// <summary>
        /// The cursor's position during the click.
        /// </summary>
        public Vector3 mousePos;
    }
}
