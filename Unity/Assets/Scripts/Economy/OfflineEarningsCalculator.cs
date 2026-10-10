using System;
using BurgerRush.Data;
using UnityEngine;

namespace BurgerRush.Economy
{
    /// <summary>
    /// Offline gelir: max 8 saat, reklamla 16 saat; verim %15 / %45 / %70 (GDD 2.7).
    /// </summary>
    public class OfflineEarningsCalculator : Singleton<OfflineEarningsCalculator>
    {
        public struct OfflineEarningsResult
        {
            public float Earnings;
            public float HoursElapsed;
            public float RealHours;
            public float HourlyRate;
            public float Efficiency;
            public bool CanDoubleWithAd;
        }

        public OfflineEarningsResult Calculate()
        {
            var data = SaveManager.Instance.Data;
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long last = data.lastOfflineTime > 0 ? data.lastOfflineTime : now;

            double elapsedMs = Math.Max(0, now - last);
            float realHours = (float)(elapsedMs / 3_600_000d);
            float clampedHours = Mathf.Min(realHours, Constants.Economy.MaxOfflineHours);

            float hourly = EconomyManager.Instance ? EconomyManager.Instance.CalculateHourlyEarning() : 0f;
            float efficiency = GetEfficiency();
            float earnings = Mathf.Floor(hourly * clampedHours * efficiency);

            data.offlineEarnings = earnings;
            data.lastOfflineTime = now;
            SaveManager.Instance.Save();

            return new OfflineEarningsResult
            {
                Earnings = earnings,
                HoursElapsed = clampedHours,
                RealHours = realHours,
                HourlyRate = hourly,
                Efficiency = efficiency,
                CanDoubleWithAd = earnings > 0f,
            };
        }

        private float GetEfficiency()
        {
            int staff = Gameplay.Staff.StaffManager.Instance ? Gameplay.Staff.StaffManager.Instance.ActiveStaffCount : 0;
            if (staff == 0) return Constants.Economy.OfflineNoStaff;
            return UpgradeSystem.Instance && UpgradeSystem.Instance.IsFullyUpgraded
                ? Constants.Economy.OfflineFullUpgrade
                : Constants.Economy.OfflineWithStaff;
        }

        /// <summary>Ödüllü reklam: 8 saatlik tavanı 16 saate çıkarır.</summary>
        public float RecalculateWithAdBonus()
        {
            var data = SaveManager.Instance.Data;
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            float realHours = Mathf.Max(0f, (now - data.lastOfflineTime) / 3_600_000f);
            float hours = Mathf.Min(realHours, Constants.Economy.MaxOfflineHoursWithAd);

            float hourly = EconomyManager.Instance ? EconomyManager.Instance.CalculateHourlyEarning() : 0f;
            float earnings = Mathf.Floor(hourly * hours * GetEfficiency());

            data.offlineEarnings = earnings;
            SaveManager.Instance.Save();
            return earnings;
        }

        public float Claim(bool doubled = false)
        {
            var data = SaveManager.Instance.Data;
            float baseAmount = data.offlineEarnings;
            if (baseAmount <= 0f) return 0f;

            float amount = doubled ? baseAmount * 2f : baseAmount;
            EconomyManager.Instance?.AddCoins(amount);

            data.offlineEarnings = 0f;
            data.lastOfflineTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            SaveManager.Instance.Save();
            return amount;
        }
    }
}
