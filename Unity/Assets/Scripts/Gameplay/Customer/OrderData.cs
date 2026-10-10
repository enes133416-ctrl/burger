using System;
using System.Collections.Generic;
using BurgerRush.Gameplay.Kitchen;
using UnityEngine;

namespace BurgerRush.Gameplay.Customer
{
    /// <summary>Müşteri tipleri ve sabır/ödeme katsayıları (GDD 2.5).</summary>
    public enum CustomerType { Casual, BusinessPerson, Child, VIP, Influencer }

    [Serializable]
    public class OrderData
    {
        public List<IngredientType> Fillings = new();
        public float BasePrice = 25f;
        public bool IsCombo;
        public CustomerType OwnerType;

        public string DisplayText
        {
            get
            {
                var parts = new List<string>();
                foreach (var f in Fillings) parts.Add(Emoji(f));
                if (IsCombo) parts.Add("🍟");
                return string.Join(" ", parts);
            }
        }

        public static string Emoji(IngredientType type) => type switch
        {
            IngredientType.Bun => "🍞",
            IngredientType.Patty => "🥩",
            IngredientType.Cheese => "🧈",
            IngredientType.Lettuce => "🥬",
            IngredientType.Tomato => "🍅",
            IngredientType.Sauce => "🫗",
            _ => "❓",
        };

        /// <summary>Bölüm verisine göre rastgele sipariş üretir.</summary>
        public static OrderData Generate(Data.LevelData level, CustomerType type)
        {
            var order = new OrderData { OwnerType = type };
            if (level == null) return order;

            var pool = new List<IngredientType>(level.availableIngredients);
            pool.Remove(IngredientType.Bun);
            pool.Shuffle();

            int count = Mathf.Clamp(UnityEngine.Random.Range(1, level.maxFillingCount + 1), 1, pool.Count);
            for (int i = 0; i < count && i < pool.Count; i++) order.Fillings.Add(pool[i]);
            if (!order.Fillings.Contains(IngredientType.Patty)) order.Fillings.Insert(0, IngredientType.Patty);

            order.IsCombo = UnityEngine.Random.value < level.comboOrderChance;

            float baseValue = 0f;
            foreach (var f in order.Fillings) baseValue += KitchenManager.IngredientValue(f);

            float payMultiplier = type switch
            {
                CustomerType.BusinessPerson => 1.30f,
                CustomerType.Child => 0.70f,
                CustomerType.VIP => 2.00f,
                CustomerType.Influencer => 1.15f,
                _ => 1.00f,
            };

            order.BasePrice = Mathf.Round((8f + baseValue * 3.2f) * level.priceScale * payMultiplier * (order.IsCombo ? 1.5f : 1f));
            return order;
        }
    }
}
