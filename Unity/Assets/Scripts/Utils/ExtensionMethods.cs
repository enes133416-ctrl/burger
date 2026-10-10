using System;
using System.Collections.Generic;
using UnityEngine;

namespace BurgerRush.Utils
{
    /// <summary>Günlük işleri kolaylaştıran genişletme metodları.</summary>
    public static class ExtensionMethods
    {
        public static string ToShortNumber(this float value)
        {
            if (value >= 1_000_000f) return $"{value / 1_000_000f:0.##}M";
            if (value >= 10_000f) return $"{value / 1000f:0.#}K";
            return Mathf.RoundToInt(value).ToString();
        }

        public static T Random<T>(this IList<T> list)
            => list.Count == 0 ? default : list[UnityEngine.Random.Range(0, list.Count)];

        public static void Shuffle<T>(this IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        public static bool Approximately(this float a, float b, float tolerance = 0.01f)
            => Mathf.Abs(a - b) <= tolerance;

        /// <summary>Child'lar dahil tüm MeshRenderer materyallerinin rengini değiştirir.</summary>
        public static void SetColorRecursive(this GameObject go, Color color)
        {
            foreach (var r in go.GetComponentsInChildren<MeshRenderer>())
                r.material.color = color;
        }
    }
}
