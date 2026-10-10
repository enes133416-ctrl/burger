using System.Collections.Generic;
using BurgerRush.Audio;
using BurgerRush.Economy;
using BurgerRush.Gameplay.Staff;
using UnityEngine;
using UnityEngine.UI;

namespace BurgerRush.UI.Panels
{
    /// <summary>10_HROffice: personel işe alma ve 5 seviye yükseltme (GDD 2.6).</summary>
    public class HROfficeUI : MonoBehaviour
    {
        [System.Serializable]
        public class StaffRow
        {
            public string StaffId;
            public Text NameText;
            public Text LevelText;
            public Text PerkText;
            public Button ActionButton;
            public Text ActionLabel;
            public Image[] LevelPips;
        }

        [SerializeField] private List<StaffRow> rows = new();
        [SerializeField] private Text coinText;
        [SerializeField] private GameObject lockedOverlay;

        private void OnEnable() { Refresh(); if (lockedOverlay) lockedOverlay.SetActive(!HROfficeManager.Instance.IsUnlocked); }

        public void Refresh()
        {
            if (coinText) coinText.text = $"🪙 {(int)EconomyManager.Instance.Coins}";

            foreach (var row in rows)
            {
                var staff = StaffManager.Instance?.Get(row.StaffId);
                if (staff == null) continue;

                if (row.NameText) row.NameText.text = staff.DisplayName;
                if (row.LevelText) row.LevelText.text = staff.IsHired ? $"Lv {staff.Level}" : "İşe alınmadı";
                if (row.PerkText) row.PerkText.text = staff.IsHired
                    ? $"Verim x{staff.Efficiency:0.00}"
                    : $"İşe alım: 🪙{staff.HireCost}";

                for (int i = 0; i < row.LevelPips.Length; i++)
                    if (row.LevelPips[i]) row.LevelPips[i].enabled = staff.IsHired && i < staff.Level;

                if (!row.ActionButton) continue;

                row.ActionButton.onClick.RemoveAllListeners();
                bool maxed = staff.IsHired && staff.Level >= 5;
                int cost = staff.IsHired ? HROfficeManager.Instance.UpgradeCostFor(row.StaffId) : staff.HireCost;

                if (row.ActionLabel)
                    row.ActionLabel.text = maxed ? "MAX" : staff.IsHired ? $"Yükselt 🪙{cost}" : $"İşe al 🪙{cost}";

                row.ActionButton.interactable = !maxed && EconomyManager.Instance.Coins >= cost;
                var id = row.StaffId;
                row.ActionButton.onClick.AddListener(() => OnAction(id));
            }
        }

        private void OnAction(string staffId)
        {
            var staff = StaffManager.Instance?.Get(staffId);
            if (staff == null) return;

            bool ok = staff.IsHired ? HROfficeManager.Instance.Upgrade(staffId) : HROfficeManager.Instance.Hire(staffId);
            if (ok) AudioManager.Instance?.PlaySFX("PowerUp");
            Refresh();
        }
    }
}
