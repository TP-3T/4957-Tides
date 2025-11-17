using System.Collections;
using UnityEngine;

namespace TTT.Helpers
{
    /// <summary>
    /// Helper class for fading audio sources in and out.
    /// </summary>
    public class AudioFadingPlayer : MonoBehaviour
    {
        /// <summary>
        /// Default duration for fade transitions in seconds.
        /// </summary>
        [Header("Controls")]
        [Tooltip("Default duration for fade transitions in seconds")]
        public float DefaultFadeDuration = 1f;

        [Header("Audio Sources")]
        [Tooltip("First AudioSource used for crossfading")]
        [SerializeField] private AudioSource sourceA;
        [Tooltip("Second AudioSource used for crossfading")]
        [SerializeField] private AudioSource sourceB;

        private Coroutine crossfadeCoroutine;
        private bool isUsingSourceA = true;

        /// <summary>
        /// Plays the given audio clip instantly, stopping any currently playing clip.
        /// </summary>
        /// <param name="newClip">The Audio Clip to play</param>
        public void PlayClipInstant(AudioClip newClip)
        {
            // Stop any ongoing crossfade
            if (crossfadeCoroutine != null)
            {
                StopCoroutine(crossfadeCoroutine);
            }

            AudioSource activeSource = GetActiveSource();
            AudioSource inactiveSource = GetInactiveSource();

            inactiveSource.Stop();
            inactiveSource.clip = newClip;
            inactiveSource.volume = 1f;
            inactiveSource.Play();

            activeSource.Stop();

            isUsingSourceA = (inactiveSource == sourceA);
        }

        /// <summary>
        /// Fades from the currently playing clip to the new clip over the given duration.
        /// If fadeDuration is less than 0, the DefaultFadeDuration will be used.
        /// </summary>
        /// <param name="newClip">The Audio Clip to play</param>
        /// <param name="fadeDuration">The duration to fade, in seconds - if this is less than 0, uses DefaultFadeDuration instead</param>
        public void FadeToClip(AudioClip newClip, float fadeDuration = -1f)
        {
            if (fadeDuration < 0f)
            {
                fadeDuration = DefaultFadeDuration;
            }

            if (crossfadeCoroutine != null)
            {
                StopCoroutine(crossfadeCoroutine);
            }

            crossfadeCoroutine = StartCoroutine(FadeRoutine(newClip, fadeDuration));
        }

        /// <summary>
        /// Checks if a fade is currently in progress.
        /// Use this to prevent overlapping fade requests.
        /// </summary>
        /// <returns>True if this player is currently fading, False otherwise</returns>
        public bool IsFadeInProgress()
        {
            return crossfadeCoroutine != null;
        }

        private void Awake()    // Ensure both audio sources are set up
        {
            if (sourceA == null)
            {
                sourceA = gameObject.AddComponent<AudioSource>();
                sourceA.loop = true;
            }
            if (sourceB == null)
            {
                sourceB = gameObject.AddComponent<AudioSource>();
                sourceB.loop = true;
            }
        }

        private IEnumerator FadeRoutine(AudioClip newClip, float fadeDuration)  // Handles the crossfade between two audio sources
        {
            AudioSource audioSourceToFadeOut = GetActiveSource();
            AudioSource audioSourceToFadeIn = GetInactiveSource();

            audioSourceToFadeIn.clip = newClip;
            audioSourceToFadeIn.volume = 0f;
            audioSourceToFadeIn.Play();

            float timer = 0f;

            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                float t = timer / fadeDuration;

                audioSourceToFadeIn.volume = Mathf.Lerp(0f, 1f, t);
                audioSourceToFadeOut.volume = Mathf.Lerp(1f, 0f, t);

                yield return null;
            }

            audioSourceToFadeOut.Stop();
            isUsingSourceA = (audioSourceToFadeIn == sourceA);
        }

        private AudioSource GetActiveSource()   // The audio source currently playing
        {
            return isUsingSourceA ? sourceA : sourceB;
        }

        private AudioSource GetInactiveSource() // The audio source not currently playing
        {
            return isUsingSourceA ? sourceB : sourceA;
        }
    }
}