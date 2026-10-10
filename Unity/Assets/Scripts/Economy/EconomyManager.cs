using BurgerRush.Core;
using BurgerRush.Data;
using BurgerRush.Utils;
using UnityEngine;

namespace BurgerRush.Economy
{
    /// <summary>
    /// Para, gem ve gelir çarpanları (GDD 2.7 / 4).
    /// </summary>
    public class EconomyManager : Singleton<EconomyManager>
    {
        [SerializeField] private float doubleIncomeTimer;
        [SerializeField] private float chefPassMultiplier = 1.10f;

        public float Coins => SaveManager.Instance.Data.totalCoins;
        public float Gems => SaveManager.Instance.Data.totalGems;
        public bool IsDoubleIncomeActive => doubleIncomeTimer > 0f;
        public float DoubleIncomeRemaining => doubleIncomeTimer;

        private float cachedStaffMultiplier = 1f;
        private bool staffDirty = true;

        public void MarkStaffDirty() => staffDirty = true;

        private void Update()
        {
            if (doubleIncomeTimer > 0f) doubleIncomeTimer -= Time.deltaTime;
        }

        // ─────────── Bakiye ───────────
        public void AddCoins(float amount)
        {
            if (amount <= 0f) return;
            var data = SaveManager.Instance.Data;
            data.totalCoins += Mathf.Round(amount);
            EventBus.Publish(new CoinChangedEvent { NewAmount = data.totalCoins, Delta = amount });
            SaveManager.Instance.Save();
        }

        public bool DeductCoins(float amount)
        {
            var data = SaveManager.Instance.Data;
            if (data.totalCoins < amount) return false;
            data.totalCoins -= Mathf.Round(amount);
            EventBus.Publish(new CoinChangedEvent { NewAmount = data.totalCoins, Delta = -amount });
            SaveManager.Instance.Save();
            return true;
        }

        public bool SpendCoins(float amount) => DeductCoins(amount);

        public void AddGems(int amount)
        {
            var data = SaveManager.Instance.Data;
            data.totalGems += amount;
            EventBus.Publish(new GemChangedEvent { NewAmount = data.totalGems, Delta = amount });
            SaveManager.Instance.Save();
        }

        public bool SpendGems(int amount)
        {
            var data = SaveManager.Instance.Data;
            if (data.totalGems < amount) return false;
            data.totalGems -= amount;
            EventBus.Publish(new GemChangedEvent { NewAmount = data.totalGems, Delta = -amount });
            SaveManager.Instance.Save();
            return true;
        }

        // ─────────── Çarpanlar ───────────
        public void ActivateDoubleIncome(float seconds = 180f)
            => doubleIncomeTimer = Mathf.Max(doubleIncomeTimer, seconds);

        public float TotalIncomeMultiplier
        {
            get
            {
                float multiplier = cachedStaffMultiplier;
                if (IsDoubleIncomeActive) multiplier *= 2f;

                var data = SaveManager.Instance.Data;
                if (data.hasChefPass && data.chefPassExpiry > Now()) multiplier *= chefPassMultiplier;

                return multiplier;
            }
        }

        /// <summary>Personel/ekipman değiştiğinde çağrılır; çarpanı yeniden hesaplar.</summary>
        public void RefreshMultipliers()
        {
            float multiplier = 1f;
            multiplier += UpgradeSystem.Instance.IncomeMultiplier - 1f;
            if (Gameplay.Staff.StaffManager.Instance)
                multiplier += Gameplay.Staff.StaffManager.Instance.IncomeBonus;

            cachedStaffMultiplier = multiplier;
            staffDirty = false;
        }

        private void LateUpdate()
        {
            if (staffDirty) RefreshMultipliers();
        }

        // ─────────── Offline gelir için saatlik tahmin ───────────
        public float CalculateHourlyEarning()
        {
            float baseIncome = 40f * Mathf.Pow(1.18f, SaveManager.Instance.Data.currentLevel - 1);
            return baseIncome * TotalIncomeMultiplier;
        }

        private static long Now() => System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}
