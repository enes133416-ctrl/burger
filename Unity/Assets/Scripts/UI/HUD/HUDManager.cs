using System.Collections.Generic;
using BurgerRush.Core;
using BurgerRush.Gameplay.Customer;
using UnityEngine;

namespace BurgerRush.UI.HUD
{
    /// <summary>
    /// Oyun içi HUD: müşteri balonları, boost etiketleri, alt butonlar (GDD 5.1).
    /// </summary>
    public class HUDManager : Utils.Singleton<HUDManager>
    {
        [SerializeField] private PatientBar patienceBarPrefab;
        [SerializeField] private RectTransform bubbleContainer;
        [SerializeField] private GameObject doubleIncomeTag;

        private readonly Dictionary<CustomerAI, PatientBar> bars = new();

        private void OnEnable() => EventBus.Subscribe<StateChangedEvent>(OnStateChanged);
        private void OnDisable() => EventBus.Unsubscribe<StateChangedEvent>(OnStateChanged);

        private void Update()
        {
            if (doubleIncomeTag)
                doubleIncomeTag.SetActive(Economy.EconomyManager.Instance && Economy.EconomyManager.Instance.IsDoubleIncomeActive);
        }

        private void OnStateChanged(StateChangedEvent e)
            => gameObject.SetActive(e.State == GameManager.GameState.Gameplay);

        public void RegisterCustomer(CustomerAI customer)
        {
            if (!customer || !patienceBarPrefab || !bubbleContainer || bars.ContainsKey(customer)) return;
            var bar = Instantiate(patienceBarPrefab, bubbleContainer);
            bars[customer] = bar;
        }

        public void UnregisterCustomer(CustomerAI customer)
        {
            if (!bars.TryGetValue(customer, out var bar)) return;
            if (bar) Destroy(bar.gameObject);
            bars.Remove(customer);
        }

        public void ShowToast(string message) => Debug.Log($"[HUD] {message}");
    }
}
