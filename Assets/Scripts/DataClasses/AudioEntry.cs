using System;
using UnityEngine;

namespace TTT.DataClasses
{
    /// <summary>
    /// Data class for storing information about each Sound asset.
    /// Used by AudioManager.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(
        fileName = "New Audio Entry",
        menuName = "TTT/Data Classes/Audio Entry"
    )]
    public class AudioEntry : ScriptableObject
    {
        /// <summary>
        /// The unique name identifier for this audio entry.
        /// </summary>
        [SerializeField]
        public string EntryName;

        /// <summary>
        /// The audio clip associated with this entry.
        /// </summary>
        [SerializeField]
        public AudioClip Clip;
    }
}
