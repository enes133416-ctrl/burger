using System;
using System.Collections.Generic;
using UnityEngine;

namespace BurgerRush.Data
{
    public enum LocationId { Downtown, Beach, Rooftop, Mountain, Tokyo, FoodTruck, Space }

    /// <summary>
    /// Bir bölümün tüm tanımı (ScriptableObject → Inspector'dan düzenlenir).
    /// </summary>
    [CreateAssetMenu(menuName = "Burger Rush/Level Data", fileName = "LevelData")]
    public class LevelData : ScriptableObject
    {
        [Header("Identity")]
        public int index = 1;
        public string levelName = "Bölüm 1";
        public LocationId location = LocationId.Downtown;

        [Header("Objectives")]
        public int totalOrders = 8;
        public int minServeToPass = 5;
        public float duration = 100f;

        [Header("Spawning")]
        public float spawnInterval = 5f;
        public int queueSize = 3;
        public CustomerType[] customerTypes = { CustomerType.Casual };

        [Header("Kitchen")]
        public IngredientType[] availableIngredients = { IngredientType.Bun, IngredientType.Patty };
        public int maxFillingCount = 1;
        [Range(0f, 1f)] public float comboOrderChance;

        [Header("Channels")]
        public bool driveThruEnabled;
        public bool deliveryEnabled;

        [Header("Economy")]
        public float priceScale = 1f;

        public bool IsBoss => Array.IndexOf(new[] { 5, 15, 25, 35, 45 }, index) >= 0;
        public bool IsMilestone => Array.IndexOf(new[] { 10, 20, 30, 40, 50 }, index) >= 0;

        public int TotalOrders => totalOrders;

        /// <summary>Prosedürel varsayılan üretim (ScriptableObject oluşturulmadıysa).</summary>
        public static LevelData Procedural(int index)
        {
            var data = CreateInstance<LevelData>();
            data.index = index;
            data.levelName = $"Bölüm {index}";
            data.location = (LocationId)Mathf.Clamp((index - 1) / 10, 0, 6);
            data.totalOrders = Mathf.Min(30, 6 + Mathf.RoundToInt(index * 1.1f));
            data.minServeToPass = Mathf.CeilToInt(data.totalOrders * (data.IsBoss ? 0.75f : 0.6f));
            data.duration = Mathf.Min(210, 95 + index * 4);
            data.spawnInterval = Mathf.Max(2.2f, 6.5f - index * 0.07f);
            data.queueSize = Mathf.Min(8, 3 + index / 2);
            data.priceScale = 1f + (index - 1) * 0.08f;
            data.maxFillingCount = Mathf.Min(4, 1 + index / 4);
            data.driveThruEnabled = index >= 8;

            var ingredients = new List<IngredientType> { IngredientType.Bun, IngredientType.Patty };
            if (index >= 2) ingredients.Add(IngredientType.Cheese);
            if (index >= 4) { ingredients.Add(IngredientType.Lettuce); ingredients.Add(IngredientType.Tomato); }
            if (index >= 7) ingredients.Add(IngredientType.Sauce);
            data.availableIngredients = ingredients.ToArray();

            var types = new List<CustomerType> { CustomerType.Casual };
            if (index >= 2) types.Add(CustomerType.BusinessPerson);
            if (index >= 5) types.Add(CustomerType.Child);
            if (index >= 13) types.Add(CustomerType.VIP);
            if (index >= 17) types.Add(CustomerType.Influencer);
            data.customerTypes = types.ToArray();
            return data;
        }
    }
}
