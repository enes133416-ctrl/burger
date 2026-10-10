using BurgerRush.Gameplay.Kitchen;
using UnityEngine;

namespace BurgerRush.Gameplay.Staff
{
    /// <summary>Aşçı: köfteleri otomatik pişirir, Lv4+ pişeni tepsiye alır (GDD 2.6).</summary>
    public class Cook : StaffBase
    {
        [SerializeField] private float[] autoCookDelay = { 6.0f, 4.5f, 3.2f, 2.4f, 1.6f };
        [SerializeField] private KitchenManager kitchen;

        private float timer;

        public float AutoCookDelay => autoCookDelay[Mathf.Clamp(level - 1, 0, autoCookDelay.Length - 1)];
        public int PattiesPerTick => level >= 5 ? 6 : level >= 3 ? 2 : 1;
        public bool CanCollectToTray => level >= 4;

        protected override void DoWork(float deltaTime)
        {
            if (!kitchen || !kitchen.Oven) return;

            timer += deltaTime;
            if (timer < AutoCookDelay) return;
            timer = 0f;

            for (int i = 0; i < PattiesPerTick; i++)
            {
                var patty = kitchen.SpawnIngredient(IngredientType.Patty, kitchen.Oven.transform.position);
                if (patty == null || !kitchen.Oven.PlaceFood(patty)) { kitchen.DespawnIngredient(patty); break; }
            }
        }
    }
}
