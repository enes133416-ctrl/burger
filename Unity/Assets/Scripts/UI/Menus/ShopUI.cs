using System.Collections.Generic;
using BurgerRush.Monetization;
using BurgerRush.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace BurgerRush.UI.Menus
{
    /// <summary>12_Shop: IAP paketleri ve gem paketleri (GDD 4.3).</summary>
    public class ShopUI : MonoBehaviour
    {
        [System.Serializable]
        public class ShopRow
        {
            public IAPManager.ProductId Product;
            public Button Button;
            public Text PriceText;
        }

        [SerializeField] private List<ShopRow> rows = new();
        [SerializeField] private Text gemBalanceText;

        private void Start()
        {
            foreach (var row in rows)
            {
                if (row.PriceText) row.PriceText.text = IAPManager.PriceLabel(row.Product);
                if (!row.Button) continue;

                var product = row.Product;
                row.Button.onClick.AddListener(() => Purchase(product));
            }
            RefreshBalance();
        }

        private void Purchase(IAPManager.ProductId product)
        {
            if (IAPManager.Instance && IAPManager.Instance.Purchase(product)) RefreshBalance();
        }

        private void RefreshBalance()
        {
            if (gemBalanceText)
                gemBalanceText.text = $"💎 {(int)SaveManager.Instance.Data.totalGems}";
        }
    }
}
