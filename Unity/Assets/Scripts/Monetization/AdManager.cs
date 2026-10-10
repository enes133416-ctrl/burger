using System;
using BurgerRush.Core;
using BurgerRush.Utils;
using UnityEngine;

namespace BurgerRush.Monetization
{
    /// <summary>
    /// Reklam yönetimi: interstitial (her 4 levelda 1) + rewarded (GDD 4.2).
    /// SDK entegrasyonu (MAX / IronSource / AdMob) için işaretli bölgeler hazırdır.
    /// </summary>
    public class AdManager : Singleton<AdManager>
    {
        [Header("Settings")]
        [SerializeField] private int interstitialFrequency = Constants.Ads.InterstitialFrequency;

        public enum RewardType { DoubleIncome, SpeedBoost, DoubleOfflineEarnings, ReviveLevel, ExtraIngredients }
        public enum AdState { Idle, Loading, Ready, Showing }

        private int levelsSinceLastAd;
        private bool isInitialized;
        private AdState rewardedState = AdState.Idle;

        public bool IsRewardedReady => isInitialized && rewardedState == AdState.Ready;
        public int LevelsUntilNextAd => Mathf.Max(0, interstitialFrequency - levelsSinceLastAd);

        public void Initialize()
        {
            if (isInitialized) return;
            // ── SDK init ──
            // IronSource.Agent.init(APP_KEY);
            // MaxSdk.InitializeSdk();
            isInitialized = true;
            LoadRewardedAd();
            LoadInterstitialAd();
            Debug.Log("[AdManager] Initialized");
        }

        // ─────────── Interstitial ───────────
        public bool TryShowInterstitial()
        {
            levelsSinceLastAd++;
            if (levelsSinceLastAd < interstitialFrequency) return false;
            if (!isInitialized) return false;

            levelsSinceLastAd = 0;
            ShowInterstitial();
            return true;
        }

        private void ShowInterstitial()
        {
            Debug.Log("[AdManager] Showing Interstitial");
            // IronSource.Agent.showInterstitial();
            Analytics.AnalyticsManager.Instance.TrackAdShown("interstitial");
        }

        // ─────────── Rewarded ───────────
        public void ShowRewardedAd(RewardType rewardType, Action onSuccess = null, Action onFail = null)
        {
            if (!IsRewardedReady)
            {
                Debug.LogWarning("[AdManager] Rewarded ad hazır değil");
                onFail?.Invoke();
                return;
            }

            rewardedState = AdState.Showing;
            Debug.Log($"[AdManager] Showing Rewarded Ad: {rewardType}");

            // SDK callback'inde: OnRewardedAdRewardedEvent → ApplyReward(rewardType)
            ApplyReward(rewardType);
            onSuccess?.Invoke();

            Analytics.AnalyticsManager.Instance.TrackAdWatched(rewardType.ToString());
            rewardedState = AdState.Idle;
            LoadRewardedAd();
        }

        private void ApplyReward(RewardType type)
        {
            switch (type)
            {
                case RewardType.DoubleIncome:
                    Economy.EconomyManager.Instance.ActivateDoubleIncome(Constants.Ads.DoubleIncomeSeconds);
                    break;
                case RewardType.SpeedBoost:
                    Gameplay.Kitchen.KitchenManager.Instance?.ActivateSpeedBoost(30f);
                    break;
                case RewardType.DoubleOfflineEarnings:
                    Economy.OfflineEarningsCalculator.Instance.RecalculateWithAdBonus();
                    break;
                case RewardType.ReviveLevel:
                    LevelManager.Instance?.ReviveCurrentLevel();
                    break;
                case RewardType.ExtraIngredients:
                    Gameplay.Kitchen.KitchenManager.Instance?.ActivateSpeedBoost(15f);
                    break;
            }

            EventBus.Publish(new AdRewardedEvent { RewardType = type.ToString() });
        }

        private void LoadRewardedAd()
        {
            // IronSource.Agent.loadRewardedVideo();
            rewardedState = AdState.Ready;
        }

        private void LoadInterstitialAd()
        {
            // IronSource.Agent.loadInterstitial();
        }
    }
}
