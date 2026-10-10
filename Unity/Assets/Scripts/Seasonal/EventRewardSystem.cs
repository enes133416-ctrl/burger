using System.Collections.Generic;
using BurgerRush.Data;
using BurgerRush.Economy;
using UnityEngine;

namespace BurgerRush.Seasonal
{
    /// <summary>
    /// Etkinlik görevleri ve token mağazası (GDD 10.2).
    /// </summary>
    public class EventRewardSystem : Utils.Singleton<EventRewardSystem>
    {
        [System.Serializable]
        public class EventTask
        {
            public string Id;
            public string Description;
            public int Target;
            public int Progress;
            public int TokenReward;
            public bool Claimed;
            public bool IsComplete => Progress >= Target;
        }

        [SerializeField] private List<EventTask> dailyTasks = new()
        {
            new EventTask { Id = "sell50", Description = "50 burger sat", Target = 50, TokenReward = 500 },
            new EventTask { Id = "noloss", Description = "0 müşteri kaybet", Target = 1, TokenReward = 300 },
            new EventTask { Id = "threestar", Description = "3 yıldızlı bölüm", Target = 1, TokenReward = 200 },
        };

        public int Tokens { get; private set; }
        public IReadOnlyList<EventTask> Tasks => dailyTasks;

        public void BeginSeason(SeasonId season)
        {
            foreach (var task in dailyTasks)
            {
                task.Progress = 0;
                task.Claimed = false;
            }
        }

        public void AddProgress(string taskId, int amount = 1)
        {
            var task = dailyTasks.Find(t => t.Id == taskId);
            if (task == null || task.Claimed) return;
            task.Progress = Mathf.Min(task.Target, task.Progress + amount);
        }

        public bool Claim(string taskId)
        {
            var task = dailyTasks.Find(t => t.Id == taskId);
            if (task == null || task.Claimed || !task.IsComplete) return false;

            task.Claimed = true;
            Tokens += task.TokenReward;
            return true;
        }

        public bool Buy(string itemId, int cost)
        {
            if (Tokens < cost) return false;
            Tokens -= cost;
            ApplyItem(itemId);
            return true;
        }

        private void ApplyItem(string itemId)
        {
            var data = SaveManager.Instance.Data;
            switch (itemId)
            {
                case "coins":
                    EconomyManager.Instance.AddCoins(10000);
                    break;
                default:
                    if (!data.unlockedCosmetics.Contains(itemId)) data.unlockedCosmetics.Add(itemId);
                    SaveManager.Instance.Save();
                    break;
            }
        }
    }
}
