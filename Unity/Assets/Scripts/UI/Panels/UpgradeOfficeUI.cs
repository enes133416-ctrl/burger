using System.Collections.Generic;
using BurgerRush.Audio;
using BurgerRush.Economy;
using UnityEngine;
using UnityEngine.UI;

namespace BurgerRush.UI.Panels
{
    /// <summary>11_UpgradeOffice: ızgara / tezgah / kasa yükseltmeleri (GDD 2.6).</summary>
    public class UpgradeOfficeUI : MonoBehaviour
    {
        [System.Serializable]
        public class EquipmentRow
        {
            public string EquipmentId;
            public Text NameText;
            public Text LevelText;
            public Text PerkText;
            public Button ActionButton;
            public Text ActionLabel;
        }

        [SerializeField] private List<EquipmentRow> rows = new();
        [SerializeField] private Text grillSummaryText;
        [SerializeField] private GameObject lockedOverlay;

        private void OnEnable() { Refresh(); if (lockedOverlay) lockedOverlay.SetActive(!UpgradeOfficeManager.Instance.IsUnlocked); }

        public void Refresh()
        {
            foreach (var row in rows)
            {
                int level = UpgradeSystem.Instance.GetEquipmentLevel(row.EquipmentId);
                int cost = UpgradeOfficeManager.Instance.CostFor(row.EquipmentId);
                bool maxed = cost <= 0;

                if (row.LevelText) row.LevelText.text = $"Lv {level}";
                if (row.PerkText) row.PerkText.text = PerkFor(row.EquipmentId, level);

                if (row.ActionButton)
                {
                    row.ActionButton.onClick.RemoveAllListeners();
                    row.ActionButton.interactable = !maxed && EconomyManager.Instance.Coins >= cost;
                    var id = row.EquipmentId;
                    row.ActionButton.onClick.AddListener(() => Upgrade(id));
                }

                if (row.ActionLabel)
                    row.ActionLabel.text = maxed ? "MAX" : $"Yükselt 🪙{cost}";
            }

            if (grillSummaryText) grillSummaryText.text = UpgradeOfficeManager.Instance.GrillSummary();
        }

        private string PerkFor(string id, int level) => id switch
        {
            "grill" => $"{level + 1} slot · daha hızlı pişirme",
            "counter" => $"+{level - 1} kuyruk · hazırlık hızı",
            "cashier_desk" => $"+%{(level - 1) * 8} gelir",
            _ => "",
        };

        private void Upgrade(string id)
        {
            if (UpgradeOfficeManager.Instance.TryUpgrade(id)) AudioManager.Instance?.PlaySFX("PowerUp");
            Refresh();
        }
    }
}
