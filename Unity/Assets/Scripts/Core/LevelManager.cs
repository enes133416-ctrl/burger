using System.Collections.Generic;
using BurgerRush.Data;
using BurgerRush.Gameplay.Progression;
using UnityEngine;

namespace BurgerRush.Core
{
    /// <summary>
    /// Bölüm yaşam döngüsü: hedef takibi, süre, başarı/başarısızlık (GDD 3).
    /// </summary>
    public class LevelManager : Singleton<LevelManager>
    {
        [Header("Current Level")]
        [SerializeField] private LevelData currentLevel;
        [SerializeField] private float timeLeft;
        [SerializeField] private int servedCount;
        [SerializeField] private int lostCount;
        [SerializeField] private int complaintCount;
        [SerializeField] private float earnedCoins;

        private bool running;

        public LevelData CurrentLevel => currentLevel;
        public float TimeLeft => timeLeft;
        public int ServedCount => servedCount;
        public int LostCount => lostCount;
        public float Progress => currentLevel ? Mathf.Clamp01((servedCount + lostCount) / (float)currentLevel.TotalOrders) : 0f;

        public void StartLevel(LevelData level)
        {
            currentLevel = level;
            timeLeft = level.Duration;
            servedCount = 0;
            lostCount = 0;
            complaintCount = 0;
            earnedCoins = 0f;
            running = true;
        }

        private void Update()
        {
            if (!running || currentLevel == null) return;

            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0f) Finish();
        }

        public void RegisterServed(float coins, bool complaint)
        {
            servedCount++;
            earnedCoins += coins;
            if (complaint) complaintCount++;
        }

        public void RegisterLost() => lostCount++;

        public void Finish()
        {
            if (!running) return;
            running = false;

            int stars = StarRatingSystem.Evaluate(currentLevel, servedCount, complaintCount, lostCount);
            GameManager.Instance.ChangeState(GameManager.GameState.LevelComplete);
            EventBus.Publish(new LevelCompleteEvent { Level = currentLevel.Index, Stars = stars, Coins = earnedCoins });
        }

        public void Fail(string reason)
        {
            if (!running) return;
            running = false;
            GameManager.Instance.ChangeState(GameManager.GameState.LevelFail);
            EventBus.Publish(new LevelFailEvent { Level = currentLevel ? currentLevel.Index : 0, Reason = reason });
        }

        /// <summary>Ödüllü reklam sonrası bölüme devam (GDD 4.2).</summary>
        public void ReviveCurrentLevel()
        {
            if (currentLevel == null) return;
            timeLeft = Mathf.Max(timeLeft, 45f);
            running = true;
            GameManager.Instance.ChangeState(GameManager.GameState.Gameplay);
        }

        public void Restart() => StartLevel(currentLevel);

        public Dictionary<string, object> Snapshot() => new()
        {
            { "level", currentLevel ? currentLevel.Index : 0 },
            { "served", servedCount },
            { "lost", lostCount },
            { "complaints", complaintCount },
            { "earned", earnedCoins },
        };
    }
}
