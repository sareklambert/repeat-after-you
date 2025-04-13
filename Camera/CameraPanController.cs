using UnityEngine;
using Plamb.Events;
using Plamb.Events.LevelEditor;

namespace Plamb.LevelEditor.Core
{
    /// <summary>
    /// Handles panning functionality of the level editor's camera.
    /// </summary>
    public class CameraPanController : MonoBehaviour
    {
        // References
        private LevelEditorSettings m_settings;
        private NavigationManager m_navigationManager;
        private LevelEditorCamera m_camera;
        private Transform cameraTransform => m_camera.CameraTransform;

        private Vector3 m_targetPosition;
        private Vector3 m_dragOrigin = Vector3.zero;
        private bool m_isDragging;
        private bool m_isSprinting;

        private float m_zOffset;
        private float m_currentSpeed;

        [Header("Pan Settings")]
        [SerializeField] private float baseSpeed = 0.7f;
        [SerializeField] private float sprintSpeed = 1.8f;
        [SerializeField] private float lerpSpeed = 12f;
        [SerializeField] private float zoomAdjustment = 0.2f;
        [SerializeField] private float screenEdgeRange = 0.05f;

        /// <summary>
        /// Gets references, resets values, and subscribes to events.
        /// </summary>
        public void Initialize(LevelEditorSettings levelEditorSettings, NavigationManager navigationManager,
            LevelEditorCamera camera)
        {
            m_settings = levelEditorSettings;
            m_navigationManager = navigationManager;
            m_camera = camera;
            
            m_zOffset = transform.position.z;
            m_targetPosition = transform.position;

            SubscribeEvents();
        }

        private void OnDestroy() => UnsubscribeEvents();

        /// <summary>
        /// Resets the transform's position.
        /// </summary>
        public void ResetPosition()
        {
            m_targetPosition = transform.position = new Vector3(0f, transform.position.y, 0f);
        }

        /// <summary>
        /// Handles panning; animates position over time.
        /// </summary>
        private void FixedUpdate()
        {
            PanByScreenEdge();
            PanDragApply();
            
            transform.position = Vector3.Lerp(transform.position, m_targetPosition, lerpSpeed * Time.deltaTime);
        }

        /// <summary>
        /// Handles panning input by hovering over the screen's edge.
        /// </summary>
        private void PanByScreenEdge()
        {
            if (m_navigationManager.MouseInputBlocked || m_isDragging) return;

            Vector3 moveDir = Vector3.zero;
            Vector2 mouse = LevelEditorInput.Instance.MousePositionScreen;

            if (mouse.x < screenEdgeRange * Screen.width)
                moveDir -= GetCameraRight();
            else if (mouse.x > (1f - screenEdgeRange) * Screen.width)
                moveDir += GetCameraRight();

            if (mouse.y < screenEdgeRange * Screen.height)
                moveDir -= GetCameraForward();
            else if (mouse.y > (1f - screenEdgeRange) * Screen.height)
                moveDir += GetCameraForward();

            if (moveDir != Vector3.zero)
                ApplyPan(moveDir);
        }

        /// <summary>
        /// Applies pan input.
        /// </summary>
        private void ApplyPan(Vector3 direction)
        {
            UpdateSpeed();
            m_targetPosition += direction * m_currentSpeed;
            ClampPosition();
        }

        /// <summary>
        /// Calculates panning speed based on zoom level and sprint input.
        /// </summary>
        private void UpdateSpeed()
        {
            m_currentSpeed = m_isSprinting ? sprintSpeed : baseSpeed;

            var zoomController = m_camera.GetComponent<CameraZoomController>();
            float zoom = zoomController.CurrentZoom;
            float min = zoomController.ZoomMin;
            float max = zoomController.ZoomMax;

            float factor = Mathf.Approximately(min, max) ? 0f : Mathf.Clamp01((zoom - max) / (min - max));
            m_currentSpeed *= zoomAdjustment + (1 - zoomAdjustment) * factor;
        }

        /// <summary>
        /// Clamps panning target position to the grid.
        /// </summary>
        private void ClampPosition()
        {
            float half = m_settings.gridSizeHalf;
            m_targetPosition.x = Mathf.Clamp(m_targetPosition.x, -half, half);
            m_targetPosition.z = Mathf.Clamp(m_targetPosition.z, -half + m_zOffset, half + m_zOffset);
        }

        /// <summary>
        /// Gets the camera's forward vector.
        /// </summary>
        private Vector3 GetCameraForward()
        {
            var forward = cameraTransform.forward;
            forward.y = 0;
            return forward.normalized;
        }

        /// <summary>
        /// Gets the camera's right vector.
        /// </summary>
        private Vector3 GetCameraRight()
        {
            var right = cameraTransform.right;
            right.y = 0;
            return right.normalized;
        }

        #region Input Events

        private void SubscribeEvents()
        {
            EventBus.Subscribe<EventInputCameraPanKey>(OnPanKey);
            EventBus.Subscribe<EventInputCameraPanDrag>(OnPanDrag);
            EventBus.Subscribe<EventInputCameraPanSprint>(OnSprintToggle);
        }

        private void UnsubscribeEvents()
        {
            EventBus.Unsubscribe<EventInputCameraPanKey>(OnPanKey);
            EventBus.Unsubscribe<EventInputCameraPanDrag>(OnPanDrag);
            EventBus.Unsubscribe<EventInputCameraPanSprint>(OnSprintToggle);
        }

        private void OnPanKey(EventInputCameraPanKey e)
        {
            Vector2 input = e.InputVector;
            Vector3 dir = (input.x * GetCameraRight() + input.y * GetCameraForward()).normalized;
            if (dir.sqrMagnitude <= 0.01f) return;

            ApplyPan(dir);
            m_dragOrigin = Vector3.zero;
        }

        private void OnPanDrag(EventInputCameraPanDrag e)
        {
            if (e.Performed)
            {
                if (m_isDragging) return;

                m_isDragging = true;
                m_dragOrigin = m_navigationManager.MousePositionPlane;
            }
            else
            {
                m_isDragging = false;
            }
        }

        private void PanDragApply()
        {
            if (!m_isDragging || m_dragOrigin == Vector3.zero || m_navigationManager.MouseInputBlocked)
                return;

            Vector3 diff = m_dragOrigin - m_navigationManager.MousePositionPlane;
            diff.y = 0;
            m_targetPosition += diff;
            ClampPosition();
            transform.position = m_targetPosition;
        }

        private void OnSprintToggle(EventInputCameraPanSprint e)
        {
            m_isSprinting = e.Start;
        }

        #endregion
    }
}
