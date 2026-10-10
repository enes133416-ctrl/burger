using System.Collections.Generic;
using System.Linq;
using BurgerRush.Data;
using BurgerRush.Gameplay.Kitchen;
using UnityEngine;

namespace BurgerRush.Economy
{
    /// <summary>
    /// Upgrade Office: ekipman (ızgara, tezgah, kasa) yükseltmeleri (GDD 2.6).
    /// </summary>
    public class UpgradeOfficeManager : Singleton<UpgradeOfficeManager>
    {
        [SerializeField] private UpgradeData upgradeData;
        [SerializeField] private KitchenManager kitchen;

        public bool IsUnlocked => SaveManager.Instance.Data.currentLevel >= 14;

        public IEnumerable<EquipmentUpgrade> AllEquipment
            => upgradeData?.equipment ?? Enumerable.Empty<EquipmentUpgrade>();

        public int CostFor(string id)
        {
            var def = upgradeData?.equipment?.FirstOrDefault(e => e.id == id);
            int level = UpgradeSystem.Instance.GetEquipmentLevel(id);
            if (def == null || level >= UpgradeSystem.Instance.MaxLevel) return 0;
            return def.CostForLevel(level + 1);
        }

        public bool TryUpgrade(string id)
        {
            bool ok = UpgradeSystem.Instance.TryUpgradeEquipment(id);
            if (ok && id == "grill" && kitchen) kitchen.ApplyEquipmentUpgrades();
            return ok;
        }

        /// <summary>Izgara seviyesinin okunabilir özeti (UI).</summary>
        public string GrillSummary()
        {
            int level = UpgradeSystem.Instance.GetEquipmentLevel("grill");
            var def = upgradeData?.equipment?.FirstOrDefault(e => e.id == "grill");
            if (def == null) return $"Lv {level}";
            int idx = Mathf.Clamp(level - 1, 0, def.slots.Length - 1);
            return $"Lv {level} · {def.slots[idx]} slot · {def.cookDurations[idx]:0}s";
        }
    }
}
