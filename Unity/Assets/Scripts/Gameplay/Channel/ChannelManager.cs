using System.Collections.Generic;
using BurgerRush.Utils;
using UnityEngine;

namespace BurgerRush.Gameplay.Channel
{
    /// <summary>
    /// Counter + Drive-Thru (+ Delivery) kanallarının kaydı (GDD 2.2).
    /// </summary>
    public class ChannelManager : MonoBehaviour
    {
        [SerializeField] private CounterChannel counter;
        [SerializeField] private DriveThruChannel driveThru;
        [SerializeField] private DeliveryChannel delivery;

        private readonly Dictionary<string, BaseChannel> channels = new();

        public CounterChannel Counter => counter;
        public DriveThruChannel DriveThru => driveThru;
        public DeliveryChannel Delivery => delivery;

        private void Awake()
        {
            Register(counter);
            Register(driveThru);
            Register(delivery);
        }

        private void Register(BaseChannel channel)
        {
            if (!channel) return;
            channels[channel.ChannelId] = channel;
        }

        public void Setup(Data.LevelData level)
        {
            if (!level) return;
            counter?.Setup(level);
            driveThru?.Setup(level, level.driveThruEnabled);
            delivery?.Setup(level, level.deliveryEnabled);
        }

        /// <summary>Yer olan ilk kanal (rastgele ağırlıklı).</summary>
        public BaseChannel GetChannelWithRoom()
        {
            bool dtReady = driveThru && driveThru.IsUnlocked && driveThru.HasRoom;
            bool counterReady = counter && counter.HasRoom;

            if (dtReady && (!counterReady || Random.value < 0.35f)) return driveThru;
            if (counterReady) return counter;
            return dtReady ? driveThru : null;
        }

        public BaseChannel Get(string id) => channels.TryGetValue(id, out var c) ? c : null;

        public void Release(Customer.CustomerAI customer)
        {
            if (!customer) return;
            foreach (var channel in channels.Values)
                channel.Queue.Remove(customer);
        }
    }
}
