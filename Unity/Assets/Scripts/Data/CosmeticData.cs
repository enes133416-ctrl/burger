using System;
using UnityEngine;

namespace BurgerRush.Data
{
    /// <summary>Kozmetik içerikler: restoran temaları ve şef kostümleri (GDD 4.3).</summary>
    [CreateAssetMenu(menuName = "Burger Rush/Cosmetic Data", fileName = "CosmeticData")]
    public class CosmeticData : ScriptableObject
    {
        public ThemeItem[] themes;
        public SkinItem[] skins;
    }

    [Serializable]
    public class ThemeItem
    {
        public string id = "default";
        public string displayName = "Klasik Burger";
        public int gemPrice;
        public string iapPrice;
        public Color floorColor = new(0.29f, 0.29f, 0.39f);
        public Color wallColor = new(0.13f, 0.13f, 0.23f);
        public Color accentColor = new(1f, 0.42f, 0.21f);
        public Material skybox;
    }

    [Serializable]
    public class SkinItem
    {
        public string id = "classic";
        public string displayName = "Klasik Şef";
        public int gemPrice;
        public string iapPrice;
        public GameObject prefabOverride;
        public Color bodyColor = Color.white;
        public Color apronColor = new(1f, 0.42f, 0.21f);
    }
}
