using System.Collections.Generic;
using System.Linq;
using BurgerRush.Data;
using BurgerRush.Gameplay.Staff;
using UnityEngine;

namespace BurgerRush.Economy
{
    /// <summary>
    /// HR Office: personel işe alma ve seviye yükseltme (GDD 2.6).
    /// </summary>
    public class HROfficeManager : Singleton<HROfficeManager>
    {
        [SerializeField] private UpgradeData upgradeData;

        public bool IsUnlocked => SaveManager.Instance.Data.currentLevel >= 9;

        public StaffUpgrade DefinitionFor(string staffId)
            => upgradeData?.staff?.FirstOrDefault(s => s.id == staffId);

        public int HireCostFor(string staffId)
            => StaffManager.Instance?.Get(staffId)?.HireCost ?? DefinitionFor(staffId)?.hireCost ?? 500;

        public int UpgradeCostFor(string staffId)
        {
            var staff = StaffManager.Instance?.Get(staffId);
            if (staff == null || !staff.IsHired || staff.Level >= 5) return 0;

            var def = DefinitionFor(staffId);
            return def != null ? def.CostForLevel(staff.Level + 1) : PriceScaler.Scale(200, staff.Level);
        }

        public bool Hire(string staffId) => StaffManager.Instance && StaffManager.Instance.Hire(staffId);

        public bool Upgrade(string staffId) => StaffManager.Instance && StaffManager.Instance.Upgrade(staffId);

        public IEnumerable<StaffBase> AllStaff
            => StaffManager.Instance ? StaffManager.Instance.All.Values : Enumerable.Empty<StaffBase>();
    }
}
