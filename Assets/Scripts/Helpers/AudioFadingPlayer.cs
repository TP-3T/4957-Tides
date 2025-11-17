using System.Collections;
using UnityEngine;

namespace TTT.Helpers
{
    /// <summary>
    /// Helper class for fading audio sources in and out.
    /// </summary>
    public class AudioFadingPlayer : MonoBehaviour
    {
        [Header("Controls")]
        public float DefaultFadeDuration = 1f;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource sourceA;
        [SerializeField] private AudioSource sourceB;

        private Coroutine crossfadeCoroutine;
        private bool isUsingSourceA = true;

        public void PlayClipInstant(AudioClip newClip)
        {
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

        public bool IsFadeInProgress()
        {
            return crossfadeCoroutine != null;
        }

        private void Awake()
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

        private IEnumerator FadeRoutine(AudioClip newClip, float fadeDuration)
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

        private AudioSource GetActiveSource()
        {
            return isUsingSourceA ? sourceA : sourceB;
        }

        private AudioSource GetInactiveSource()
        {
            return isUsingSourceA ? sourceB : sourceA;
        }
    }
}