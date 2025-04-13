using UnityEngine;
using Plamb.Events;
using Plamb.Events.LevelEditor;

namespace Plamb.LevelEditor.Core
{
    /// <summary>
    /// Handles zooming functionality of the level editor's camera.
    /// </summary>
    public class CameraZoomController : MonoBehaviour
    {
        // References
        private NavigationManager m_navigationManager;
        private LevelEditorCamera m_camera;
        private Transform cameraTransform => m_camera.CameraTransform;

        private bool m_zoomModeActive;
        private float m_zoomTarget;
        private float m_zoomCurrent;

        public float CurrentZoom => m_zoomCurrent;
        public float ZoomMin => zoomMin;
        public float ZoomMax => zoomMax;

        [Header("Zoom Settings")]
        [SerializeField] private float zoomAmount = 3f;
        [SerializeField] private float zoomSpeed = 12f;
        [SerializeField] private float zoomMin = -75f;
        [SerializeField] private float zoomMax = 12f;
        [SerializeField] private float zoomStart = -23f;

        /// <summary>
        /// Gets references, resets values, and subscribes to events.
        /// </summary>
        public void Initialize(NavigationManager navigationManager, LevelEditorCamera camera)
        {
            m_navigationManager = navigationManager;
            m_camera = camera;

            m_zoomCurrent = zoomStart;
            m_zoomTarget = zoomStart;

            UpdateCameraPosition();
            SubscribeEvents();
        }

        private void OnDestroy() => UnsubscribeEvents();

        /// <summary>
        /// Resets the camera's zoom level.
        /// </summary>
        public void ResetZoom()
        {
            m_zoomCurrent = m_zoomTarget = zoomStart;
            UpdateCameraPosition();
        }

        /// <summary>
        /// Animates the zoom.
        /// </summary>
        private void FixedUpdate()
        {
            m_zoomCurrent = Mathf.Lerp(m_zoomCurrent, m_zoomTarget, zoomSpeed * Time.deltaTime);
            UpdateCameraPosition();
        }

        /// <summary>
        /// Calculates the camera's position based on zoom level.
        /// </summary>
        private void UpdateCameraPosition()
        {
            cameraTransform.position = transform.position + cameraTransform.forward * m_zoomCurrent;
        }

        #region Input Events

        private void SubscribeEvents()
        {
            EventBus.Subscribe<EventInputCameraZoomToggle>(OnZoomToggle);
            EventBus.Subscribe<EventInputCameraZoomVector>(OnZoomVector);
        }

        private void UnsubscribeEvents()
        {
            EventBus.Unsubscribe<EventInputCameraZoomToggle>(OnZoomToggle);
            EventBus.Unsubscribe<EventInputCameraZoomVector>(OnZoomVector);
        }

        private void OnZoomToggle(EventInputCameraZoomToggle e)
        {
            m_zoomModeActive = e.Start;
        }

        private void OnZoomVector(EventInputCameraZoomVector e)
        {
            if (m_navigationManager.MouseInputBlocked || !m_zoomModeActive) return;

            Vector2 input = e.InputVector.normalized;
            float delta = input.y;

            if (Mathf.Abs(delta) < 0.1f) return;

            m_zoomTarget += delta * zoomAmount;
            m_zoomTarget = Mathf.Clamp(m_zoomTarget, zoomMin, zoomMax);
        }

        #endregion
    }
}
