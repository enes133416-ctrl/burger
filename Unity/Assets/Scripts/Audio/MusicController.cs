using System.Collections;
using UnityEngine;

namespace BurgerRush.Audio
{
    /// <summary>Müzik katmanı: crossfade ile parça değişimi.</summary>
    public class MusicController : MonoBehaviour
    {
        [SerializeField] private AudioSource sourceA;
        [SerializeField] private AudioSource sourceB;
        [SerializeField] private float crossfadeTime = 1.2f;
        [SerializeField] private float defaultVolume = 0.35f;

        private bool usingA = true;

        private void Awake()
        {
            if (sourceA) sourceA.volume = defaultVolume;
            if (sourceB) sourceB.volume = 0f;
        }

        public void Play(AudioClip clip, bool loop = true)
        {
            if (!clip) return;

            var active = usingA ? sourceA : sourceB;
            var idle = usingA ? sourceB : sourceA;

            if (active && active.clip == clip && active.isPlaying) return;

            if (idle)
            {
                idle.clip = clip;
                idle.loop = loop;
                idle.volume = 0f;
                idle.Play();
            }

            StopAllCoroutines();
            StartCoroutine(Crossfade(idle, active));
            usingA = !usingA;
        }

        private IEnumerator Crossfade(AudioSource fadeIn, AudioSource fadeOut)
        {
            float timer = 0f;
            while (timer < crossfadeTime)
            {
                timer += Time.deltaTime;
                float t = timer / crossfadeTime;
                if (fadeIn) fadeIn.volume = Mathf.Lerp(0f, defaultVolume, t);
                if (fadeOut) fadeOut.volume = Mathf.Lerp(defaultVolume, 0f, t);
                yield return null;
            }
            if (fadeOut && fadeOut.volume <= 0.01f) fadeOut.Stop();
        }

        public void SetVolume(float volume)
        {
            defaultVolume = Mathf.Clamp01(volume);
            var active = usingA ? sourceA : sourceB;
            if (active) active.volume = defaultVolume;
        }

        public void Stop()
        {
            if (sourceA) sourceA.Stop();
            if (sourceB) sourceB.Stop();
        }
    }
}
