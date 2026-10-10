using System.Collections;
using BurgerRush.Core;
using BurgerRush.Data;
using BurgerRush.Gameplay.Progression;
using BurgerRush.Monetization;
using BurgerRush.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace BurgerRush.UI.Menus
{
    /// <summary>15_LevelComplete: yıldızlar, kazanç, 2x reklam teklifi (GDD 5.2).</summary>
    public class LevelCompleteUI : MonoBehaviour
    {
        [SerializeField] private GameObject[] starObjects;
        [SerializeField] private Text earnedText;
        [SerializeField] private Text servedText;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button doubleRewardButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private ParticleSystem starBurst;

        private float earned;
        private int stars;

        private void Awake()
        {
            if (nextButton) nextButton.onClick.AddListener(NextLevel);
            if (doubleRewardButton) doubleRewardButton.onClick.AddListener(WatchAdForDouble);
            if (menuButton) menuButton.onClick.AddListener(() => SceneLoader.Load(Constants.Scenes.MainMenu));
        }

        private void OnEnable() => EventBus.Subscribe<LevelCompleteEvent>(OnLevelComplete);
        private void OnDisable() => EventBus.Unsubscribe<LevelCompleteEvent>(OnLevelComplete);

        private void OnLevelComplete(LevelCompleteEvent e)
        {
            stars = e.Stars;
            earned = e.Coins;

            for (int i = 0; i < starObjects.Length; i++)
                if (starObjects[i]) starObjects[i].SetActive(i < stars);

            if (earnedText) earnedText.text = $"🪙 {(int)earned}";
            if (starBurst && stars > 0) starBurst.Play();

            SaveManager.Instance.SetStar(e.Level, stars);
            MilestoneReward.TryGrantFor(e.Level);
            StartCoroutine(UI.UIAnimator.CountTo(earnedText, 0, earned, 0.8f));
        }

        private void NextLevel()
        {
            int next = Mathf.Min(50, SaveManager.Instance.Data.currentLevel + 1);
            SaveManager.Instance.Data.currentLevel = next;
            SaveManager.Instance.Save();
            SceneLoader.LoadGameplay((next - 1) / 10);
        }

        private void WatchAdForDouble()
        {
            AdManager.Instance.ShowRewardedAd(AdManager.RewardType.DoubleIncome, () =>
            {
                Economy.EconomyManager.Instance.AddCoins(earned);
                if (doubleRewardButton) doubleRewardButton.interactable = false;
            });
        }
    }
}
