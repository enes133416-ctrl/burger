using UnityEngine;
using UnityEngine.UI;

namespace BurgerRush.UI.HUD
{
    /// <summary>Geri sayım: son 20 saniyede kırmızıya döner.</summary>
    public class TimerDisplay : MonoBehaviour
    {
        [SerializeField] private Text timerText;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color dangerColor = new(0.91f, 0.30f, 0.24f);
        [SerializeField] private float dangerThreshold = 20f;

        private Core.LevelManager levelManager;

        private void Start() => levelManager = Core.LevelManager.Instance;

        private void Update()
        {
            if (!levelManager || !timerText) return;

            float t = Mathf.Max(0f, levelManager.TimeLeft);
            int minutes = Mathf.FloorToInt(t / 60f);
            int seconds = Mathf.FloorToInt(t % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
            timerText.color = t <= dangerThreshold ? dangerColor : normalColor;
        }
    }
}
