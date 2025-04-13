using UnityEngine;
using Plamb.Events;
using Plamb.Events.LevelEditor;

namespace Plamb.LevelEditor.Core
{
    /// <summary>
    /// Handles rotating functionality of the level editor's camera.
    /// </summary>
    public class CameraRotateController : MonoBehaviour
    {
        // References
        private PlacementManager m_placementManager;
        private LevelEditorCamera m_camera;

        private bool m_isDragging;

        [Header("Rotation Settings")]
        [SerializeField] private float rotationSpeed = 0.1f;

        /// <summary>
        /// Gets references, and subscribes to events.
        /// </summary>
        public void Initialize(PlacementManager placementManager, LevelEditorCamera camera)
        {
            m_placementManager = placementManager;
            m_camera = camera;
            
            SubscribeEvents();
        }

        private void OnDestroy() => UnsubscribeEvents();

        /// <summary>
        /// Resets the transform's rotation.
        /// </summary>
        public void ResetRotation()
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }

        #region Input Events

        private void SubscribeEvents()
        {
            EventBus.Subscribe<EventInputCameraRotateKey>(OnRotateKey);
            EventBus.Subscribe<EventInputCameraRotateMouseVector>(OnRotateDrag);
            EventBus.Subscribe<EventInputCameraRotateDragToggle>(OnRotateDragToggle);
        }

        private void UnsubscribeEvents()
        {
            EventBus.Unsubscribe<EventInputCameraRotateKey>(OnRotateKey);
            EventBus.Unsubscribe<EventInputCameraRotateMouseVector>(OnRotateDrag);
            EventBus.Unsubscribe<EventInputCameraRotateDragToggle>(OnRotateDragToggle);
        }

        private void OnRotateKey(EventInputCameraRotateKey e)
        {
            if (m_placementManager.currentPlacementMode != PlacementMode.None) return;

            float newY = transform.rotation.eulerAngles.y + e.InputValue;
            transform.rotation = Quaternion.Euler(0f, newY, 0f);
        }

        private void OnRotateDrag(EventInputCameraRotateMouseVector e)
        {
            if (!m_isDragging) return;

            float newY = transform.rotation.eulerAngles.y + e.InputVector.x * rotationSpeed;
            transform.rotation = Quaternion.Euler(0f, newY, 0f);
        }

        private void OnRotateDragToggle(EventInputCameraRotateDragToggle e)
        {
            m_isDragging = e.Start;
        }

        #endregion
    }
}
