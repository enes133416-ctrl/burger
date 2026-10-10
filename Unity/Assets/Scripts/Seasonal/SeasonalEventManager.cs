using System;
using BurgerRush.Utils;
using UnityEngine;

namespace BurgerRush.Seasonal
{
    public enum SeasonId { None, Halloween, Christmas, Valentine, CherryBlossom, Summer }

    /// <summary>
    /// Sezonsal etkinlik yönetimi: 14 günlük etkinlikler, görevler ve token ödülleri (GDD 10).
    /// </summary>
    public class SeasonalEventManager : Singleton<SeasonalEventManager>
    {
        [SerializeField] private ThemeManager themeManager;

        public SeasonId Current { get; private set; } = SeasonId.None;
        public bool IsEventActive => Current != SeasonId.None;

        private void Start()
        {
            Refresh();
            EventRewardSystem.Instance?.BeginSeason(Current);
        }

        public void Refresh()
        {
            Current = SeasonFor(DateTime.Now);
            if (themeManager) themeManager.ApplySeason(Current);
            Debug.Log($"[Seasonal] Aktif sezon: {Current}");
        }

        public static SeasonId SeasonFor(DateTime date) => date.Month switch
        {
            10 => SeasonId.Halloween,
            12 => SeasonId.Christmas,
            2 => SeasonId.Valentine,
            4 => SeasonId.CherryBlossom,
            7 => SeasonId.Summer,
            _ => SeasonId.None,
        };

        public string DisplayName => Current switch
        {
            SeasonId.Halloween => "🎃 Halloween Haunted Burger",
            SeasonId.Christmas => "🎄 Christmas Cozy Café",
            SeasonId.Valentine => "💝 Valentine's Sweetheart Diner",
            SeasonId.CherryBlossom => "🌸 Cherry Blossom Fusion",
            SeasonId.Summer => "☀️ Summer Beach Grill",
            _ => "Etkinlik yok",
        };
    }
}
