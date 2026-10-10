using BurgerRush.Economy;
using UnityEngine;

namespace BurgerRush.Gameplay.Staff
{
    /// <summary>Kasiyer: sipariş alır, ödeme tahsil eder → gelir bonusu (GDD 2.6).</summary>
    public class Cashier : StaffBase
    {
        [SerializeField] private float[] incomeBonus = { 0.10f, 0.20f, 0.30f, 0.45f, 0.70f };

        public float IncomeBonus => hired ? incomeBonus[Mathf.Clamp(level - 1, 0, incomeBonus.Length - 1)] : 0f;

        protected override void OnHired() => EconomyManager.Instance?.MarkStaffDirty();
        protected override void OnUpgraded() => EconomyManager.Instance?.MarkStaffDirty();
    }
}
