using BurgerRush.Economy;
using BurgerRush.Monetization;
using UnityEngine;
using UnityEngine.UI;

namespace BurgerRush.UI.Panels
{
    /// <summary>Offline gelir penceresi: 8 saatlik kazanç + 2x reklam (GDD 2.7).</summary>
    public class OfflineEarningsUI : MonoBehaviour
    {
        [SerializeField] private Text descriptionText;
        [SerializeField] private Text amountText;
        [SerializeField] private Button claimButton;
        [SerializeField] private Button claimAdButton;
        [SerializeField] private GameObject root;

        private void Awake()
        {
            if (claimButton) claimButton.onClick.AddListener(() => Claim(false));
            if (claimAdButton) claimAdButton.onClick.AddListener(() => ClaimWithAd());
        }

        public void Show()
        {
            var result = OfflineEarningsCalculator.Instance.Calculate();
            if (result.Earnings <= 0f) { Hide(); return; }

            if (root) root.SetActive(true);
            if (descriptionText)
                descriptionText.text = $"{result.HoursElapsed:0.0} saat kapalıydın · verim %{Mathf.RoundToInt(result.Efficiency * 100)}";
            if (amountText) amountText.text = $"🪙 {(int)result.Earnings}";
            if (claimAdButton) claimAdButton.gameObject.SetActive(result.CanDoubleWithAd);
        }

        public void Hide() { if (root) root.SetActive(false); }

        private void Claim(bool doubled)
        {
            OfflineEarningsCalculator.Instance.Claim(doubled);
            Hide();
        }

        private void ClaimWithAd()
        {
            AdManager.Instance.ShowRewardedAd(AdManager.RewardType.DoubleOfflineEarnings, () => Claim(true));
        }
    }
}
