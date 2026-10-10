using System;
using UnityEngine;

namespace BurgerRush.Input
{
    /// <summary>
    /// Sürükle & bırak: raycast ile nesne yakalar, hareket ve bırakma olaylarını yayınlar (GDD 2.3).
    /// </summary>
    public class TouchDragHandler : Utils.Singleton<TouchDragHandler>
    {
        public static event Action<GameObject> OnDragStart;
        public static event Action<Vector2> OnDrag;
        public static event Action<GameObject> OnDropTarget;
        public static event Action OnDragEnd;

        [SerializeField] private LayerMask draggableLayers = ~0;
        [SerializeField] private float maxDragDistance = 50f;

        private GameObject draggedObject;
        private Camera mainCamera;

        private void Awake() => mainCamera = Camera.main;

        private void OnEnable()
        {
            InputManager.OnPointerDown += HandlePointerDown;
            InputManager.OnPointerMove += HandlePointerMove;
            InputManager.OnPointerUp += HandlePointerUp;
        }

        private void OnDisable()
        {
            InputManager.OnPointerDown -= HandlePointerDown;
            InputManager.OnPointerMove -= HandlePointerMove;
            InputManager.OnPointerUp -= HandlePointerUp;
        }

        private void HandlePointerDown(Vector2 position)
        {
            var ray = mainCamera.ScreenPointToRay(position);
            if (!Physics.Raycast(ray, out var hit, maxDragDistance, draggableLayers)) return;

            draggedObject = hit.collider.gameObject;
            OnDragStart?.Invoke(draggedObject);
        }

        private void HandlePointerMove(Vector2 position)
        {
            if (!draggedObject) return;
            OnDrag?.Invoke(position);
        }

        private void HandlePointerUp(Vector2 position)
        {
            if (!draggedObject) return;

            var ray = mainCamera.ScreenPointToRay(position);
            if (Physics.Raycast(ray, out var hit, maxDragDistance))
                OnDropTarget?.Invoke(hit.collider.gameObject);

            OnDragEnd?.Invoke();
            draggedObject = null;
        }
    }
}
