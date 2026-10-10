using System;
using System.Collections.Generic;
using BurgerRush.Gameplay.Customer;

namespace BurgerRush.Core
{
    /// <summary>
    /// Bağımlılıksız iletişim için merkezi event sistemi (GDD 8.2).
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> Handlers = new();

        public static void Subscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (!Handlers.ContainsKey(type)) Handlers[type] = new List<Delegate>();
            if (!Handlers[type].Contains(handler)) Handlers[type].Add(handler);
        }

        public static void Unsubscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (Handlers.TryGetValue(type, out var list)) list.Remove(handler);
        }

        public static void Publish<T>(T eventData)
        {
            var type = typeof(T);
            if (!Handlers.TryGetValue(type, out var list)) return;

            for (int i = list.Count - 1; i >= 0; i--)
            {
                try { (list[i] as Action<T>)?.Invoke(eventData); }
                catch (Exception e) { UnityEngine.Debug.LogException(e); }
            }
        }

        public static void Clear() => Handlers.Clear();
    }

    // ─────────── Event veri yapıları ───────────
    public struct CustomerServedEvent { public CustomerAI Customer; public float Tip; public bool WasWrong; }
    public struct CustomerLeftEvent { public CustomerAI Customer; }
    public struct FoodBurntEvent { public string FoodName; public float LostValue; }
    public struct LevelCompleteEvent { public int Level; public int Stars; public float Coins; }
    public struct LevelFailEvent { public int Level; public string Reason; }
    public struct UpgradePurchasedEvent { public string UpgradeId; public int NewLevel; }
    public struct StaffHiredEvent { public string StaffId; }
    public struct CoinChangedEvent { public float NewAmount; public float Delta; }
    public struct GemChangedEvent { public float NewAmount; public float Delta; }
    public struct StateChangedEvent { public GameManager.GameState State; }
    public struct AdRewardedEvent { public string RewardType; }
}
