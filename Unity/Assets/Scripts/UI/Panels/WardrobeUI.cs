using System.Collections.Generic;
using BurgerRush.Data;
using BurgerRush.Economy;
using BurgerRush.Seasonal;
using UnityEngine;
using UnityEngine.UI;

namespace BurgerRush.UI.Panels
{
    /// <summary>13_Wardrobe: restoran temaları ve şef kostümleri (GDD 4.3).</summary>
    public class WardrobeUI : MonoBehaviour
    {
        [System.Serializable]
        public class CosmeticRow
        {
            public string Id;
            public bool IsTheme;
            public Text NameText;
            public Text PriceText;
            public Button ActionButton;
            public Text ActionLabel;
        }

        [SerializeField] private List<CosmeticRow> rows = new();
        [SerializeField] private CosmeticData cosmeticData;
        [SerializeField] private ThemeManager themeManager;

        private void OnEnable() => Refresh();

        public void Refresh()
        {
            var data = SaveManager.Instance.Data;

            foreach (var row in rows)
            {
                bool owned = data.unlockedCosmetics.Contains(row.Id);
                bool equipped = row.IsTheme ? data.equippedTheme == row.Id : data.equippedSkin == row.Id;
                int price = PriceFor(row.Id, row.IsTheme);

                if (row.NameText) row.NameText.text = DisplayName(row.Id, row.IsTheme);
                if (row.PriceText) row.PriceText.text = owned ? "Sahipsin" : $"💎 {price}";

                if (!row.ActionButton) continue;

                row.ActionButton.onClick.RemoveAllListeners();
                var current = row;
                if (equipped)
                {
                    if (row.ActionLabel) row.ActionLabel.text = "KUŞANILDI";
                    row.ActionButton.interactable = false;
                }
                else if (owned)
                {
                    if (row.ActionLabel) row.ActionLabel.text = "Kuşan";
                    row.ActionButton.interactable = true;
                    row.ActionButton.onClick.AddListener(() => Equip(current));
                }
                else
                {
                    if (row.ActionLabel) row.ActionLabel.text = $"Satın al 💎{price}";
                    row.ActionButton.interactable = EconomyManager.Instance.Gems >= price;
                    row.ActionButton.onClick.AddListener(() => Buy(current));
                }
            }
        }

        private int PriceFor(string id, bool isTheme)
        {
            if (!cosmeticData) return 50;
            if (isTheme)
            {
                var t = System.Array.Find(cosmeticData.themes, x => x && x.id == id);
                return t != null ? t.gemPrice : 50;
            }
            var s = System.Array.Find(cosmeticData.skins, x => x && x.id == id);
            return s != null ? s.gemPrice : 25;
        }

        private string DisplayName(string id, bool isTheme)
        {
            if (!cosmeticData) return id;
            if (isTheme)
            {
                var t = System.Array.Find(cosmeticData.themes, x => x && x.id == id);
                return t != null ? t.displayName : id;
            }
            var s = System.Array.Find(cosmeticData.skins, x => x && x.id == id);
            return s != null ? s.displayName : id;
        }

        private void Buy(CosmeticRow row)
        {
            if (!EconomyManager.Instance.SpendGems(PriceFor(row.Id, row.IsTheme))) return;

            var data = SaveManager.Instance.Data;
            if (!data.unlockedCosmetics.Contains(row.Id)) data.unlockedCosmetics.Add(row.Id);
            SaveManager.Instance.Save();
            Equip(row);
        }

        private void Equip(CosmeticRow row)
        {
            var data = SaveManager.Instance.Data;
            if (row.IsTheme)
            {
                data.equippedTheme = row.Id;
                if (themeManager) themeManager.ApplyTheme(row.Id);
            }
            else
            {
                data.equippedSkin = row.Id;
            }
            SaveManager.Instance.Save();
            Refresh();
        }
    }
}
