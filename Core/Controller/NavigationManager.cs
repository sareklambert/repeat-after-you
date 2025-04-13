using System.Collections.Generic;
using UnityEngine;
using Plamb.Events;
using Plamb.Events.LevelEditor;

namespace Plamb.LevelEditor.Core
{
    /// <summary>
    /// Class <c>NavigationManager</c> manages layer navigation and mouse position in the editor.
    /// </summary>
    public class NavigationManager : MonoBehaviour
    {
        // References
        private LevelEditorSettings m_settings;
        private PlacementManager m_placementManager;
        private OnionSkinManager m_onionSkinManager;
        private UIManager m_uiManager;
        private Grid m_gridMain;
        public Grid GridMain => m_gridMain;
        private Grid m_gridSub;
        public Grid GridSub => m_gridSub;
        
        public Camera SceneCamera { get; private set; }
        [SerializeField] private GameObject gridParent;
        public GameObject GridParent => gridParent;
        [SerializeField] private GameObject gridParentTarget;
        [SerializeField] private GameObject depthIndicatorLine;
        public GameObject DepthIndicatorLine => depthIndicatorLine;
        [SerializeField] private LayerMask gridLayer;
        [SerializeField] private LayerMask objectLayer;
        public LayerMask ObjectLayer => objectLayer;
        private bool m_layerZoomModeActive = false;
        
        // Position variables
        public bool MouseInputBlocked { get; private set; }
        public Vector3 MousePositionPlane { get; private set; }
        public Vector3Int gridPositionMain { get; private set; }
        public Vector3Int gridPositionSub { get; private set; }
        public SubgridId currentSubgridId { get; private set; }
        public PlatformId currentPlatformId { get; private set; }

        // Layer variables
        public int currentLayer { get; private set; }
        [HideInInspector] public List<LayerVisibilityOption> layerVisibilityOptions;
        [HideInInspector] public LayerVisibilityOption layerVisibilityGlobalOption = LayerVisibilityOption.Visible;
        
        /// <summary>
        /// Initializes references.
        /// </summary>
        public void Initialize(LevelEditorSettings settings, PlacementManager placementManager, OnionSkinManager onionSkinManager,
            UIManager uiManager, Camera sceneCamera)
        {
            // Get references
            m_settings = settings;
            m_placementManager = placementManager;
            m_onionSkinManager = onionSkinManager;
            m_uiManager = uiManager;
            SceneCamera = sceneCamera;
            m_gridMain = GetComponent<Grid>();
            m_gridSub = transform.GetChild(0).GetComponent<Grid>();

            // Set grid parent position
            gridParent.transform.position = new Vector3(0, -currentLayer * m_gridMain.cellSize.y, 0);
            gridParentTarget.transform.position = gridParent.transform.position;
            
            currentLayer = m_settings.layerAmount / 2;
        }

        private void OnEnable()
        {
            // Subscribe to events
            EventBus.Subscribe<EventInputLayerUpDown>(LayerUpDown);
            EventBus.Subscribe<EventInputCameraZoomToggle>(OnLayerZoomModeToggle);
        }

        private void OnDisable()
        {
            // Unsubscribe from events
            EventBus.Unsubscribe<EventInputLayerUpDown>(LayerUpDown);
            EventBus.Unsubscribe<EventInputCameraZoomToggle>(OnLayerZoomModeToggle);
        }
        
        /// <summary>
        /// Checks, if we're hovering any UI element.
        /// Updates the mouse position if possible.
        /// </summary>
        private void Update()
        {
            // Don't do anything when we're hovering a UI element
            MouseInputBlocked = MouseOverUI.isMouseOverUIElement();
            if (MouseInputBlocked) return;

            // Only update anything when the layer switch animation is not running. Otherwise, we get visual glitches
            if (!Mathf.Approximately(gridParent.transform.position.y, gridParentTarget.transform.position.y)) return;

            UpdateMousePosition();
        }
        
        /// <summary>
        /// Updates the mouse position on the grid and retrieves hovered platform and subgrid IDs.
        /// </summary>
        private void UpdateMousePosition()
        {
            // Retrieve and adjust the mouse position
            Vector3 mouseScreenPos = LevelEditorInput.Instance.MousePositionScreen;
            mouseScreenPos.z = SceneCamera.nearClipPlane;

            // Perform raycast to detect grid intersection
            if (Physics.Raycast(SceneCamera.ScreenPointToRay(mouseScreenPos), out RaycastHit hit, 
                    m_settings.gridMouseRaycastLength, gridLayer))
            {
                MousePositionPlane = hit.point;
            }

            // Convert world position to grid coordinates
            gridPositionMain = m_gridMain.WorldToCell(MousePositionPlane);
            gridPositionSub = m_gridSub.WorldToCell(MousePositionPlane);

            // Apply coordinate offsets
            Vector2Int offsetMain = m_settings.gridPositionOffsetMain;
            Vector2Int offsetSub = m_settings.gridPositionOffsetSub;

            Vector2Int remappedMain = new Vector2Int(
                gridPositionMain.x + offsetMain.x,
                -gridPositionMain.z + offsetMain.y
            );

            Vector2Int remappedSub = new Vector2Int(
                gridPositionSub.x + offsetSub.x,
                -gridPositionSub.z + offsetSub.y
            );

            // Update IDs
            currentPlatformId = new PlatformId(currentLayer, remappedMain.x, remappedMain.y);
            currentSubgridId = new SubgridId(
                remappedSub.x % m_settings.subCellsPerMainCell,
                remappedSub.y % m_settings.subCellsPerMainCell
            );
        }

        /// <summary>
        /// Changes the current layer. Animates over time.
        /// </summary>
        public void ChangeCurrentLayerTo(int newLayerIndex)
        {
            // Set current layer
            currentLayer = newLayerIndex;

            // Animate grid parent object
            gridParentTarget.transform.position = new Vector3(0, -currentLayer * m_gridMain.cellSize.y, 0);

            // Set transparent materials
            m_onionSkinManager.UpdateOnionSkinMaterials(currentLayer);

            // Update UI
            m_uiManager.SetActiveFloor(newLayerIndex);
        }
        
        /// <summary>
        /// Sets a layer's visibility option.
        /// </summary>
        public void SetLayerVisibilityOption(int layerIndex, LayerVisibilityOption option)
        {
            layerVisibilityOptions[layerIndex] = option;

            m_onionSkinManager.UpdateOnionSkinMaterials(currentLayer);
        }
        
        #region Events
        private void LayerUpDown(EventInputLayerUpDown e)
        {
            // If we're not switching the layers instead of zooming, return
            if (m_layerZoomModeActive) return;

            if ((e.InputValue >= 1 && currentLayer + 1 <= m_settings.layerAmount - 1) || e.InputValue <= -1 &&
                currentLayer - 1 >= 0)
            {
                ChangeCurrentLayerTo(currentLayer + e.InputValue);
            }
        }
        
        private void OnLayerZoomModeToggle(EventInputCameraZoomToggle e)
        {
            m_layerZoomModeActive = e.Start;
        }
        #endregion
    }
}
