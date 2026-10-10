using System;
using UnityEngine;

namespace BurgerRush.Input
{
    /// <summary>
    /// Swipe (kaydırma) algılama: sos ekleme, menü gezinme gibi jestler için.
    /// </summary>
    public class SwipeDetector : Utils.Singleton<SwipeDetector>
    {
        public enum Direction { None, Left, Right, Up, Down }
        public static event Action<Direction> OnSwipe;

        [SerializeField] private float minSwipeDistance = 80f;
        [SerializeField] private float maxSwipeTime = 0.6f;

        private Vector2 startPosition;
        private float startTime;
        private bool tracking;

        private void OnEnable()
        {
            InputManager.OnPointerDown += StartTracking;
            InputManager.OnPointerUp += EndTracking;
        }

        private void OnDisable()
        {
            InputManager.OnPointerDown -= StartTracking;
            InputManager.OnPointerUp -= EndTracking;
        }

        private void StartTracking(Vector2 position)
        {
            startPosition = position;
            startTime = Time.time;
            tracking = true;
        }

        private void EndTracking(Vector2 position)
        {
            if (!tracking) return;
            tracking = false;

            if (Time.time - startTime > maxSwipeTime) return;

            var delta = position - startPosition;
            if (delta.magnitude < minSwipeDistance) return;

            Direction direction = Mathf.Abs(delta.x) > Mathf.Abs(delta.y)
                ? (delta.x > 0 ? Direction.Right : Direction.Left)
                : (delta.y > 0 ? Direction.Up : Direction.Down);

            OnSwipe?.Invoke(direction);
        }
    }
}
