using UnityEngine;

namespace BurgerRush.Gameplay.Customer
{
    /// <summary>
    /// Müşterinin kafasının üzerindeki sabır barı (world-space UI, kameraya bakar).
    /// </summary>
    public class PatienceBar : MonoBehaviour
    {
        [SerializeField] private Transform fill;
        [SerializeField] private Renderer fillRenderer;
        [SerializeField] private Color safeColor = new(0.18f, 0.80f, 0.44f);
        [SerializeField] private Color warnColor = new(1f, 0.84f, 0f);
        [SerializeField] private Color dangerColor = new(0.91f, 0.30f, 0.24f);

        private Camera mainCamera;

        private void Awake() => mainCamera = Camera.main;

        private void LateUpdate()
        {
            if (mainCamera) transform.forward = mainCamera.transform.forward;
        }

        public void UpdateBar(float normalized)
        {
            normalized = Mathf.Clamp01(normalized);
            if (fill) fill.localScale = new Vector3(normalized, fill.localScale.y, fill.localScale.z);
            if (!fillRenderer) return;

            fillRenderer.material.color = normalized > 0.6f ? safeColor : normalized > 0.35f ? warnColor : dangerColor;
        }
    }
}
