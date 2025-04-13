using UnityEngine;

namespace Plamb.LevelEditor.Core
{
    /// <summary>
    /// Defines the level editor's camera. Handles subcomponents.
    /// </summary>
    [RequireComponent(typeof(CameraPanController), typeof(CameraZoomController), typeof(CameraRotateController))]
    public class LevelEditorCamera : Singleton<LevelEditorCamera>
    {
        // Define public fields
        public Camera Camera { get; private set; }
        public Transform CameraTransform { get; private set; }

        // Component references
        private CameraPanController m_panController;
        private CameraZoomController m_zoomController;
        private CameraRotateController m_rotateController;

        /// <summary>
        /// Initializes components and resets camera.
        /// </summary>
        public void Initialize(LevelEditorSettings settings, NavigationManager navigationManager,
            PlacementManager placementManager)
        {
            m_panController.Initialize(settings, navigationManager, this);
            m_zoomController.Initialize(navigationManager, this);
            m_rotateController.Initialize(placementManager, this);

            Reset();
        }
        
        /// <summary>
        /// Gets component references.
        /// </summary>
        private void Awake()
        {
            Camera = GetComponentInChildren<Camera>();
            CameraTransform = Camera.transform;

            m_panController = GetComponent<CameraPanController>();
            m_zoomController = GetComponent<CameraZoomController>();
            m_rotateController = GetComponent<CameraRotateController>();
        }

        /// <summary>
        /// Resets camera's values.
        /// </summary>
        public void Reset()
        {
            transform.position = new Vector3(0f, transform.position.y, 0f);
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);

            m_zoomController.ResetZoom();
            m_panController.ResetPosition();
        }
    }
}
