using BurgerRush.Audio;
using BurgerRush.Economy;
using UnityEngine;

namespace BurgerRush.Gameplay.Channel
{
    /// <summary>
    /// Drive-Thru kanalı: Level 8'de açılır, sabır %30 daha hızlı tükenir,
    /// her tamamlanan araç kalıcı çarpan ekler (GDD 2.2).
    /// </summary>
    public class DriveThruChannel : BaseChannel
    {
        [SerializeField] private int maxCars = 4;
        [SerializeField] private float patiencePenalty = 1.3f;

        private int completedCars;
        private float bonusMultiplier = 1f;

        public override string ChannelId => "driveThru";
        public int CompletedCars => completedCars;
        public float PatiencePenalty => patiencePenalty;

        public override void Setup(Data.LevelData level, bool unlocked)
        {
            base.Setup(level, unlocked);
            if (!IsUnlocked) return;
            completedCars = 0;
            bonusMultiplier = 1f;
        }

        public void RegisterCarCompleted()
        {
            completedCars++;
            bonusMultiplier = 1f + Mathf.Min(1f, completedCars * 0.08f);
            AudioManager.Instance?.PlaySFX("DriveThruBell");
        }

        public override float IncomeMultiplier
            => bonusMultiplier * (1f + (UpgradeSystem.Instance ? UpgradeSystem.Instance.DriveThruBonus : 0f));
    }
}
