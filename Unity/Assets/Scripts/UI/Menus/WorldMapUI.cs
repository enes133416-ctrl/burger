using System.Collections.Generic;
using BurgerRush.Data;
using BurgerRush.Gameplay.Progression;
using BurgerRush.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace BurgerRush.UI.Menus
{
    /// <summary>02_WorldMap: lokasyon kartları ve kilit sistemi (GDD 3.3).</summary>
    public class WorldMapUI : MonoBehaviour
    {
        [System.Serializable]
        public class LocationCard
        {
            public LocationId Id;
            public Button Button;
            public GameObject LockIcon;
            public Text StarsText;
            public int FromLevel;
            public int ToLevel;
        }

        [SerializeField] private List<LocationCard> cards = new();
        [SerializeField] private LevelProgressionData progression;
        [SerializeField] private Text coinText;

        private void Start()
        {
            var data = SaveManager.Instance.Data;
            if (coinText) coinText.text = $"🪙 {(int)data.totalCoins}";

            foreach (var card in cards)
            {
                bool unlocked = data.maxLevelReached >= card.FromLevel;
                if (card.Button) card.Button.interactable = unlocked;
                if (card.LockIcon) card.LockIcon.SetActive(!unlocked);

                int stars = 0;
                for (int i = card.FromLevel; i <= card.ToLevel; i++)
                    stars += data.levelStars.TryGetValue(i, out var s) ? s : 0;

                if (card.StarsText) card.StarsText.text = $"⭐ {stars}/{(card.ToLevel - card.FromLevel + 1) * 3}";
            }
        }

        public void SelectLocation(int locationIndex)
        {
            var range = progression ? progression.RangeFor(SaveManager.Instance.Data.currentLevel) : null;
            Core.SceneLoader.LoadGameplay(locationIndex);
        }
    }
}
