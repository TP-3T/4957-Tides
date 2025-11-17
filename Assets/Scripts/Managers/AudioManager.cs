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
        //Implementation: Songs and SFX are limited so just load all as assets and have playable through methods?

        //TODO 1: Just have a single song imported and playing

        //TODO 2: Have multiple songs imported and shuffle through

        //TODO 3: Expose volume options for UI

        //TODO 3: Have SFX play based on game events like UI clicks and game state

        [Header("One-Shot Sounds")]
        [Tooltip("AudioSource used for playing one-shot sound effects")]
        public AudioSource OneShotSource;
        [Tooltip("Array of one-shot sound effect AudioEntries")]
        public AudioEntry[] OneShotEntries;

        [Header("Ambience")]
        [Tooltip("AudioSource used for playing ambience sounds")]
        public AudioSource AmbienceSource;
        //public AudioSource TempAmbienceSource;  // For cross-fading
        [Tooltip("Array of ambience AudioEntries")]
        public AudioEntry[] AmbienceEntries;
        //public double AmbienceFadeDuration = 1.0;

        [Header("Music")]
        [Tooltip("AudioSource used for playing music tracks")]
        public AudioSource MusicSource;
        //public AudioSource TempMusicSource;  // For cross-fading
        [Tooltip("Array of music AudioEntries")]
        public AudioEntry[] MusicEntries;
        //public double MusicFadeDuration = 1.0;

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
        /// Plays ambience sound.
        /// </summary>
        /// <param name="name">The EntryName of the AudioEntry</param>
        public void PlayAmbience(string name)
        {
            AudioEntry entry = GetAudioEntryByName(name, AmbienceEntries);
            if (entry != null && entry.Clip != null)
            {
                if (AmbienceSource.isPlaying)
                {
                    AmbienceSource.Stop();
                }
                AmbienceSource.clip = entry.Clip;
                AmbienceSource.loop = entry.Loop;
                AmbienceSource.Play();
            }
        }

        /// <summary>
        /// Plays music track.
        /// </summary>
        /// <param name="name">The EntryName of the AudioEntry</param>
        public void PlayMusic(string name)
        {
            AudioEntry entry = GetAudioEntryByName(name, MusicEntries);
            if (entry != null && entry.Clip != null)
            {
                if (MusicSource.isPlaying)
                {
                    MusicSource.Stop();
                }
                MusicSource.clip = entry.Clip;
                MusicSource.loop = entry.Loop;
                MusicSource.Play();
            }
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


