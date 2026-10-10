using UnityEngine;

namespace BurgerRush.Multiplayer
{
    /// <summary>
    /// Oyuncu avatarının ağ üzerindeki senkronizasyonu (pozisyon + durum).
    /// NetworkTransform yerine basit interpolasyonlu çözüm.
    /// </summary>
    public class PlayerSync : MonoBehaviour
    {
        [SerializeField] private float interpolationSpeed = 12f;

        public string PlayerId { get; private set; }
        public bool IsBusy { get; set; }
        public bool IsLocalPlayer { get; private set; }

        private Vector3 targetPosition;
        private Quaternion targetRotation = Quaternion.identity;

        public void Initialize(string playerId, bool isLocal)
        {
            PlayerId = playerId;
            IsLocalPlayer = isLocal;
            targetPosition = transform.position;
            targetRotation = transform.rotation;
        }

        private void Update()
        {
            if (IsLocalPlayer) return;

            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * interpolationSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * interpolationSpeed);
        }

        public void SetNetworkState(Vector3 position, Quaternion rotation, bool busy)
        {
            targetPosition = position;
            targetRotation = rotation;
            IsBusy = busy;
        }

        public void SetBusy(bool busy) => IsBusy = busy;
    }
}
