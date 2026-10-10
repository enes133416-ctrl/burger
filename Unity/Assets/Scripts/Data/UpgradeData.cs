using System;
using UnityEngine;

namespace BurgerRush.Data
{
    /// <summary>Ekipman yükseltme ağacı (GDD 2.6).</summary>
    [CreateAssetMenu(menuName = "Burger Rush/Upgrade Data", fileName = "UpgradeData")]
    public class UpgradeData : ScriptableObject
    {
        public EquipmentUpgrade[] equipment;
        public StaffUpgrade[] staff;
    }

    [Serializable]
    public class EquipmentUpgrade
    {
        public string id = "grill";
        public string displayName = "Izgara";
        public string[] perkNames = { "2 slot", "3 slot", "4 slot", "Turbo", "Akıllı fırın" };
        public int[] costs = { 400, 1200, 3200, 8000 };        // Lv2..Lv5 maliyetleri

        // Izgara parametreleri (yalnızca grill için anlamlı)
        public int[] slots = { 2, 3, 4, 4, 6 };
        public float[] cookDurations = { 30f, 24f, 18f, 14f, 10f };
        public float[] burnThresholds = { 1.15f, 1.15f, 1.15f, 1.25f, 1.5f };

        public int CostForLevel(int targetLevel) => costs[Mathf.Clamp(targetLevel - 2, 0, costs.Length - 1)];
    }

    [Serializable]
    public class StaffUpgrade
    {
        public string id = "cashier";
        public string displayName = "Kasiyer";
        public int hireCost = 500;
        public int[] levelCosts = { 200, 500, 1200, 3000, 8000 };   // GDD 2.6 fiyatları
        public string[] perkNames = { "Hız +10%", "İsabet +15%", "Multitask x2", "Sabır +20%", "GOLD TIER" };

        public int CostForLevel(int nextLevel) => levelCosts[Mathf.Clamp(nextLevel - 1, 0, levelCosts.Length - 1)];
    }
}
