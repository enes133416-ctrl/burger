using System.Collections.Generic;
using BurgerRush.Core;
using BurgerRush.Economy;
using UnityEngine;

namespace BurgerRush.Gameplay.Kitchen
{
    /// <summary>
    /// Mutfak sistemlerinin merkezi: fırın, ızgara, montaj tezgahı ve malzeme havuzu (GDD 6.1).
    /// </summary>
    public class KitchenManager : Utils.Singleton<KitchenManager>
    {
        [Header("Stations")]
        [SerializeField] private OvenController oven;
        [SerializeField] private GrillController grill;
        [SerializeField] private AssemblyStation assembly;
        [SerializeField] private Transform[] ingredientSpawners;

        [Header("Prefabs")]
        [SerializeField] private GameObject foodItemPrefab;

        public OvenController Oven => oven;
        public AssemblyStation Assembly => assembly;

        private readonly List<FoodItem> activeItems = new();
        private float speedBoostTimer;

        public bool IsSpeedBoostActive => speedBoostTimer > 0f;
        public float CookSpeedMultiplier => IsSpeedBoostActive ? 0.5f : 1f;

        private void Start()
        {
            ApplyEquipmentUpgrades();
            EventBus.Subscribe<UpgradePurchasedEvent>(OnUpgradePurchased);
        }

        private void Update()
        {
            if (speedBoostTimer > 0f) speedBoostTimer -= Time.deltaTime;
        }

        private void OnDestroy() => EventBus.Unsubscribe<UpgradePurchasedEvent>(OnUpgradePurchased);

        private void OnUpgradePurchased(UpgradePurchasedEvent e)
        {
            if (e.UpgradeId == "grill" || e.UpgradeId == "oven") ApplyEquipmentUpgrades();
        }

        public void ApplyEquipmentUpgrades()
        {
            int grillLevel = UpgradeSystem.Instance.GetEquipmentLevel("grill");
            oven?.ApplyUpgrade(grillLevel);
            grill?.ApplyUpgrade(grillLevel);
        }

        /// <summary>Yeni malzeme örneği oluşturur (object pool entegrasyonu için hazır).</summary>
        public FoodItem SpawnIngredient(IngredientType type, Vector3 position)
        {
            var go = Utils.ObjectPooler.Instance
                ? Utils.ObjectPooler.Instance.GetFromPool(Constants.Pool.Food, position, Quaternion.identity)
                : Instantiate(foodItemPrefab, position, Quaternion.identity);

            if (!go) return null;
            var item = go.GetComponent<FoodItem>();
            item?.Initialize(type, IngredientValue(type));
            if (item) activeItems.Add(item);
            return item;
        }

        public void DespawnIngredient(FoodItem item)
        {
            if (!item) return;
            activeItems.Remove(item);
            if (Utils.ObjectPooler.Instance) Utils.ObjectPooler.Instance.ReturnToPool(Constants.Pool.Food, item.gameObject);
            else Destroy(item.gameObject);
        }

        /// <summary>Ödüllü reklam ödülü: mutfak hız bonusu (GDD 4.2).</summary>
        public void ActivateSpeedBoost(float seconds) => speedBoostTimer = Mathf.Max(speedBoostTimer, seconds);

        public static float IngredientValue(IngredientType type) => type switch
        {
            IngredientType.Bun => 1f,
            IngredientType.Patty => 4f,
            IngredientType.Cheese => 2f,
            IngredientType.Lettuce => 1f,
            IngredientType.Tomato => 2f,
            IngredientType.Sauce => 1f,
            _ => 1f,
        };
    }
}
