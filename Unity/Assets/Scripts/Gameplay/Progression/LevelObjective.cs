using System;
using UnityEngine;

namespace BurgerRush.Gameplay.Progression
{
    public enum ObjectiveType { ServeOrders, EarnCoins, ZeroLoss, ServeVIP, NoBurntFood }

    /// <summary>Bölüm hedefi: tip, hedef miktar ve tamamlanma durumu.</summary>
    [Serializable]
    public class LevelObjective
    {
        public ObjectiveType Type;
        public int TargetAmount = 8;
        public int CurrentAmount;
        public string Description;

        public bool IsComplete => CurrentAmount >= TargetAmount;
        public float Progress => Mathf.Clamp01(CurrentAmount / (float)Mathf.Max(1, TargetAmount));

        public void Add(int amount = 1) => CurrentAmount = Mathf.Min(TargetAmount, CurrentAmount + amount);

        public static LevelObjective Serve(int amount) => new()
        {
            Type = ObjectiveType.ServeOrders,
            TargetAmount = amount,
            Description = $"{amount} sipariş teslim et",
        };

        public static LevelObjective Earn(int coins) => new()
        {
            Type = ObjectiveType.EarnCoins,
            TargetAmount = coins,
            Description = $"{coins} 🪙 kazan",
        };
    }
}
