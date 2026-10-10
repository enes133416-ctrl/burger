using System;
using BurgerRush.Audio;
using BurgerRush.Data;
using BurgerRush.Economy;
using BurgerRush.Monetization;
using UnityEngine;

namespace BurgerRush.Core
{
    /// <summary>
    /// Oyun durum makinesi ve sistem başlatıcı (GDD 8.2).
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        public enum GameState
        {
            Boot, MainMenu, WorldMap, Gameplay,
            Paused, LevelComplete, LevelFail, Shop
        }

        public GameState CurrentState { get; private set; } = GameState.Boot;
        public static event Action<GameState> OnStateChanged;

        [Header("Managers")]
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private EconomyManager economyManager;
        [SerializeField] private AdManager adManager;
        [SerializeField] private AudioManager audioManager;

        public LevelManager Levels => levelManager ? levelManager : LevelManager.Instance;
        public EconomyManager Economy => economyManager ? economyManager : EconomyManager.Instance;
        public AdManager Ads => adManager ? adManager : AdManager.Instance;
        public AudioManager Audio => audioManager ? audioManager : AudioManager.Instance;

        public int CurrentLevel => SaveManager.Instance.Data.currentLevel;

        protected override void Awake()
        {
            base.Awake();
            InitializeSystems();
        }

        private void InitializeSystems()
        {
            SaveManager.Instance.Load();
            Analytics.AnalyticsManager.Instance.Initialize();
            Ads.Initialize();
            OfflineEarningsCalculator.Instance.Calculate();
        }

        public void ChangeState(GameState newState)
        {
            CurrentState = newState;
            OnStateChanged?.Invoke(newState);
            EventBus.Publish(new StateChangedEvent { State = newState });
            HandleStateTransition(newState);
        }

        private void HandleStateTransition(GameState state)
        {
            switch (state)
            {
                case GameState.Gameplay:
                    Time.timeScale = 1f;
                    Audio.PlayMusic(MusicTrack.Gameplay);
                    break;
                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;
                case GameState.LevelComplete:
                    Time.timeScale = 0f;
                    Analytics.AnalyticsManager.Instance.TrackLevelComplete(CurrentLevel, 0);
                    Ads.TryShowInterstitial();
                    break;
                case GameState.LevelFail:
                    Time.timeScale = 0f;
                    Analytics.AnalyticsManager.Instance.TrackLevelFail(CurrentLevel, "time");
                    break;
                case GameState.MainMenu:
                    Time.timeScale = 1f;
                    Audio.PlayMusic(MusicTrack.MainMenu);
                    break;
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus) SaveManager.Instance.StampExit();
        }

        private void OnApplicationQuit()
            => SaveManager.Instance.StampExit();
    }
}
