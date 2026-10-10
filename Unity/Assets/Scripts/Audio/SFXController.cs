using System.Collections.Generic;
using UnityEngine;

namespace BurgerRush.Audio
{
    /// <summary>Ses efekti havuzu: aynı anda birden fazla efekt çalınabilir.</summary>
    public class SFXController : MonoBehaviour
    {
        [SerializeField] private int poolSize = 8;
        [SerializeField] private float volume = 0.7f;

        private readonly List<AudioSource> sources = new();
        private int cursor;

        private void Awake()
        {
            for (int i = 0; i < poolSize; i++)
            {
                var source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.volume = volume;
                sources.Add(source);
            }
        }

        public void Play(AudioClip clip, float pitchJitter = 0.05f)
        {
            if (!clip || sources.Count == 0) return;

            var source = sources[cursor];
            cursor = (cursor + 1) % sources.Count;

            source.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
            source.PlayOneShot(clip);
        }

        public void SetVolume(float value)
        {
            volume = Mathf.Clamp01(value);
            foreach (var s in sources) s.volume = volume;
        }
    }
}
