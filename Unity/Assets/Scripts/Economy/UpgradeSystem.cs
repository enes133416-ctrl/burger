using System.Collections.Generic;
using System.Linq;
using BurgerRush.Core;
using BurgerRush.Data;
using BurgerRush.Gameplay.Staff;
using UnityEngine;

namespace BurgerRush.Economy
{
    /// <summary>
    /// Ekipman seviyeleri ve türetilmiş bonuslar (GDD 2.6).
    /// </summary>
    public class UpgradeSystem : Singleton<UpgradeSystem>
    {
        [SerializeField] private UpgradeData upgradeData;
        [SerializeField] private int maxLevel = 5;

        public int MaxLevel => maxLevel;

        public int GetEquipmentLevel(string id)
            => SaveManager.Instance.Data.equipment.TryGetValue(id, out var lvl) ? lvl : 1;

        public void SetEquipmentLevel(string id, int level)
        {
            SaveManager.Instance.Data.equipment[id] = Mathf.Clamp(level, 1, maxLevel);
            SaveManager.Instance.Save();
        }

        public bool CanUpgradeEquipment(string id) => GetEquipmentLevel(id) < maxLevel;

        public bool TryUpgradeEquipment(string id)
        {
            var def = upgradeData?.equipment?.FirstOrDefault(e => e.id == id);
            if (def == null) return false;

            int next = GetEquipmentLevel(id) + 1;
            if (next > maxLevel) return false;

            float cost = def.CostForLevel(next);
            if (!EconomyManager.Instance.SpendCoins(cost)) return false;

            SetEquipmentLevel(id, next);
            EventBus.Publish(new UpgradePurchasedEvent { UpgradeId = id, NewLevel = next });
            return true;
        }

        // ─────────── Türetilmiş değerler ───────────
        public float IncomeMultiplier
        {
            get
            {
                float multiplier = 1f;
                multiplier += (GetEquipmentLevel("cashier_desk") - 1) * 0.08f;
                if (StaffManager.Instance) multiplier += StaffManager.Instance.IncomeBonus;
                return multiplier;
            }
        }

        public int QueueBonus => GetEquipmentLevel("counter") - 1;

        public float PatienceMultiplier => 1f + (StaffManager.Instance ? StaffManager.Instance.PatienceBonus : 0f);

        public float TipBonus => StaffManager.Instance ? StaffManager.Instance.TipBonus : 0f;

        public float DriveThruBonus => StaffManager.Instance ? StaffManager.Instance.DriveThruBonus : 0f;

        public float PrepSpeedMultiplier => 1f + (GetEquipmentLevel("counter") - 1) * 0.06f;

        public int AverageUpgradeLevel
        {
            get
            {
                var values = new List<int>(SaveManager.Instance.Data.equipment.Values);
                if (StaffManager.Instance)
                    foreach (var s in StaffManager.Instance.All.Values)
                        if (s.IsHired) values.Add(s.Level);

                return values.Count == 0 ? 0 : Mathf.RoundToInt(values.Average());
            }
        }

        public bool IsFullyUpgraded => AverageUpgradeLevel >= 4;

        /// <summary>Izgara ayarlarını mevcut seviyeye göre döndürür.</summary>
        public void ApplyGrillSettings(Gameplay.Kitchen.OvenController oven)
        {
            if (!oven) return;
            oven.ApplyUpgrade(GetEquipmentLevel("grill"));
        }
    }
}
