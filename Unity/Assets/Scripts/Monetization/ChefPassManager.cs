using System;
using BurgerRush.Data;
using BurgerRush.Economy;
using BurgerRush.Utils;
using UnityEngine;

namespace BurgerRush.Monetization
{
    /// <summary>
    /// Chef Pass: süre kontrolü, günlük 2.000 🪙 ödülü, reklam sıklığı indirimi (GDD 4.3).
    /// </summary>
    public class ChefPassManager : Singleton<ChefPassManager>
    {
        [SerializeField] private int dailyCoins = 2000;
        [SerializeField] private int durationDays = 7;

        private const string LastClaimKey = "chefpass_last_claim";

        public bool IsActive
        {
            get
            {
                var data = SaveManager.Instance.Data;
                return data.hasChefPass && data.chefPassExpiry > DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }
        }

        public int DaysLeft
        {
            get
            {
                if (!IsActive) return 0;
                var remaining = SaveManager.Instance.Data.chefPassExpiry - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                return Mathf.Max(0, Mathf.CeilToInt(remaining / 86_400_000f));
            }
        }

        public void Grant(int days = -1)
        {
            var data = SaveManager.Instance.Data;
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long baseTime = data.chefPassExpiry > now ? data.chefPassExpiry : now;

            data.hasChefPass = true;
            data.chefPassExpiry = baseTime + (days > 0 ? days : durationDays) * 86_400_000L;
            SaveManager.Instance.Save();
        }

        /// <summary>Açılışta çağrılır: süre bittiyse kapatır, geçerliyse günlük ödülü verir.</summary>
        public void Check()
        {
            var data = SaveManager.Instance.Data;
            if (!data.hasChefPass) return;

            if (!IsActive)
            {
                data.hasChefPass = false;
                SaveManager.Instance.Save();
                return;
            }

            long lastClaim = Convert.ToInt64(PlayerPrefs.GetString(LastClaimKey, "0"));
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            if (now - lastClaim > 20 * 3_600_000L)
            {
                PlayerPrefs.SetString(LastClaimKey, now.ToString());
                EconomyManager.Instance.AddCoins(dailyCoins);
            }
        }
    }
}
