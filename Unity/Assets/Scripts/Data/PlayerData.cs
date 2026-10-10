using System;
using System.Collections.Generic;

namespace BurgerRush.Data
{
    /// <summary>Kayıt dosyasının şeması (GDD 6.3).</summary>
    [Serializable]
    public class PlayerData
    {
        public int version = 1;
        public int currentLevel = 1;
        public int maxLevelReached = 1;
        public float totalCoins = 250f;
        public float totalGems = 30f;
        public int totalStars;
        public Dictionary<int, int> levelStars = new();          // levelIndex → 0..3
        public Dictionary<string, StaffSave> staff = new();      // staffId → {hired, level}
        public Dictionary<string, int> equipment = new() { { "grill", 1 }, { "counter", 1 }, { "cashier_desk", 1 } };
        public List<string> unlockedCosmetics = new() { "default", "classic" };
        public string equippedTheme = "default";
        public string equippedSkin = "classic";
        public long lastOfflineTime;                              // unix ms (UTC)
        public float offlineEarnings;
        public bool hasChefPass;
        public long chefPassExpiry;
        public SettingsData settings = new();
        public StatsData stats = new();
        public FlagsData flags = new();

        [Serializable] public class StaffSave { public bool hired; public int level; }
        [Serializable] public class SettingsData { public bool music = true, sfx = true, haptic = true, shadows = true; }
        [Serializable] public class StatsData { public int served, burnt, lost; public float playSeconds; }
        [Serializable] public class FlagsData { public bool starterPackBought, tutorialDone; }
    }
}
