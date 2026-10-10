using System;
using UnityEngine;
using UnityEngine.UI;

namespace BurgerRush.Monetization
{
    /// <summary>
    /// Ödüllü reklam butonları için hazır bileşen: tıklanınca reklamı açar, ödülü uygular.
    /// </summary>
    public class RewardedAdHandler : MonoBehaviour
    {
        [SerializeField] private AdManager.RewardType rewardType = AdManager.RewardType.DoubleIncome;
        [SerializeField] private Button button;
        [SerializeField] private GameObject loadingIndicator;

        public event Action OnRewardGranted;

        private void Awake()
        {
            if (button) button.onClick.AddListener(RequestAd);
        }

        public void RequestAd()
        {
            if (loadingIndicator) loadingIndicator.SetActive(true);
            AdManager.Instance.ShowRewardedAd(
                rewardType,
                () => { if (loadingIndicator) loadingIndicator.SetActive(false); OnRewardGranted?.Invoke(); },
                () => { if (loadingIndicator) loadingIndicator.SetActive(false); });
        }

        private void Update()
        {
            if (button) button.interactable = AdManager.Instance && AdManager.Instance.IsRewardedReady;
        }
    }
}
