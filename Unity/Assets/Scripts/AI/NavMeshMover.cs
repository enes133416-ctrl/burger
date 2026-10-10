using UnityEngine;
using UnityEngine.AI;

namespace BurgerRush.AI
{
    /// <summary>
    /// NavMesh üzerinde hareket yardımcısı: hedef, durma mesafesi, varış kontrolü.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class NavMeshMover : MonoBehaviour
    {
        private NavMeshAgent agent;
        private Vector3 destination;
        private bool hasDestination;

        public bool HasArrived =>
            !hasDestination || (agent && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f);

        public float Speed { get => agent ? agent.speed : 0f; set { if (agent) agent.speed = value; } }

        private void Awake() => agent = GetComponent<NavMeshAgent>();

        public void MoveTo(Vector3 target, float stoppingDistance = 0.2f)
        {
            destination = target;
            hasDestination = true;
            if (!agent) return;
            agent.stoppingDistance = stoppingDistance;
            if (agent.isOnNavMesh) agent.SetDestination(target);
        }

        public void Stop()
        {
            hasDestination = false;
            if (agent && agent.isOnNavMesh) agent.ResetPath();
        }

        public bool TrySamplePosition(Vector3 origin, float range, out Vector3 result)
        {
            result = origin;
            if (NavMesh.SamplePosition(origin, out var hit, range, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
            return false;
        }
    }
}
