using BurgerRush.Audio;
using BurgerRush.Utils;
using UnityEngine;

namespace BurgerRush.Gameplay.Kitchen
{
    /// <summary>
    /// Açık ızgara: OvenController ile aynı mantıkta çalışır, farklı görsel/animasyona sahiptir.
    /// (GDD 2.6 "IZGARA/FIRIN" tek ağaç altında modellendiği için parametreleri paylaşır.)
    /// </summary>
    public class GrillController : OvenController
    {
        [Header("Grill Specific")]
        [SerializeField] private ParticleSystem flameEffect;
        [SerializeField] private Light grillLight;
        [SerializeField] private float flameIntensity = 1.2f;

        private float baseLightIntensity = 1f;

        protected override void Awake()
        {
            base.Awake();
            if (grillLight) baseLightIntensity = grillLight.intensity;
        }

        protected override void Update()
        {
            base.Update();
            AnimateFlame();
        }

        private void AnimateFlame()
        {
            int cooking = Slots.FindAll(s => s.State == SlotState.Cooking).Count;
            bool active = cooking > 0;

            if (flameEffect)
            {
                if (active && !flameEffect.isPlaying) flameEffect.Play();
                else if (!active && flameEffect.isPlaying) flameEffect.Stop();
            }

            if (grillLight)
                grillLight.intensity = baseLightIntensity * (active ? flameIntensity : 0.4f)
                                        + (active ? Mathf.PingPong(Time.time * 3f, 0.25f) : 0f);
        }

        public override bool PlaceFood(FoodItem food)
        {
            bool placed = base.PlaceFood(food);
            if (placed) AudioManager.Instance?.PlaySFX("GrillPlace");
            return placed;
        }
    }
}
