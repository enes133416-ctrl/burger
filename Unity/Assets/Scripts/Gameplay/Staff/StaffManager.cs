using System.Collections.Generic;
using BurgerRush.Core;
using BurgerRush.Data;
using BurgerRush.Economy;
using UnityEngine;

namespace BurgerRush.Gameplay.Staff
{
    /// <summary>
    /// Personel kaydı: işe alma, yükseltme, kayıttan geri yükleme (GDD 2.6 / HR Office).
    /// </summary>
    public class StaffManager : Utils.Singleton<StaffManager>
    {
        [SerializeField] private StaffBase[] staffPrefabs;
        [SerializeField] private Transform staffContainer;

        private readonly Dictionary<string, StaffBase> staffById = new();

        public int ActiveStaffCount { get; private set; }

        private void Awake()
        {
            foreach (var prefab in staffPrefabs)
            {
                if (!prefab) continue;
                var instance = Instantiate(prefab, staffContainer ? staffContainer : transform);
                instance.gameObject.SetActive(false);
                staffById[instance.StaffId] = instance;
            }
            RestoreFromSave();
        }

        private void RestoreFromSave()
        {
            var data = SaveManager.Instance.Data;
            foreach (var kv in staffById)
            {
                if (!data.staff.TryGetValue(kv.Key, out var save)) continue;
                if (!save.hired) continue;
                kv.Value.Hire();
                for (int i = 1; i < save.level; i++) kv.Value.Upgrade();
            }
            Recount();
        }

        public IReadOnlyDictionary<string, StaffBase> All => staffById;

        public StaffBase Get(string id) => staffById.TryGetValue(id, out var s) ? s : null;

        public bool Hire(string id)
        {
            var staff = Get(id);
            if (staff == null || staff.IsHired) return false;
            if (!EconomyManager.Instance.SpendCoins(staff.HireCost)) return false;

            staff.Hire();
            Persist(id);
            Recount();
            EventBus.Publish(new StaffHiredEvent { StaffId = id });
            return true;
        }

        public bool Upgrade(string id)
        {
            var staff = Get(id);
            if (staff == null || !staff.IsHired || staff.Level >= 5) return false;
            if (!EconomyManager.Instance.SpendCoins(HROfficeManager.Instance.UpgradeCostFor(id))) return false;

            staff.Upgrade();
            Persist(id);
            EventBus.Publish(new UpgradePurchasedEvent { UpgradeId = id, NewLevel = staff.Level });
            return true;
        }

        private void Persist(string id)
        {
            var staff = Get(id);
            if (staff == null) return;
            SaveManager.Instance.Data.staff[id] = new PlayerData.StaffSave { hired = staff.IsHired, level = staff.Level };
            SaveManager.Instance.Save();
            EconomyManager.Instance.MarkStaffDirty();
        }

        private void Recount()
        {
            int count = 0;
            foreach (var s in staffById.Values) if (s.IsHired) count++;
            ActiveStaffCount = count;
        }

        // ─────────── Türetilmiş bonuslar ───────────
        public float IncomeBonus => Get("cashier") is Cashier c ? c.IncomeBonus : 0f;
        public float PatienceBonus => Get("cleaner") is Cleaner cl ? cl.PatienceBonus : 0f;
        public float DriveThruBonus => Get("driveThruStaff") is DriveThruStaff d ? d.DriveThruBonus : 0f;
        public float TipBonus => Get("waiter") is Waiter w ? w.TipBonus : 0f;
        public float? AutoCookDelay => Get("cook") is Cook ck ? ck.AutoCookDelay : (float?)null;
        public float? AutoServeDelay => Get("waiter") is Waiter w ? w.AutoServeDelay : (float?)null;
    }
}
