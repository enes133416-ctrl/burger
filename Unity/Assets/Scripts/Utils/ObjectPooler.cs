using System.Collections.Generic;
using UnityEngine;

namespace BurgerRush.Utils
{
    /// <summary>
    /// Nesne havuzu: müşteri, yemek ve efektler için (GDD 8.2).
    /// </summary>
    public class ObjectPooler : Singleton<ObjectPooler>
    {
        [System.Serializable]
        public class Pool
        {
            public string tag;
            public GameObject prefab;
            public int size = 10;
            public bool expandable = true;
        }

        [SerializeField] private List<Pool> pools = new();

        private readonly Dictionary<string, Queue<GameObject>> poolDictionary = new();
        private readonly Dictionary<string, Pool> poolDefinitions = new();

        protected override void Awake()
        {
            base.Awake();
            InitializePools();
        }

        private void InitializePools()
        {
            poolDictionary.Clear();
            poolDefinitions.Clear();

            foreach (var pool in pools)
            {
                var queue = new Queue<GameObject>();
                for (int i = 0; i < pool.size; i++)
                    queue.Enqueue(CreateInstance(pool));

                poolDictionary[pool.tag] = queue;
                poolDefinitions[pool.tag] = pool;
            }
        }

        private GameObject CreateInstance(Pool pool)
        {
            var obj = Instantiate(pool.prefab, transform);
            obj.SetActive(false);
            return obj;
        }

        public GameObject GetFromPool(string tag, Vector3 position, Quaternion rotation)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"[ObjectPooler] Pool '{tag}' bulunamadı!");
                return null;
            }

            var queue = poolDictionary[tag];
            if (queue.Count == 0)
            {
                var def = poolDefinitions[tag];
                if (!def.expandable) return null;
                queue.Enqueue(CreateInstance(def));
            }

            var obj = queue.Dequeue();
            obj.transform.SetPositionAndRotation(position, rotation);
            obj.SetActive(true);
            return obj;
        }

        public void ReturnToPool(string tag, GameObject obj)
        {
            if (!obj) return;
            obj.SetActive(false);
            obj.transform.SetParent(transform);
            if (poolDictionary.TryGetValue(tag, out var queue)) queue.Enqueue(obj);
            else Destroy(obj);
        }

        public void ReturnAll()
        {
            foreach (var kv in poolDictionary)
                while (kv.Value.Count > 0)
                {
                    var obj = kv.Value.Dequeue();
                    if (obj) obj.SetActive(false);
                }
        }
    }
}
