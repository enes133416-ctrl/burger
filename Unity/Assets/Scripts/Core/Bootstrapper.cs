using BurgerRush.Audio;
using BurgerRush.Monetization;
using BurgerRush.Seasonal;
using BurgerRush.Utils;
using UnityEngine;

namespace BurgerRush.Core
{
    /// <summary>
    /// 00_Boot sahnesindeki tek bileşen: sistemleri sırayla başlatır, ana menüye geçer.
    /// </summary>
    public class Bootstrapper : MonoBehaviour
    {
        [SerializeField] private float minimumSplashTime = 1.5f;

        private IEnumerator Start()
        {
            // 1) Kayıt + analitik
            Data.SaveManager.Instance.Load();
            Analytics.AnalyticsManager.Instance.Initialize();

            // 2) Reklam SDK'ları
            AdManager.Instance.Initialize();
            IAPManager.Instance.Initialize();

            // 3) Ses
            AudioManager.Instance.Initialize();

            // 4) Sezonsal etkinlik kontrolü
            SeasonalEventManager.Instance.Refresh();

            // 5) Debug komutları
            DebugConsole.Register("addcoins", args => Economy.EconomyManager.Instance.AddCoins(float.Parse(args[0])));
            DebugConsole.Register("addgems", args => Economy.EconomyManager.Instance.AddGems(int.Parse(args[0])));
            DebugConsole.Register("level", args => SceneLoader.LoadGameplay(int.Parse(args[0])));
            DebugConsole.Register("wipe", args => Data.SaveManager.Instance.Reset());

            yield return new WaitForSeconds(minimumSplashTime);

            GameManager.Instance.ChangeState(GameManager.GameState.MainMenu);
            SceneLoader.Load(Constants.Scenes.MainMenu);
        }
    }
}
