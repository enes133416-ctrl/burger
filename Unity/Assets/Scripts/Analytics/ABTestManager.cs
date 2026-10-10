using System.Collections.Generic;
using UnityEngine;

namespace BurgerRush.Analytics
{
    /// <summary>
    /// A/B testi: reklam frekansı, starter pack fiyatı, sabır hızı (GDD KPI bölümü).
    /// </summary>
    public class ABTestManager : Utils.Singleton<ABTestManager>
    {
        [System.Serializable]
        public class Variant
        {
            public string key;
            public string[] options;
            public int selectedIndex;
            public string Selected => options.Length > 0 ? options[Mathf.Clamp(selectedIndex, 0, options.Length - 1)] : "";
        }

        [SerializeField] private List<Variant> variants = new()
        {
            new Variant { key = "ad_frequency", options = new[] { "3", "4", "5" } },
            new Variant { key = "starter_price", options = new[] { "0.99", "1.99" } },
            new Variant { key = "patience_speed", options = new[] { "slow", "normal", "fast" } },
        };

        public string Get(string key)
        {
            var variant = variants.Find(v => v.key == key);
            if (variant == null) return "";
            if (variant.selectedIndex < 0)
                variant.selectedIndex = Random.Range(0, variant.options.Length);
            return variant.Selected;
        }

        public int GetInt(string key, int fallback = 0)
            => int.TryParse(Get(key), out var value) ? value : fallback;

        public float GetFloat(string key, float fallback = 1f)
            => float.TryParse(Get(key), out var value) ? value : fallback;
    }
}
