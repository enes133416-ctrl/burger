using System.Collections;
using UnityEngine;

namespace BurgerRush.UI
{
    /// <summary>UI animasyon yardımcıları: pop, fade, sayı sayacı (GDD 9.2).</summary>
    public static class UIAnimator
    {
        public static void Pop(Transform target, float scale = 1.15f, float duration = 0.25f)
        {
            if (!target) return;
            target.GetComponent<MonoBehaviour>()?.StartCoroutine(PopRoutine(target, scale, duration));
        }

        private static IEnumerator PopRoutine(Transform target, float scale, float duration)
        {
            var start = target.localScale;
            var peak = start * scale;
            float t = 0f;

            while (t < duration * 0.4f)
            {
                t += Time.deltaTime;
                target.localScale = Vector3.Lerp(start, peak, t / (duration * 0.4f));
                yield return null;
            }

            t = 0f;
            while (t < duration * 0.6f)
            {
                t += Time.deltaTime;
                target.localScale = Vector3.Lerp(peak, start, t / (duration * 0.6f));
                yield return null;
            }

            target.localScale = start;
        }

        public static IEnumerator CountTo(UnityEngine.UI.Text label, float from, float to, float duration)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                label.text = Mathf.RoundToInt(Mathf.Lerp(from, to, t / duration)).ToString();
                yield return null;
            }
            label.text = Mathf.RoundToInt(to).ToString();
        }
    }
}
