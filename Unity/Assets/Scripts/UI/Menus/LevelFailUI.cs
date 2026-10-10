using BurgerRush.Core;
using BurgerRush.Monetization;
using BurgerRush.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace BurgerRush.UI.Menus
{
    /// <summary>16_LevelFail: reklamla devam, gem ile atla, menü (GDD 5.2).</summary>
    public class LevelFailUI : MonoBehaviour
    {
        [SerializeField] private Text summaryText;
        [SerializeField] private Button reviveAdButton;
        [SerializeField] private Button reviveGemButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private int reviveGemCost = 50;

        private void Awake()
        {
            if (reviveAdButton) reviveAdButton.onClick.AddListener(ReviveWithAd);
            if (reviveGemButton) reviveGemButton.onClick.AddListener(ReviveWithGems);
            if (menuButton) menuButton.onClick.AddListener(() => SceneLoader.Load(Constants.Scenes.MainMenu));
        }

        private void OnEnable() => EventBus.Subscribe<LevelFailEvent>(OnFail);
        private void OnDisable() => EventBus.Unsubscribe<LevelFailEvent>(OnFail);

        private void OnFail(LevelFailEvent e)
        {
            if (!summaryText) return;
            var mgr = LevelManager.Instance;
            summaryText.text = mgr
                ? $"Servis: {mgr.ServedCount}\nHedefe ulaşılamadı ({e.Reason})"
                : "Bölüm başarısız";
        }

        private void ReviveWithAd()
            => AdManager.Instance.ShowRewardedAd(AdManager.RewardType.ReviveLevel, () => gameObject.SetActive(false));

        private void ReviveWithGems()
        {
            if (!Economy.EconomyManager.Instance.SpendGems(reviveGemCost)) return;
            LevelManager.Instance?.ReviveCurrentLevel();
            gameObject.SetActive(false);
        }
    }
}
