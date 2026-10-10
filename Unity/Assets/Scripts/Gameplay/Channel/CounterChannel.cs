using BurgerRush.Economy;
using UnityEngine;

namespace BurgerRush.Gameplay.Channel
{
    /// <summary>İç mekan tezgah kanalı (GDD 2.2).</summary>
    public class CounterChannel : BaseChannel
    {
        [SerializeField] private int baseQueueSize = 3;

        public override string ChannelId => "counter";

        public override void Setup(Data.LevelData level)
        {
            base.Setup(level);
            IsUnlocked = true;
            if (!level) return;

            int bonus = UpgradeSystem.Instance ? UpgradeSystem.Instance.QueueBonus : 0;
            int size = Mathf.Clamp(level.queueSize + bonus, 3, 8);
            ResizeQueue(size);
        }

        private void ResizeQueue(int size)
        {
            // Kuyruk slot objeleri sahnede hazır ise görünürlük ile ölçeklenir.
            if (queue) queue.Clear();
            baseQueueSize = size;
        }
    }
}
