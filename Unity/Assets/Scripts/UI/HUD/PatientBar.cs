using UnityEngine;
using UnityEngine.UI;

namespace BurgerRush.UI.HUD
{
    /// <summary>
    /// Ekran üstü (screen-space) sabır göstergesi — 3D world-space barın UI karşılığı.
    /// </summary>
    public class PatientBar : MonoBehaviour
    {
        [SerializeField] private Image fill;
        [SerializeField] private Color safeColor = new(0.18f, 0.80f, 0.44f);
        [SerializeField] private Color warnColor = new(1f, 0.84f, 0f);
        [SerializeField] private Color dangerColor = new(0.91f, 0.30f, 0.24f);

        public void SetValue(float normalized)
        {
            normalized = Mathf.Clamp01(normalized);
            if (!fill) return;
            fill.fillAmount = normalized;
            fill.color = normalized > 0.6f ? safeColor : normalized > 0.35f ? warnColor : dangerColor;
        }
    }
}
