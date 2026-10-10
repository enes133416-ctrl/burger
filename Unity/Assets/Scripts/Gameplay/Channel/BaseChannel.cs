using BurgerRush.Gameplay.Customer;
using UnityEngine;

namespace BurgerRush.Gameplay.Channel
{
    /// <summary>Tüm servis kanallarının ortak tabanı.</summary>
    public abstract class BaseChannel : MonoBehaviour
    {
        [SerializeField] protected CustomerQueue queue;
        [SerializeField] protected Transform servicePoint;

        public abstract string ChannelId { get; }
        public CustomerQueue Queue => queue;
        public Transform ServicePoint => servicePoint;
        public bool HasRoom => queue && queue.HasRoom;
        public bool IsUnlocked { get; protected set; }

        public virtual void Setup(Data.LevelData level) { }
        public virtual void Setup(Data.LevelData level, bool unlocked) => IsUnlocked = unlocked;

        public bool Enqueue(CustomerAI customer) => queue && queue.TryEnqueue(customer);

        /// <summary>Kanalın gelir çarpanı (Drive-Thru seri bonusu gibi).</summary>
        public virtual float IncomeMultiplier => 1f;
    }
}
