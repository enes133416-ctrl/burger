using UnityEngine;

namespace BurgerRush.Economy
{
    /// <summary>
    /// Fiyat ölçekleyici: her seviyede maliyet büyür (GDD 2.6).
    /// Formül: base * growth^(level-1)
    /// </summary>
    public static class PriceScaler
    {
        public const float DefaultGrowth = 2.2f;

        public static int Scale(int basePrice, int level, float growth = DefaultGrowth)
            => Mathf.RoundToInt(basePrice * Mathf.Pow(growth, Mathf.Max(0, level - 1)));

        /// <summary>İndirimli fiyat (Chef Pass vb.).</summary>
        public static int ApplyDiscount(int price, float discountPercent)
            => Mathf.RoundToInt(price * (1f - Mathf.Clamp01(discountPercent)));
    }
}
