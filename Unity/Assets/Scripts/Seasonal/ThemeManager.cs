using System.Collections.Generic;
using BurgerRush.Data;
using UnityEngine;

namespace BurgerRush.Seasonal
{
    /// <summary>
    /// Restoran temalarını sahneye uygular: zemin/duvar/vurgu renkleri ve gökyüzü (GDD 4.3 / 10).
    /// </summary>
    public class ThemeManager : MonoBehaviour
    {
        [SerializeField] private CosmeticData cosmeticData;
        [SerializeField] private Renderer floorRenderer;
        [SerializeField] private Renderer wallRenderer;
        [SerializeField] private Light accentLight;
        [SerializeField] private Material defaultSkybox;

        private readonly Dictionary<SeasonId, Color> seasonAccents = new()
        {
            { SeasonId.Halloween, new Color(0.90f, 0.49f, 0.13f) },
            { SeasonId.Christmas, new Color(0.18f, 0.80f, 0.44f) },
            { SeasonId.Valentine, new Color(1.00f, 0.36f, 0.54f) },
            { SeasonId.CherryBlossom, new Color(1.00f, 0.62f, 0.77f) },
            { SeasonId.Summer, new Color(0.50f, 0.85f, 0.91f) },
        };

        public void ApplyTheme(string themeId)
        {
            var theme = System.Array.Find(cosmeticData ? cosmeticData.themes : null, t => t && t.id == themeId);
            if (theme == null) return;

            if (floorRenderer) floorRenderer.material.color = theme.floorColor;
            if (wallRenderer) wallRenderer.material.color = theme.wallColor;
            if (accentLight) accentLight.color = theme.accentColor;
            if (theme.skybox) RenderSettings.skybox = theme.skybox;
        }

        public void ApplySeason(SeasonId season)
        {
            if (season == SeasonId.None)
            {
                if (defaultSkybox) RenderSettings.skybox = defaultSkybox;
                ApplyTheme(SaveManager.Instance.Data.equippedTheme);
                return;
            }

            if (accentLight && seasonAccents.TryGetValue(season, out var color))
                accentLight.color = color;
        }
    }
}
