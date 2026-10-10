using System.Collections.Generic;
using BurgerRush.Core;
using UnityEngine;

namespace BurgerRush.Gameplay.Customer
{
    /// <summary>
    /// Tüm müşterilerin kaydı, servis eşleştirmesi ve istatistikleri (GDD 6.1).
    /// </summary>
    public class CustomerManager : Utils.Singleton<CustomerManager>
    {
        [SerializeField] private CustomerSpawner spawner;
        [SerializeField] private Channel.ChannelManager channelManager;

        private readonly List<CustomerAI> active = new();

        public int ServedCount { get; private set; }
        public int LostCount { get; private set; }
        public int ComplaintCount { get; private set; }
        public int ActiveCount => active.Count;

        private void OnEnable()
        {
            EventBus.Subscribe<CustomerServedEvent>(OnServed);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<CustomerServedEvent>(OnServed);
        }

        private void OnServed(CustomerServedEvent e)
        {
            ServedCount++;
            if (e.WasWrong) ComplaintCount++;
            Unregister(e.Customer);
        }

        public void Register(CustomerAI customer)
        {
            if (!customer || active.Contains(customer)) return;
            active.Add(customer);
            customer.OnCustomerLeave += HandleLeave;
        }

        public void Unregister(CustomerAI customer)
        {
            if (!customer) return;
            customer.OnCustomerLeave -= HandleLeave;
            active.Remove(customer);
            channelManager?.Release(customer);
        }

        private void HandleLeave(CustomerAI customer)
        {
            if (customer && customer.CurrentState == CustomerAI.CustomerState.Angry) LostCount++;
            Unregister(customer);
        }

        public void BeginLevel(Data.LevelData level)
        {
            active.Clear();
            ServedCount = 0;
            LostCount = 0;
            ComplaintCount = 0;
            spawner?.Begin(level);
            channelManager?.Setup(level);
        }

        /// <summary>Bekleyen müşteriler arasından siparişe uyan ilk müşteri (garson otomasyonu).</summary>
        public CustomerAI FindMatch(Kitchen.BurgerBuilder builder)
        {
            CustomerAI best = null;
            foreach (var c in active)
            {
                if (!c.IsWaiting || !builder.Matches(c.CurrentOrder)) continue;
                if (best == null || c.PatienceRatio < best.PatienceRatio) best = c;
            }
            return best;
        }

        public void ResetAllPatience()
        {
            foreach (var c in active) c.ResetPatience();
        }
    }
}
