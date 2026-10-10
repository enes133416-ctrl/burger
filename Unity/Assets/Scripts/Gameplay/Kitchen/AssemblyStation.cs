using System.Collections.Generic;
using BurgerRush.Audio;
using BurgerRush.Input;
using UnityEngine;

namespace BurgerRush.Gameplay.Kitchen
{
    /// <summary>
    /// Montaj tezgahı: katmanları yığar, hazır burgeri servis noktasına sunar (GDD 2.3).
    /// </summary>
    public class AssemblyStation : MonoBehaviour
    {
        [Header("Stacking")]
        [SerializeField] private Transform stackRoot;
        [SerializeField] private float layerHeight = 0.09f;
        [SerializeField] private float placeAnimationTime = 0.8f;

        [Header("Snap")]
        [SerializeField] private float snapRadius = 2f;

        public BurgerBuilder Builder { get; } = new();
        public bool HasCompleteBurger => Builder.IsComplete;

        private readonly List<FoodItem> placed = new();

        public bool CanAccept(FoodItem item)
        {
            if (item == null) return false;
            if (item.IsBurnt) return false;
            if (item.NeedsCooking && item.State != CookState.Cooked) return false;
            return placed.Count < 8;
        }

        public bool AddLayer(FoodItem item)
        {
            if (!CanAccept(item)) return false;

            item.transform.SetParent(stackRoot ? stackRoot : transform);
            var target = (stackRoot ? stackRoot : transform).position + Vector3.up * (placed.Count * layerHeight);
            StartCoroutine(AnimatePlacement(item.transform, target));
            item.transform.rotation = Quaternion.identity;

            placed.Add(item);
            Builder.AddLayer(item);

            AudioManager.Instance?.PlaySFX("Thwack");
            HapticManager.Instance?.TriggerLight();
            return true;
        }

        private System.Collections.IEnumerator AnimatePlacement(Transform t, Vector3 target)
        {
            var start = t.position;
            float timer = 0f;
            while (timer < placeAnimationTime)
            {
                timer += Time.deltaTime;
                t.position = Vector3.Lerp(start, target, timer / placeAnimationTime);
                yield return null;
            }
            t.position = target;
        }

        /// <summary>Hazır burgeri alır ve tezgahı boşaltır.</summary>
        public List<FoodItem> TakeBurger()
        {
            if (!Builder.IsComplete) return null;
            var burger = new List<FoodItem>(placed);
            placed.Clear();
            Builder.Clear();
            return burger;
        }

        public void Discard()
        {
            foreach (var item in placed)
                if (item) Destroy(item.gameObject);
            placed.Clear();
            Builder.Clear();
        }

        /// <summary>Sürükleme sırasında mıknatıs etkisi (GDD 2.3 Snap Zone).</summary>
        public bool IsInSnapZone(Vector3 position)
            => Vector3.Distance(transform.position, position) <= snapRadius;
    }
}
