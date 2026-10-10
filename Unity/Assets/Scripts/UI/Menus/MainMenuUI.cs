using BurgerRush.Audio;
using BurgerRush.Core;
using BurgerRush.Data;
using BurgerRush.Monetization;
using BurgerRush.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace BurgerRush.UI.Menus
{
    /// <summary>01_MainMenu arayüzü (GDD 5.2).</summary>
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button shopButton;
        [SerializeField] private Button wardrobeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Text statsText;
        [SerializeField] private GameObject chefPassBadge;

        private void Awake()
        {
            if (playButton) playButton.onClick.AddListener(() => SceneLoader.Load(Constants.Scenes.WorldMap));
            if (shopButton) shopButton.onClick.AddListener(() => SceneLoader.LoadAdditive(Constants.Scenes.Shop));
            if (wardrobeButton) wardrobeButton.onClick.AddListener(() => SceneLoader.LoadAdditive(Constants.Scenes.Wardrobe));
            if (settingsButton) settingsButton.onClick.AddListener(() => SceneLoader.LoadAdditive(Constants.Scenes.Settings));
        }

        private void Start()
        {
            var data = SaveManager.Instance.Data;
            if (statsText)
                statsText.text = $"📍 Bölüm {data.currentLevel}   ⭐ {data.totalStars}   🍔 {data.stats.served}";

            if (chefPassBadge)
                chefPassBadge.SetActive(ChefPassManager.Instance && ChefPassManager.Instance.IsActive);

            AudioManager.Instance?.PlayMusic(MusicTrack.MainMenu);
            GameManager.Instance.ChangeState(GameManager.GameState.MainMenu);
        }
    }
}
