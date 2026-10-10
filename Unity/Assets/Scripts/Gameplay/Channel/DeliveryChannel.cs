using System.Collections.Generic;
using UnityEngine;

namespace BurgerRush.Gameplay.Channel
{
    /// <summary>
    /// Üçüncü kanal: paket servis (Level 27+, GDD 3.1).
    /// Siparişler kuyruğa girmez, zamanlayıcı ile birikir.
    /// </summary>
    public class DeliveryChannel : BaseChannel
    {
        [SerializeField] private int maxActiveOrders = 3;
        [SerializeField] private float orderInterval = 25f;

        private readonly List<Data.OrderTicket> pending = new();
        private float timer;

        public override string ChannelId => "delivery";
        public IReadOnlyList<Data.OrderTicket> Pending => pending;

        private void Update()
        {
            if (!IsUnlocked) return;
            timer -= Time.deltaTime;
            if (timer > 0f) return;

            timer = orderInterval;
            if (pending.Count < maxActiveOrders) pending.Add(new Data.OrderTicket());
        }

        public override void Setup(Data.LevelData level, bool unlocked)
        {
            base.Setup(level, unlocked);
            pending.Clear();
            timer = orderInterval;
        }
    }
}
