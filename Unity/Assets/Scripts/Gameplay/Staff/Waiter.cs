using System.Collections.Generic;
using BurgerRush.Gameplay.Customer;
using BurgerRush.Gameplay.Kitchen;
using UnityEngine;

namespace BurgerRush.Gameplay.Staff
{
    /// <summary>Garson: hazır burgeri eşleşen müşteriye otomatik servis eder (GDD 2.6).</summary>
    public class Waiter : StaffBase
    {
        [SerializeField] private float[] autoServeDelay = { 3.5f, 2.6f, 1.9f, 1.4f, 0.8f };
        [SerializeField] private KitchenManager kitchen;
        [SerializeField] private CustomerManager customers;

        private float timer;

        public float AutoServeDelay => autoServeDelay[Mathf.Clamp(level - 1, 0, autoServeDelay.Length - 1)];
        public bool ServesDriveThru => level >= 4;
        public float TipBonus => level >= 3 ? 0.15f : 0f;

        protected override void DoWork(float deltaTime)
        {
            if (!kitchen || !customers || !kitchen.Assembly) return;

            timer += deltaTime;
            if (timer < AutoServeDelay) return;
            timer = 0f;

            if (!kitchen.Assembly.HasCompleteBurger) return;

            var match = customers.FindMatch(kitchen.Assembly.Builder);
            if (match == null) return;
            if (match.ChannelId == "driveThru" && !ServesDriveThru) return;

            var burger = kitchen.Assembly.TakeBurger();
            if (burger == null) return;
            match.ReceiveOrder(burger);
        }
    }
}
