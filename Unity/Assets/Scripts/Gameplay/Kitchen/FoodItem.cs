using System;
using UnityEngine;

namespace BurgerRush.Gameplay.Kitchen
{
    public enum IngredientType { Bun, Patty, Cheese, Lettuce, Tomato, Sauce }
    public enum CookState { Raw, Cooking, Cooked, Burnt }

    /// <summary>
    /// Bir malzeme örneği: tipi, pişme durumu ve değeri (GDD 8.2 FoodItem).
    /// </summary>
    public class FoodItem : MonoBehaviour
    {
        [SerializeField] private IngredientType type = IngredientType.Patty;
        [SerializeField] private CookState state = CookState.Raw;
        [SerializeField] private float baseValue = 4f;

        [Header("Visuals")]
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private Material rawMaterial;
        [SerializeField] private Material cookedMaterial;
        [SerializeField] private Material burntMaterial;

        public IngredientType Type { get => type; set => type = value; }
        public CookState State => state;
        public float BaseValue => baseValue;
        public float Value => state == CookState.Burnt ? 0f : baseValue;
        public bool NeedsCooking => type == IngredientType.Patty;
        public bool IsReadyToUse => !NeedsCooking || state == CookState.Cooked;
        public bool IsBurnt => state == CookState.Burnt;

        public void SetState(CookState newState)
        {
            state = newState;
            ApplyMaterial();
        }

        private void ApplyMaterial()
        {
            if (!meshRenderer) return;
            var mat = state switch
            {
                CookState.Cooked => cookedMaterial ? cookedMaterial : rawMaterial,
                CookState.Burnt => burntMaterial ? burntMaterial : rawMaterial,
                _ => rawMaterial,
            };
            if (mat) meshRenderer.material = mat;
        }

        public void Initialize(IngredientType t, float value = 4f)
        {
            type = t;
            baseValue = value;
            state = CookState.Raw;
            ApplyMaterial();
        }

        private void OnValidate() => ApplyMaterial();
    }
}
