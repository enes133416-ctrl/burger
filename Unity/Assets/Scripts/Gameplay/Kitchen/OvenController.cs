using System.Collections.Generic;
using BurgerRush.Audio;
using BurgerRush.Core;
using BurgerRush.Economy;
using BurgerRush.Input;
using BurgerRush.Utils;
using UnityEngine;

namespace BurgerRush.Gameplay.Kitchen
{
    /// <summary>
    /// Fırın timer yönetimi: %75 uyarı, %100 hazır, %115 yanma (GDD 2.4).
    /// </summary>
    public class OvenController : MonoBehaviour
    {
        [Header("Oven Settings")]
        [SerializeField] private int maxSlots = 2;
        [SerializeField] private float cookingDuration = 30f;
        [SerializeField] private float burnThreshold = 1.15f;

        [Header("Visual")]
        [SerializeField] private ParticleSystem smokeEffect;
        [SerializeField] private Material[] foodMaterials;      // Raw, Cooking, Done, Burnt
        [SerializeField] private Transform[] slotAnchors;

        public enum SlotState { Empty, Cooking, Ready, Burnt }

        [System.Serializable]
        public class OvenSlot
        {
            public bool IsOccupied;
            public FoodItem Food;
            public float CookProgress;
            public SlotState State;
            public GameObject SlotObject;
            public Transform Anchor;
        }

        public List<OvenSlot> Slots { get; private set; } = new();

        public bool HasFreeSlot => Slots.Exists(s => !s.IsOccupied);
        public int ReadyCount => Slots.FindAll(s => s.State == SlotState.Ready).Count;
        public int BurntCount => Slots.FindAll(s => s.State == SlotState.Burnt).Count;

        protected virtual void Awake() => InitializeSlots();

        private void InitializeSlots()
        {
            Slots.Clear();
            for (int i = 0; i < maxSlots; i++)
            {
                var anchor = slotAnchors != null && i < slotAnchors.Length ? slotAnchors[i] : transform;
                Slots.Add(new OvenSlot { State = SlotState.Empty, Anchor = anchor });
            }
        }

        protected virtual void Update()
        {
            foreach (var slot in Slots)
                if (slot.State == SlotState.Cooking) UpdateCooking(slot);
        }

        private void UpdateCooking(OvenSlot slot)
        {
            slot.CookProgress += Time.deltaTime / cookingDuration;
            UpdateFoodVisual(slot);

            if (slot.CookProgress >= 1f && slot.State != SlotState.Ready)
            {
                slot.State = SlotState.Ready;
                AudioManager.Instance?.PlaySFX("OvenDone");
                HapticManager.Instance?.TriggerLight();
                SetSlotMaterial(slot, 2);       // yeşil glow
            }
            else if (slot.CookProgress >= 0.75f && slot.CookProgress < 1f)
            {
                AudioManager.Instance?.PlaySFX("AlmostReady");
            }

            if (slot.CookProgress >= burnThreshold) BurnFood(slot);
        }

        private void UpdateFoodVisual(OvenSlot slot)
        {
            if (slot.CookProgress < 0.5f) SetSlotMaterial(slot, 0);
            else if (slot.CookProgress < 1f) SetSlotMaterial(slot, 1);
        }

        private void SetSlotMaterial(OvenSlot slot, int index)
        {
            if (!slot.SlotObject || foodMaterials == null || index >= foodMaterials.Length) return;
            var rend = slot.SlotObject.GetComponent<Renderer>();
            if (rend && foodMaterials[index]) rend.material = foodMaterials[index];
        }

        private void BurnFood(OvenSlot slot)
        {
            slot.State = SlotState.Burnt;
            SetSlotMaterial(slot, 3);
            smokeEffect?.Play();
            AudioManager.Instance?.PlaySFX("Burn");

            float lost = slot.Food ? slot.Food.BaseValue : 4f;
            EconomyManager.Instance?.DeductCoins(lost);
            EventBus.Publish(new FoodBurntEvent { FoodName = slot.Food ? slot.Food.Type.ToString() : "Patty", LostValue = lost });
        }

        public virtual bool PlaceFood(FoodItem food)
        {
            if (food == null) return false;
            var slot = Slots.Find(s => !s.IsOccupied);
            if (slot == null) return false;

            slot.IsOccupied = true;
            slot.Food = food;
            slot.CookProgress = 0f;
            slot.State = SlotState.Cooking;

            food.SetState(CookState.Cooking);
            food.transform.SetParent(slot.Anchor);
            food.transform.localPosition = Vector3.zero;
            slot.SlotObject = food.gameObject;
            AudioManager.Instance?.PlaySFX("Sizzle");
            return true;
        }

        public FoodItem CollectFood(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= Slots.Count) return null;
            var slot = Slots[slotIndex];
            if (slot.State != SlotState.Ready && slot.State != SlotState.Burnt) return null;

            var food = slot.Food;
            food?.SetState(slot.State == SlotState.Burnt ? CookState.Burnt : CookState.Cooked);
            ClearSlot(slot);
            return food;
        }

        public void ClearSlot(OvenSlot slot)
        {
            slot.IsOccupied = false;
            slot.Food = null;
            slot.CookProgress = 0f;
            slot.State = SlotState.Empty;
            slot.SlotObject = null;
        }

        /// <summary>Ekipman seviyesine göre fırın parametreleri (GDD 2.4 tablosu).</summary>
        public void ApplyUpgrade(int level)
        {
            switch (level)
            {
                case 2: maxSlots = 3; cookingDuration = 24f; break;
                case 3: maxSlots = 4; cookingDuration = 18f; break;
                case 4: maxSlots = 4; cookingDuration = 14f; burnThreshold = 1.25f; break;
                case 5: maxSlots = 6; cookingDuration = 10f; burnThreshold = 1.5f; break;
                default: maxSlots = 2; cookingDuration = 30f; burnThreshold = 1.15f; break;
            }
            InitializeSlots();
        }

        /// <summary>Ödüllü reklam: tüm slotları anında hazırla (GDD 4.2).</summary>
        public int InstantFinishAll()
        {
            int count = 0;
            foreach (var slot in Slots)
            {
                if (slot.State != SlotState.Cooking) continue;
                slot.CookProgress = 1f;
                slot.State = SlotState.Ready;
                slot.Food?.SetState(CookState.Cooked);
                SetSlotMaterial(slot, 2);
                count++;
            }
            if (count > 0) AudioManager.Instance?.PlaySFX("OvenDone");
            return count;
        }
    }
}
