using BurgerRush.Data;
using UnityEngine;

namespace BurgerRush.Gameplay.Progression
{
    /// <summary>
    /// 3 yıldız sistemi (GDD 3.2):
    /// ⭐ %60 · ⭐⭐ %80 + sıfır hata · ⭐⭐⭐ %95 + sıfır kayıp
    /// </summary>
    public class StarRatingSystem : Utils.Singleton<StarRatingSystem>
    {
        [SerializeField] private int stars = 3;
        public int CurrentStars => stars;

        public void ResetStars() => stars = 3;

        public void DeductStar() => stars = Mathf.Max(0, stars - 1);

        public static int Evaluate(LevelData level, int served, int complaints, int lost)
        {
            if (!level) return 0;
            float ratio = served / (float)Mathf.Max(1, level.totalOrders);

            if (ratio >= 0.95f && complaints == 0 && lost == 0) return 3;
            if (ratio >= 0.80f && complaints == 0) return 2;
            if (ratio >= 0.60f) return 1;
            return 0;
        }
    }
}
