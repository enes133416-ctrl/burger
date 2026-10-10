using System;
using UnityEngine;

namespace BurgerRush.Input
{
    /// <summary>
    /// Dokunma/fare girdisinin tek giriş noktası: tüm sürükleme olaylarını yayınlar.
    /// </summary>
    public class InputManager : Utils.Singleton<InputManager>
    {
        public static event Action<Vector2> OnPointerDown;
        public static event Action<Vector2> OnPointerMove;
        public static event Action<Vector2> OnPointerUp;

        [SerializeField] private bool enableMouseInEditor = true;

        private Camera mainCamera;
        private bool pointerDown;

        public Camera MainCamera => mainCamera ? mainCamera : Camera.main;

        private void Awake() => mainCamera = Camera.main;

        private void Update()
        {
            bool hasTouch = UnityEngine.Input.touchCount > 0;

            if (hasTouch)
            {
                var touch = UnityEngine.Input.GetTouch(0);
                Vector2 position = touch.position;
                switch (touch.phase)
                {
                    case TouchPhase.Began: pointerDown = true; OnPointerDown?.Invoke(position); break;
                    case TouchPhase.Moved:
                    case TouchPhase.Stationary: OnPointerMove?.Invoke(position); break;
                    case TouchPhase.Ended:
                    case TouchPhase.Canceled: pointerDown = false; OnPointerUp?.Invoke(position); break;
                }
                return;
            }

#if UNITY_EDITOR || UNITY_STANDALONE
            if (!enableMouseInEditor) return;

            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                pointerDown = true;
                OnPointerDown?.Invoke(UnityEngine.Input.mousePosition);
            }
            else if (pointerDown && UnityEngine.Input.GetMouseButton(0))
            {
                OnPointerMove?.Invoke(UnityEngine.Input.mousePosition);
            }
            else if (pointerDown && UnityEngine.Input.GetMouseButtonUp(0))
            {
                pointerDown = false;
                OnPointerUp?.Invoke(UnityEngine.Input.mousePosition);
            }
#endif
        }

        public Ray RayFromScreen(Vector2 screenPosition) => MainCamera.ScreenPointToRay(screenPosition);

        public bool TryRaycast<T>(Vector2 screenPosition, out T component) where T : Component
        {
            component = null;
            var ray = RayFromScreen(screenPosition);
            if (Physics.Raycast(ray, out var hit))
                component = hit.collider.GetComponentInParent<T>();
            return component != null;
        }
    }
}
