using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BurgerRush.Gameplay.Kitchen
{
    /// <summary>
    /// Katman yığını ve sipariş doğrulama (GDD 2.3).
    /// Sıra: Alt ekmek → köfte → (peynir/marul/domates/sos) → üst ekmek
    /// </summary>
    [System.Serializable]
    public class BurgerBuilder
    {
        private readonly List<FoodItem> layers = new();

        public IReadOnlyList<FoodItem> Layers => layers;
        public bool IsEmpty => layers.Count == 0;

        public bool HasBottomBun => layers.Count > 0 && layers[0].Type == IngredientType.Bun;
        public bool HasTopBun => layers.Count > 1 && layers[^1].Type == IngredientType.Bun;
        public bool HasPatty => Fillings.Any(f => f.Type == IngredientType.Patty);
        public bool HasRawPatty => Fillings.Any(f => f.Type == IngredientType.Patty && f.State != CookState.Cooked);
        public bool HasBurnt => layers.Any(l => l.IsBurnt);
        public bool IsComplete => HasBottomBun && HasTopBun && HasPatty;

        /// <summary>Alt ve üst ekmek hariç dolgu malzemeleri.</summary>
        public IEnumerable<FoodItem> Fillings
        {
            get
            {
                if (layers.Count == 0) return Enumerable.Empty<FoodItem>();
                int first = layers.FindIndex(l => l.Type == IngredientType.Bun);
                int last = layers.FindLastIndex(l => l.Type == IngredientType.Bun);
                return layers.Where((l, i) => i != first && i != last && first >= 0);
            }
        }

        public void AddLayer(FoodItem item) => layers.Add(item);

        public void Clear()
        {
            layers.Clear();
        }

        public float Value => layers.Sum(l => l.Value);

        /// <summary>Siparişle karşılaştırma sonucu.</summary>
        public struct Evaluation
        {
            public bool Ok;
            public string Reason;
            public List<IngredientType> Missing;
            public List<IngredientType> Extra;
            public float TipFactor;
        }

        public Evaluation Evaluate(OrderData order)
        {
            var result = new Evaluation { Missing = new List<IngredientType>(), Extra = new List<IngredientType>() };

            if (!HasBottomBun || !HasTopBun) { result.Reason = "Ekmek eksik!"; return result; }
            if (!HasPatty) { result.Reason = "Köfte yok!"; return result; }
            if (HasRawPatty) { result.Reason = "Köfte çiğ!"; return result; }
            if (HasBurnt) { result.Reason = "Yanmış köfte!"; return result; }

            var pool = new List<IngredientType>(order.Fillings);
            foreach (var f in Fillings)
            {
                if (pool.Remove(f.Type)) continue;
                result.Extra.Add(f.Type);
            }

            result.Missing = pool;
            result.Ok = result.Missing.Count == 0;
            result.TipFactor = result.Extra.Count * 0.1f;
            if (!result.Ok) result.Reason = "Yanlış sipariş!";
            return result;
        }

        public bool Matches(OrderData order) => Evaluate(order).Ok;
    }
}
