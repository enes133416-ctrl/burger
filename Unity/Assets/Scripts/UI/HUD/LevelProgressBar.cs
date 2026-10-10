using UnityEngine;
using UnityEngine.UI;

namespace BurgerRush.UI.HUD
{
    /// <summary>Bölüm ilerleme çubuğu: tamamlanan sipariş / toplam sipariş (GDD 5.1).</summary>
    public class LevelProgressBar : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private Text label;

        private Core.LevelManager levelManager;

        private void Start() => levelManager = Core.LevelManager.Instance;

        private void Update()
        {
            if (!levelManager) return;
            if (slider) slider.value = levelManager.Progress;
            if (label) label.text = $"Sipariş {levelManager.ServedCount}";
        }
    }
}
