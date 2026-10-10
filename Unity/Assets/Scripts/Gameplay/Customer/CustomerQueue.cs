using System.Collections.Generic;
using UnityEngine;

namespace BurgerRush.Gameplay.Customer
{
    /// <summary>
    /// Kuyruk noktalarını yönetir: sıra boşaldığında müşteriler öne kayar (GDD 2.2).
    /// </summary>
    public class CustomerQueue : MonoBehaviour
    {
        [SerializeField] private Transform[] slots;
        public int Capacity => slots != null ? slots.Length : 0;

        private readonly List<CustomerAI> occupants = new();

        public bool HasRoom => occupants.Count < Capacity;
        public int Count => occupants.Count;

        public bool TryEnqueue(CustomerAI customer)
        {
            if (!HasRoom || customer == null) return false;
            occupants.Add(customer);
            RefreshPositions();
            return true;
        }

        public void Remove(CustomerAI customer)
        {
            if (occupants.Remove(customer)) RefreshPositions();
        }

        public void RefreshPositions()
        {
            for (int i = 0; i < occupants.Count; i++)
                if (occupants[i]) occupants[i].MoveToQueue(slots[i].position);
        }

        public void Clear() => occupants.Clear();

        public CustomerAI First => occupants.Count > 0 ? occupants[0] : null;
        public IReadOnlyList<CustomerAI> Occupants => occupants;
    }
}
