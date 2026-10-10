using UnityEngine;

namespace BurgerRush.Gameplay.Staff
{
    /// <summary>Temizlikçi: müşteri sabrını yavaşlatır (GDD 2.6).</summary>
    public class Cleaner : StaffBase
    {
        [SerializeField] private float[] patienceBonus = { 0.08f, 0.16f, 0.25f, 0.35f, 0.50f };

        public float PatienceBonus => hired ? patienceBonus[Mathf.Clamp(level - 1, 0, patienceBonus.Length - 1)] : 0f;
    }
}
