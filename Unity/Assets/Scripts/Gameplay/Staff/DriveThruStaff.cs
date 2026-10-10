using UnityEngine;

namespace BurgerRush.Gameplay.Staff
{
    /// <summary>Drive-Thru görevlisi: araç kanalına gelir bonusu (GDD 2.6).</summary>
    public class DriveThruStaff : StaffBase
    {
        [SerializeField] private float[] driveThruBonus = { 0.20f, 0.40f, 0.60f, 0.85f, 1.20f };

        public float DriveThruBonus => hired ? driveThruBonus[Mathf.Clamp(level - 1, 0, driveThruBonus.Length - 1)] : 0f;
    }
}
