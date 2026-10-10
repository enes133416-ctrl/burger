using System;
using System.Collections.Generic;
using BurgerRush.Data;
using BurgerRush.Economy;
using UnityEngine;

namespace BurgerRush.Monetization
{
    /// <summary>
    /// Uygulama içi satın alımlar (GDD 4.3).
    /// Unity IAP (UnityEngine.Purchasing) entegrasyonu için hazır iskelet.
    /// </summary>
    public class IAPManager : Utils.Singleton<IAPManager>
    {
        public enum ProductId
        {
            StarterPack, ChefPass,
            Gems50, Gems150, Gems500, Gems1200, Gems3500,
            ThemeCyberpunk, ThemeVintage, ThemeTokyo, ThemeTropical, ThemeSpace,
            SkinNinja, SkinRobot, SkinViking, SkinAstronaut
        }

        private static readonly Dictionary<ProductId, (int coins, int gems, string cosmetic)> Products = new()
        {
            { ProductId.StarterPack, (10000, 50, "gold") },
            { ProductId.ChefPass, (2000, 0, null) },
            { ProductId.Gems50, (0, 50, null) },
            { ProductId.Gems150, (0, 150, null) },
            { ProductId.Gems500, (0, 500, null) },
            { ProductId.Gems1200, (0, 1200, null) },
            { ProductId.Gems3500, (0, 3500, null) },
            { ProductId.ThemeCyberpunk, (0, 0, "cyberpunk") },
            { ProductId.ThemeVintage, (0, 0, "vintage") },
            { ProductId.ThemeTokyo, (0, 0, "tokyo") },
            { ProductId.ThemeTropical, (0, 0, "tropical") },
            { ProductId.ThemeSpace, (0, 0, "space") },
            { ProductId.SkinNinja, (0, 0, "ninja") },
            { ProductId.SkinRobot, (0, 0, "robot") },
            { ProductId.SkinViking, (0, 0, "viking") },
            { ProductId.SkinAstronaut, (0, 0, "astronaut") },
        };

        public event Action<ProductId> OnPurchaseComplete;

        public void Initialize()
        {
            // var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            // builder.AddProduct("burger_rush_starter_pack", ProductType.NonConsumable);
            // UnityPurchasing.Initialize(this, builder);
            Debug.Log("[IAPManager] Hazır (Unity IAP iskeleti)");
        }

        /// <summary>Satın alma akışı (test için doğrudan çağrılabilir).</summary>
        public bool Purchase(ProductId productId)
        {
            if (!Products.TryGetValue(productId, out var reward)) return false;
            Grant(reward.coins, reward.gems, reward.cosmetic);

            Analytics.AnalyticsManager.Instance.TrackIAP(productId.ToString(), PriceLabel(productId));
            OnPurchaseComplete?.Invoke(productId);
            return true;
        }

        private void Grant(int coins, int gems, string cosmetic)
        {
            if (coins > 0) EconomyManager.Instance.AddCoins(coins);
            if (gems > 0) EconomyManager.Instance.AddGems(gems);

            if (!string.IsNullOrEmpty(cosmetic))
            {
                var data = SaveManager.Instance.Data;
                if (!data.unlockedCosmetics.Contains(cosmetic)) data.unlockedCosmetics.Add(cosmetic);
                SaveManager.Instance.Save();
            }

            if (coins == 2000 && gems == 0 && cosmetic == null)   // Chef Pass
            {
                var data = SaveManager.Instance.Data;
                data.hasChefPass = true;
                data.chefPassExpiry = DateTimeOffset.UtcNow.AddDays(7).ToUnixTimeMilliseconds();
                SaveManager.Instance.Save();
            }
        }

        public static string PriceLabel(ProductId id) => id switch
        {
            ProductId.StarterPack => "$1.99",
            ProductId.ChefPass => "$4.99",
            ProductId.Gems50 => "$0.99",
            ProductId.Gems150 => "$2.99",
            ProductId.Gems500 => "$7.99",
            ProductId.Gems1200 => "$14.99",
            ProductId.Gems3500 => "$34.99",
            _ => "$2.99",
        };
    }
}
