using System;
using System.Collections;
using BurgerRush.Audio;
using BurgerRush.Core;
using BurgerRush.Economy;
using BurgerRush.Gameplay.Progression;
using BurgerRush.Utils;
using UnityEngine;
using UnityEngine.AI;

namespace BurgerRush.Gameplay.Customer
{
    /// <summary>
    /// Müşteri yapay zekâsı: kuyruğa yürü → sipariş ver → bekle → servis/öfkelen (GDD 2.5).
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class CustomerAI : MonoBehaviour
    {
        public enum CustomerState { Spawning, WalkingToQueue, Waiting, Ordering, WaitingForFood, Leaving, Angry }

        [Header("Customer Settings")]
        [SerializeField] private CustomerType customerType = CustomerType.Casual;
        [SerializeField] private float basePatienceDuration = 60f;

        [Header("References")]
        [SerializeField] private PatienceBar patienceBar;
        [SerializeField] private Animator animator;
        [SerializeField] private AudioSource audioSource;

        public CustomerType Type => customerType;
        public CustomerState CurrentState { get; private set; } = CustomerState.Spawning;
        public OrderData CurrentOrder { get; private set; }
        public float PatienceRatio => maxPatience > 0f ? Mathf.Clamp01(currentPatience / maxPatience) : 0f;
        public bool IsWaiting => CurrentState == CustomerState.Waiting || CurrentState == CustomerState.WaitingForFood;
        public string ChannelId { get; set; } = "counter";

        public event Action<CustomerAI> OnCustomerLeave;
        public event Action<CustomerAI, bool> OnOrderComplete;

        private NavMeshAgent navAgent;
        private float patienceMultiplier = 1f;
        private float currentPatience;
        private float maxPatience = 60f;
        private bool isServed;
        private Vector3 queueTarget;

        private void Awake()
        {
            navAgent = GetComponent<NavMeshAgent>();
            if (!patienceBar) patienceBar = GetComponentInChildren<PatienceBar>();
            if (!animator) animator = GetComponent<Animator>();
            ConfigureByType();
        }

        private void ConfigureByType()
        {
            patienceMultiplier = customerType switch
            {
                CustomerType.Casual => 1.0f,
                CustomerType.BusinessPerson => 2.5f,
                CustomerType.Child => 3.0f,
                CustomerType.VIP => 0.7f,
                CustomerType.Influencer => 1.5f,
                _ => 1.0f,
            };
        }

        public void Initialize(CustomerType type, Data.LevelData level)
        {
            customerType = type;
            ConfigureByType();

            float patienceBuff = Economy.UpgradeSystem.Instance ? Economy.UpgradeSystem.Instance.PatienceMultiplier : 1f;
            maxPatience = (basePatienceDuration / patienceMultiplier) * patienceBuff;
            currentPatience = maxPatience;

            CurrentOrder = OrderData.Generate(level, type);
            ChangeState(CustomerState.Spawning);
        }

        public void MoveToQueue(Vector3 position)
        {
            queueTarget = position;
            ChangeState(CustomerState.WalkingToQueue);
            if (navAgent && navAgent.isOnNavMesh) navAgent.SetDestination(position);
        }

        private void Update()
        {
            switch (CurrentState)
            {
                case CustomerState.WalkingToQueue:
                    if (navAgent && !navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance + 0.15f)
                        ChangeState(CustomerState.Ordering);
                    break;

                case CustomerState.Ordering:
                    ChangeState(CustomerState.Waiting);
                    break;

                case CustomerState.Waiting:
                case CustomerState.WaitingForFood:
                    UpdatePatience();
                    break;
            }
        }

        private void UpdatePatience()
        {
            currentPatience -= Time.deltaTime;
            patienceBar?.UpdateBar(PatienceRatio);

            if (currentPatience <= 0f) StartCoroutine(LeaveAngrily());
        }

        public void ResetPatience() => currentPatience = maxPatience;

        public void ReceiveOrder(System.Collections.Generic.List<Gameplay.Kitchen.FoodItem> burger)
        {
            var builder = new Kitchen.BurgerBuilder();
            foreach (var item in burger) builder.AddLayer(item);

            var evaluation = builder.Evaluate(CurrentOrder);
            isServed = true;

            float pay = CurrentOrder.BasePrice;
            float tip = 0f;

            if (evaluation.Ok)
            {
                tip = PatienceRatio > 0.8f ? pay * 0.5f : PatienceRatio > 0.5f ? pay * 0.2f : 0f;
                tip += pay * evaluation.TipFactor;
                tip += pay * (Economy.UpgradeSystem.Instance ? Economy.UpgradeSystem.Instance.TipBonus : 0f);
                AudioManager.Instance?.PlaySFX("CustomerHappy");
            }
            else
            {
                pay *= 0.5f;
                StarRatingSystem.Instance?.DeductStar();
                AudioManager.Instance?.PlaySFX("CustomerAngry");
            }

            float total = Mathf.Max(1f, Mathf.Round((pay + tip) * Economy.EconomyManager.Instance.TotalIncomeMultiplier));
            Economy.EconomyManager.Instance.AddCoins(total);

            EventBus.Publish(new CustomerServedEvent { Customer = this, Tip = tip, WasWrong = !evaluation.Ok });
            OnOrderComplete?.Invoke(this, evaluation.Ok);

            StartCoroutine(evaluation.Ok ? LeaveHappily() : LeaveAngrily());
        }

        private IEnumerator LeaveAngrily()
        {
            ChangeState(CustomerState.Angry);
            if (animator) animator.SetTrigger("Angry");
            AudioManager.Instance?.PlaySFX("Angry");
            yield return new WaitForSeconds(1f);
            FinishLeave();
        }

        private IEnumerator LeaveHappily()
        {
            ChangeState(CustomerState.Leaving);
            if (animator) animator.SetTrigger("Happy");
            yield return new WaitForSeconds(0.5f);
            FinishLeave();
        }

        private void FinishLeave()
        {
            OnCustomerLeave?.Invoke(this);
            if (Utils.ObjectPooler.Instance) Utils.ObjectPooler.Instance.ReturnToPool(Constants.Pool.Customer, gameObject);
            else Destroy(gameObject);
        }

        public void ChangeState(CustomerState newState)
        {
            CurrentState = newState;
            if (animator) animator.SetInteger("State", (int)newState);
        }
    }
}
