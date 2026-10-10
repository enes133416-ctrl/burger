using System.Collections;
using BurgerRush.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BurgerRush.Core
{
    /// <summary>
    /// Sahne yükleme: tek sahne, additive sahne ve yükleme ekranı geçişleri.
    /// </summary>
    public class SceneLoader : Singleton<SceneLoader>
    {
        [SerializeField] private GameObject loadingScreen;
        [SerializeField] private UnityEngine.UI.Slider progressBar;

        public static void Load(string sceneName)
        {
            if (Instance == null) { SceneManager.LoadScene(sceneName); return; }
            Instance.StartCoroutine(Instance.LoadAsync(sceneName, LoadSceneMode.Single));
        }

        public static void LoadAdditive(string sceneName)
        {
            if (Instance == null) { SceneManager.LoadScene(sceneName, LoadSceneMode.Additive); return; }
            Instance.StartCoroutine(Instance.LoadAsync(sceneName, LoadSceneMode.Additive));
        }

        public static void Unload(string sceneName)
            => SceneManager.UnloadSceneAsync(sceneName);

        private IEnumerator LoadAsync(string sceneName, LoadSceneMode mode)
        {
            if (loadingScreen) loadingScreen.SetActive(true);

            var op = SceneManager.LoadSceneAsync(sceneName, mode);
            op.allowSceneActivation = false;

            while (op.progress < 0.9f)
            {
                if (progressBar) progressBar.value = op.progress;
                yield return null;
            }

            if (progressBar) progressBar.value = 1f;
            yield return new WaitForSeconds(0.15f);
            op.allowSceneActivation = true;
            yield return op;

            if (loadingScreen) loadingScreen.SetActive(false);
        }

        /// <summary>Lokasyon index'ine göre gameplay sahnesini açar.</summary>
        public static void LoadGameplay(int locationIndex)
            => Load(Constants.Scenes.GameplayFor(locationIndex));
    }
}
