using System;
using BurgerRush.Gameplay.Customer;
using UnityEngine;

namespace BurgerRush.AI
{
    /// <summary>
    /// Hafif davranış ağacı: Selector / Sequence / Condition / Action düğümleri (GDD 1.3).
    /// Küçük AI'lar için yeterli; gerektiğinde Behaviour Designer/NodeCanvas'a taşınabilir.
    /// </summary>
    public abstract class BTNode
    {
        public enum Status { Success, Failure, Running }
        public abstract Status Tick();
    }

    public sealed class Selector : BTNode
    {
        private readonly BTNode[] children;
        public Selector(params BTNode[] children) => this.children = children;

        public override Status Tick()
        {
            foreach (var child in children)
            {
                var status = child.Tick();
                if (status != Status.Failure) return status;
            }
            return Status.Failure;
        }
    }

    public sealed class Sequence : BTNode
    {
        private readonly BTNode[] children;
        public Sequence(params BTNode[] children) => this.children = children;

        public override Status Tick()
        {
            foreach (var child in children)
            {
                var status = child.Tick();
                if (status != Status.Success) return status;
            }
            return Status.Success;
        }
    }

    public sealed class Condition : BTNode
    {
        private readonly Func<bool> predicate;
        public Condition(Func<bool> predicate) => this.predicate = predicate;
        public override Status Tick() => predicate() ? Status.Success : Status.Failure;
    }

    public sealed class ActionNode : BTNode
    {
        private readonly Func<Status> action;
        public ActionNode(Func<Status> action) => this.action = action;
        public override Status Tick() => action();
    }

    /// <summary>Müşteri AI için hazır ağaç: sabrı tükenmişse ayrıl, aksi halde bekle.</summary>
    public static class CustomerBehaviourTree
    {
        public static BTNode Build(CustomerAI customer, Action onLeave)
        {
            return new Selector(
                new Sequence(
                    new Condition(() => customer.PatienceRatio <= 0f),
                    new ActionNode(() =>
                    {
                        onLeave?.Invoke();
                        return Status.Success;
                    })),
                new ActionNode(() => Status.Running)
            );
        }
    }
}
