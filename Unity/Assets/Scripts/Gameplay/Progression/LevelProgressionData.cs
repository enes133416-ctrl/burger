using System.Collections.Generic;
using UnityEngine;

namespace BurgerRush.Gameplay.Progression
{
    /// <summary>
    /// Bölüm → lokasyon → kilit açılışları haritası (GDD 3.1 / 3.3).
    /// </summary>
    [CreateAssetMenu(menuName = "Burger Rush/Level Progression", fileName = "LevelProgressionData")]
    public class LevelProgressionData : ScriptableObject
    {
        [Serializable]
        public class Range
        {
            public int from;
            public int to;
            public LocationId location;
            public string sceneName;
        }

        [SerializeField] private List<Range> ranges = new()
        {
            new Range { from = 1,  to = 10, location = LocationId.Downtown,  sceneName = "03_Gameplay_Downtown" },
            new Range { from = 11, to = 20, location = LocationId.Beach,     sceneName = "04_Gameplay_Beach" },
            new Range { from = 21, to = 30, location = LocationId.Rooftop,   sceneName = "05_Gameplay_Rooftop" },
            new Range { from = 31, to = 40, location = LocationId.Mountain,  sceneName = "06_Gameplay_Mountain" },
            new Range { from = 41, to = 50, location = LocationId.Tokyo,     sceneName = "07_Gameplay_Tokyo" },
        };

        [Header("Unlocks")]
        public int driveThruLevel = 8;
        public int hrOfficeLevel = 9;
        public int upgradeOfficeLevel = 14;
        public int coopLevel = 16;
        public int deliveryLevel = 27;
        public int smartKitchenLevel = 28;
        public int franchiseLevel = 35;

        public Range RangeFor(int level)
        {
            foreach (var r in ranges)
                if (level >= r.from && level <= r.to) return r;
            return ranges[^1];
        }

        public bool IsUnlocked(string feature, int currentLevel) => feature switch
        {
            "driveThru" => currentLevel >= driveThruLevel,
            "hrOffice" => currentLevel >= hrOfficeLevel,
            "upgradeOffice" => currentLevel >= upgradeOfficeLevel,
            "coop" => currentLevel >= coopLevel,
            "delivery" => currentLevel >= deliveryLevel,
            "smartKitchen" => currentLevel >= smartKitchenLevel,
            "franchise" => currentLevel >= franchiseLevel,
            _ => true,
        };
    }
}
