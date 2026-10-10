using BurgerRush.Core;
using UnityEngine;
using UnityEngine.UI;

namespace BurgerRush.UI.Menus
{
    /// <summary>Pause menüsü: devam / yeniden başlat / ana menü (GDD 5.2).</summary>
    public class PauseMenuUI : MonoBehaviour
    {
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private GameObject panel;

        private void Awake()
        {
            if (resumeButton) resumeButton.onClick.AddListener(Resume);
            if (restartButton) restartButton.onClick.AddListener(Restart);
            if (mainMenuButton) mainMenuButton.onClick.AddListener(Quit);
            SetVisible(false);
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape)) Toggle();
        }

        public void Toggle() => SetVisible(!(panel && panel.activeSelf));

        private void SetVisible(bool visible)
        {
            if (panel) panel.SetActive(visible);
            if (GameManager.Instance)
                GameManager.Instance.ChangeState(visible ? GameManager.GameState.Paused : GameManager.GameState.Gameplay);
        }

        private void Resume() => SetVisible(false);

        private void Restart()
        {
            SetVisible(false);
            LevelManager.Instance?.Restart();
        }

        private void Quit()
        {
            SetVisible(false);
            if (LevelManager.Instance) LevelManager.Instance.Fail("quit");
            SceneLoader.Load(Constants.Scenes.MainMenu);
        }
    }
}
