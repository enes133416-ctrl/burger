using UnityEngine;
using UnityEngine.AI;

namespace BurgerRush.AI
{
    /// <summary>
    /// Drive-Thru araçları: şeride girer, pencereye ilerler, sipariş sonrası çıkar (GDD 2.2).
    /// </summary>
    public class CarController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float driveSpeed = 6f;
        [SerializeField] private float arrivalThreshold = 0.25f;
        [SerializeField] private float wheelRotationSpeed = 220f;

        [Header("Wheels")]
        [SerializeField] private Transform[] wheels;

        [Header("Audio")]
        [SerializeField] private AudioSource engineAudio;

        private Vector3 targetPosition;
        private bool driving;
        private bool exited;

        public bool HasArrived => !driving;

        public void DriveTo(Vector3 position)
        {
            targetPosition = position;
            driving = true;
            if (engineAudio && !engineAudio.isPlaying) engineAudio.Play();
        }

        public void Exit(Vector3 exitPoint)
        {
            exited = true;
            DriveTo(exitPoint);
        }

        private void Update()
        {
            if (!driving) return;

            var direction = targetPosition - transform.position;
            direction.y = 0f;

            if (direction.magnitude <= arrivalThreshold)
            {
                driving = false;
                if (engineAudio) engineAudio.Stop();
                if (exited) Destroy(gameObject);
                return;
            }

            transform.position += direction.normalized * (driveSpeed * Time.deltaTime);
            if (direction.magnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);

            RotateWheels();
        }

        private void RotateWheels()
        {
            if (wheels == null) return;
            foreach (var w in wheels)
                if (w) w.Rotate(Vector3.right, wheelRotationSpeed * Time.deltaTime, Space.Self);
        }

        public void StopEngine()
        {
            if (engineAudio) engineAudio.Stop();
        }
    }
}
