using System.Collections.Generic;
using System.Linq;
using BurgerRush.Gameplay.Kitchen;
using UnityEngine;

namespace BurgerRush.AI
{
    /// <summary>
    /// Mutfak kararları: hangi siparişe öncelik verilir, hangi slot kullanılır.
    /// Personel ve Smart Kitchen AI (Lv28) bu sınıfı kullanır.
    /// </summary>
    public static class AIDecisionMaker
    {
        /// <summary>En acil (sabrı az) ve mevcut burgerle eşleşen siparişi seçer.</summary>
        public static T PickMostUrgent<T>(IEnumerable<T> candidates, System.Func<T, float> patienceSelector)
            => candidates.OrderBy(patienceSelector).FirstOrDefault();

        public static bool ShouldCookMore(int freeSlots, int waitingCustomers)
            => freeSlots > 0 && waitingCustomers > 0;

        public static int PickFreeSlot(IReadOnlyList<bool> slotOccupied)
        {
            for (int i = 0; i < slotOccupied.Count; i++)
                if (!slotOccupied[i]) return i;
            return -1;
        }

        /// <summary>Burger yığınının siparişi karşılayıp karşılamadığı.</summary>
        public static bool Matches(BurgerBuilder builder, Gameplay.Customer.OrderData order)
            => builder.Matches(order);
    }
}
