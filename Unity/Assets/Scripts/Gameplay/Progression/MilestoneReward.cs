using System;
using BurgerRush.Data;
using BurgerRush.Economy;
using UnityEngine;

namespace BurgerRush.Gameplay.Progression
{
    /// <summary>
    /// Dönüm noktası ödülleri: Level 10/20/30/40/50 (GDD 3.1).
    /// </summary>
    [Serializable]
    public class MilestoneReward
    {
        public int Level;
        public int Coins;
        public string UnlockCosmeticId;

        public static MilestoneReward[] Defaults() => new[]
        {
            new MilestoneReward { Level = 10, Coins = 5000,   UnlockCosmeticId = "vintage" },
            new MilestoneReward { Level = 20, Coins = 15000,  UnlockCosmeticId = "ninja" },
            new MilestoneReward { Level = 30, Coins = 40000,  UnlockCosmeticId = "tropical" },
            new MilestoneReward { Level = 40, Coins = 100000, UnlockCosmeticId = "robot" },
            new MilestoneReward { Level = 50, Coins = 500000, UnlockCosmeticId = "space" },
        };

        public void Grant()
        {
            EconomyManager.Instance?.AddCoins(Coins);

            if (string.IsNullOrEmpty(UnlockCosmeticId)) return;
            var data = SaveManager.Instance.Data;
            if (!data.unlockedCosmetics.Contains(UnlockCosmeticId))
                data.unlockedCosmetics.Add(UnlockCosmeticId);
            SaveManager.Instance.Save();
        }

        public static void TryGrantFor(int level)
        {
            foreach (var reward in Defaults())
                if (reward.Level == level) reward.Grant();
        }
    }
}
