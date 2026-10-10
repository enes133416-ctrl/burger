using System;
using BurgerRush.Gameplay.Customer;

namespace BurgerRush.Data
{
    /// <summary>Delivery kanalındaki paket servis fişi.</summary>
    [Serializable]
    public class OrderTicket
    {
        public string Id;
        public OrderData Order;
        public float RemainingTime = 90f;
        public bool Delivered;

        public OrderTicket()
        {
            Id = Guid.NewGuid().ToString("N")[..6];
        }

        public OrderTicket(OrderData order, float time = 90f)
        {
            Id = Guid.NewGuid().ToString("N")[..6];
            Order = order;
            RemainingTime = time;
        }
    }
}
