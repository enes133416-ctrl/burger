using BurgerRush.Input;
using UnityEngine;

namespace BurgerRush.Gameplay.Kitchen
{
    /// <summary>
    /// Fizik tabanlı malzeme sürükleme: raycast + Rigidbody.AddForce + snap zone (GDD 2.3).
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class IngredientDraggable : MonoBehaviour
    {
        [Header("Drag")]
        [SerializeField] private float followForce = 45f;
        [SerializeField] private float damping = 8f;
        [SerializeField] private float maxSpeed = 12f;
        [SerializeField] private float dragHeight = 1.6f;

        private Rigidbody rb;
        private Camera mainCamera;
        private Plane dragPlane;
        private bool dragging;
        private Vector3 targetPoint;

        public FoodItem Food { get; private set; }
        public bool IsDragging => dragging;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            mainCamera = Camera.main;
            Food = GetComponent<FoodItem>();
            dragPlane = new Plane(Vector3.up, -dragHeight);
        }

        private void OnEnable()
        {
            TouchDragHandler.OnDragStart += HandleDragStart;
            TouchDragHandler.OnDrag += HandleDrag;
            TouchDragHandler.OnDragEnd += HandleDragEnd;
        }

        private void OnDisable()
        {
            TouchDragHandler.OnDragStart -= HandleDragStart;
            TouchDragHandler.OnDrag -= HandleDrag;
            TouchDragHandler.OnDragEnd -= HandleDragEnd;
        }

        private void HandleDragStart(GameObject target)
        {
            if (target != gameObject) return;
            dragging = true;
            rb.useGravity = false;
            rb.drag = damping;
            HapticManager.Instance?.TriggerLight();
        }

        private void HandleDrag(Vector2 screenPosition)
        {
            if (!dragging) return;
            var ray = mainCamera.ScreenPointToRay(screenPosition);
            if (dragPlane.Raycast(ray, out float distance))
                targetPoint = ray.GetPoint(distance);
        }

        private void HandleDragEnd()
        {
            if (!dragging) return;
            dragging = false;
            rb.useGravity = true;
        }

        private void FixedUpdate()
        {
            if (!dragging) return;

            var direction = targetPoint - rb.position;
            direction.y = 0f;
            rb.AddForce(direction.normalized * followForce, ForceMode.Acceleration);
            if (rb.velocity.magnitude > maxSpeed)
                rb.velocity = rb.velocity.normalized * maxSpeed;
        }
    }
}
