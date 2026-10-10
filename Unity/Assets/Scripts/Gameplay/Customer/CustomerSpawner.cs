using BurgerRush.Utils;
using UnityEngine;

namespace BurgerRush.Gameplay.Customer
{
    /// <summary>
    /// Müşteri üretimi: bölüm verisine göre tip seçer, kanala yönlendirir (GDD 2.2).
    /// </summary>
    public class CustomerSpawner : MonoBehaviour
    {
        [SerializeField] private CustomerAI customerPrefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Channel.ChannelManager channelManager;

        private Data.LevelData level;
        private int spawnedCount;
        private float timer;

        public int SpawnedCount => spawnedCount;
        public bool Finished => level && spawnedCount >= level.totalOrders;

        public void Begin(Data.LevelData levelData)
        {
            level = levelData;
            spawnedCount = 0;
            timer = 0.5f;
        }

        private void Update()
        {
            if (!level || Finished) return;
            timer -= Time.deltaTime;
            if (timer > 0f) return;

            timer = level.spawnInterval;
            SpawnOne();
        }

        public CustomerAI SpawnOne()
        {
            if (!level || !channelManager) return null;

            var channel = channelManager.GetChannelWithRoom();
            if (channel == null) return null;

            var type = level.customerTypes.Length > 0
                ? level.customerTypes[Random.Range(0, level.customerTypes.Length)]
                : CustomerType.Casual;

            var go = ObjectPooler.Instance
                ? ObjectPooler.Instance.GetFromPool(Constants.Pool.Customer, spawnPoint.position, spawnPoint.rotation)
                : Instantiate(customerPrefab.gameObject, spawnPoint.position, spawnPoint.rotation);

            var customer = go.GetComponent<CustomerAI>();
            if (!customer) { Destroy(go); return null; }

            customer.Initialize(type, level);
            customer.ChannelId = channel.ChannelId;
            channel.Enqueue(customer);
            spawnedCount++;

            Analytics.AnalyticsManager.Instance?.Track("customer_spawn", null);
            CustomerManager.Instance?.Register(customer);
            return customer;
        }
    }
}
