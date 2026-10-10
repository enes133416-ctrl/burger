using UnityEngine;

namespace BurgerRush.Gameplay.Staff
{
    /// <summary>
    /// Tüm personel tiplerinin tabanı: 5 seviye, verim ve otomasyon (GDD 2.6).
    /// </summary>
    public abstract class StaffBase : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] protected string staffId = "staff";
        [SerializeField] protected string displayName = "Personel";
        [SerializeField] protected int hireCost = 500;

        [Header("State")]
        [SerializeField] protected int level = 1;
        [SerializeField] protected bool hired;

        [Header("Visual")]
        [SerializeField] protected Animator animator;
        [SerializeField] protected Transform workStation;

        public string StaffId => staffId;
        public string DisplayName => displayName;
        public int HireCost => hireCost;
        public int Level { get => level; set => level = Mathf.Clamp(value, 1, 5); }
        public bool IsHired => hired;
        public bool IsGoldTier => level >= 5;

        public virtual void Hire()
        {
            hired = true;
            level = 1;
            gameObject.SetActive(true);
            OnHired();
        }

        public virtual void Upgrade()
        {
            if (!hired || level >= 5) return;
            level++;
            OnUpgraded();
        }

        /// <summary>Personelin çalışma verimi (0..1+) — seviyeye göre artar.</summary>
        public virtual float Efficiency => 1f + (level - 1) * 0.15f + (IsGoldTier ? 0.5f : 0f);

        protected virtual void OnHired() { }
        protected virtual void OnUpgraded() { }
        protected virtual void Update() { if (hired) DoWork(Time.deltaTime); }
        protected virtual void DoWork(float deltaTime) { }
    }
}
