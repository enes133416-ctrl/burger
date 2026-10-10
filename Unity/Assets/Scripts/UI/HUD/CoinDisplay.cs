using BurgerRush.Core;
using BurgerRush.Data;
using BurgerRush.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace BurgerRush.UI.HUD
{
    /// <summary>Para ve gem sayaçları (GDD 5.1).</summary>
    public class CoinDisplay : MonoBehaviour
    {
        [SerializeField] private Text coinText;
        [SerializeField] private Text gemText;
        [SerializeField] private float lerpSpeed = 8f;

        private float displayedCoins;
        private float displayedGems;

        private void OnEnable() => EventBus.Subscribe<CoinChangedEvent>(OnCoinsChanged);
        private void OnDisable() => EventBus.Unsubscribe<CoinChangedEvent>(OnCoinsChanged);

        private void Start()
        {
            var data = SaveManager.Instance.Data;
            displayedCoins = data.totalCoins;
            displayedGems = data.totalGems;
            RefreshText();
        }

        private void Update()
        {
            var data = SaveManager.Instance.Data;
            displayedCoins = Mathf.Lerp(displayedCoins, data.totalCoins, Time.deltaTime * lerpSpeed);
            displayedGems = Mathf.Lerp(displayedGems, data.totalGems, Time.deltaTime * lerpSpeed);
            RefreshText();
        }

        private void OnCoinsChanged(CoinChangedEvent e) { /* animasyon tetiklenebilir */ }

        private void RefreshText()
        {
            if (coinText) coinText.text = $"🪙 {displayedCoins.ToShortNumber()}";
            if (gemText) gemText.text = $"💎 {(int)displayedGems}";
        }
    }
}
