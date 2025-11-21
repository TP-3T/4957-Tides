using TTT.DataClasses;
using TTT.Helpers;
using UnityEngine;

namespace TTT.Managers
{
    /// <summary>
    /// Singleton class for managing audio related to the game including music and SFX
    /// </summary>
    public class AudioManager : GenericSingleton<AudioManager>
    {
        [Header("One-Shot Sounds")]
        [Tooltip("Array of one-shot sound effect AudioEntries")]
        public AudioEntry[] OneShotEntries;
        [Tooltip("AudioSource used for playing one-shot sound effects")]
        public AudioSource OneShotSource;

        [Header("Ambience")]
        public AudioEntry[] AmbienceEntries;
        [Tooltip("AudioFadingPlayer used for playing and fading ambience")]
        public AudioFadingPlayer AmbienceFadingPlayer;

        [Header("Music")]
        [Tooltip("Array of music AudioEntries")]
        public AudioEntry[] MusicEntries;
        [Tooltip("AudioFadingPlayer used for playing and fading music tracks")]
        public AudioFadingPlayer MusicFadingPlayer;

        /// <summary>
        /// Plays a one-shot sound effect.
        /// </summary>
        /// <param name="name">The EntryName of the AudioEntry to play</param>
        public void PlayOneShotSound(string name)
        {
            AudioEntry entry = GetAudioEntryByName(name, OneShotEntries);
            if (entry != null && entry.Clip != null)
            {
                OneShotSource.PlayOneShot(entry.Clip);
            }
        }

        /// <summary>
        /// Plays the given ambience track.
        /// </summary>
        /// <param name="name">The name of the AudioEntry clip to play</param>
        /// <param name="fade">Whether or not to fade the clip in</param>
        public void PlayAmbience(string name, bool fade = true)
        {
            AudioEntry entry = GetAudioEntryByName(name, AmbienceEntries);
            if (entry != null && entry.Clip != null)
            {
                if (fade)
                {
                    AmbienceFadingPlayer.FadeToClip(entry.Clip);
                }
                else
                {
                    AmbienceFadingPlayer.PlayClipInstant(entry.Clip);
                }
            }
        }

        /// <summary>
        /// Plays the given ambience track with fading.
        /// </summary>
        /// <param name="name">The name of the AudioEntry clip to play</param>
        public void PlayAmbience(string name)
        {
            PlayAmbience(name, true);
        }

        /// <summary>
        /// Checks if ambience is currently fading.
        /// </summary>
        /// <returns>True if ambience is fading, false otherwise</returns>
        public bool IsAmbienceCurrentlyFading()
        {
            return AmbienceFadingPlayer.IsFadeInProgress();
        }

        /// <summary>
        /// Plays the given music track.
        /// </summary>
        /// <param name="name">The name of the AudioEntry clip to play</param>
        /// <param name="fade">Whether or not to fade the clip in</param>
        public void PlayMusic(string name, bool fade = true)
        {
            AudioEntry entry = GetAudioEntryByName(name, MusicEntries);
            if (entry != null && entry.Clip != null)
            {
                if (fade)
                {
                    MusicFadingPlayer.FadeToClip(entry.Clip);
                }
                else
                {
                    MusicFadingPlayer.PlayClipInstant(entry.Clip);
                }
            }
        }

        /// <summary>
        /// Checks if music is currently fading.
        /// </summary>
        /// <returns>True if music is fading, false otherwise</returns>
        public bool IsMusicCurrentlyFading()
        {
            return MusicFadingPlayer.IsFadeInProgress();
        }

        /// <summary>
        /// Gets an AudioEntry by its name from the provided array.
        /// </summary>
        /// <param name="name">The EntryName of the AudioEntry to retrieve</param>
        /// <param name="entries">Array of all AudioEntry objects to search through</param>
        /// <returns>The first AudioEntry with a matching EntryName, or null if nothing's found</returns>
        private AudioEntry GetAudioEntryByName(string name, AudioEntry[] entries)
        {
            foreach (AudioEntry entry in entries)
            {
                if (entry.EntryName == name)
                {
                    return entry;
                }
            }
            Debug.LogWarning($"AudioManager: AudioEntry with name {name} not found.");
            return null;
        }
    }
}


