using UnityEngine;
using Plamb.Events;
using Plamb.Events.LevelEditor;
using Plamb.LevelEditor.Placeables;

namespace Plamb.LevelEditor.Core
{
    /// <summary>
    /// Class <c>PlacementManager</c> manages Placement of level editor objects.
    /// </summary>
    public class PlacementManager : MonoBehaviour
    {
        // References
        private LevelEditorSettings m_settings;
        private NavigationManager m_navigationManager;
        private PlatformManager m_platformManager;
        private UIManager m_uiManager;
        
        // Object we're placing
        public LevelEditorObject objectToPlace { get; private set; }
        private string m_objectToPlaceKey = "";
        private ObjectType m_objectToPlaceType;
        private Vector3 m_objectToPlacePositionOffsetVector;

        public PlacementMode currentPlacementMode { get; private set; } = PlacementMode.None;
        public bool hoveredSpaceIsFree { get; private set; }
        private LevelEditorObject m_currentHoveredObject;
        
        /// <summary>
        /// Initializes references.
        /// </summary>
        public void Initialize(LevelEditorSettings settings, NavigationManager navigationManager,
            PlatformManager platformManager, UIManager uiManager)
        {
            // Get references
            m_settings = settings;
            m_navigationManager = navigationManager;
            m_platformManager = platformManager;
            m_uiManager = uiManager;
        }

        private void OnEnable()
        {
            // Subscribe to events
            EventBus.Subscribe<EventInputRotateSelectedObject>(RotateSelectedObject);
            EventBus.Subscribe<EventInputPlaceSelectedObject>(PlaceSelectedObject);
            EventBus.Subscribe<EventInputSetPlacementMode>(SelectPlacementMode);
        }

        private void OnDisable()
        {
            // Unsubscribe from events
            EventBus.Unsubscribe<EventInputRotateSelectedObject>(RotateSelectedObject);
            EventBus.Unsubscribe<EventInputPlaceSelectedObject>(PlaceSelectedObject);
            EventBus.Unsubscribe<EventInputSetPlacementMode>(SelectPlacementMode);
        }
        
        /// <summary>
        /// Handles placement mode updates based on current placement mode.
        /// </summary>
        private void Update()
        {
            // Don't do anything when we're hovering a UI element
            if (m_navigationManager.MouseInputBlocked) return;

            switch (currentPlacementMode)
            {
                case PlacementMode.Build:
                    UpdateSelectedObjectsPosition();
                    GetHoveredSpaceIsFree();

                    objectToPlace.SetVisibility(hoveredSpaceIsFree);
                    break;
                case PlacementMode.Delete:
                case PlacementMode.Duplicate:
                    m_currentHoveredObject = GetHoveredObject();
                    break;
            }
        }
        
        /// <summary>
        /// Selects an object to be placed in the scene based on the provided key.
        /// </summary>
        /// <param name="key">The identifier key for the object to be instantiated and placed.</param>
        public void SelectObjectToPlace(string key)
        {
            m_objectToPlaceKey = key;

            InstantiateSelectedObject();

            SetPlacementMode(PlacementMode.Build);

            m_navigationManager.DepthIndicatorLine.SetActive(true);
        }
        
        /// <summary>
        /// Deselects the currently selected object for placement and resets placement-related settings.
        /// </summary>
        private void DeselectObjectToPlace()
        {
            DestroySelectedObject();

            m_objectToPlaceKey = "";
            SetPlacementMode(PlacementMode.None);

            m_navigationManager.DepthIndicatorLine.SetActive(false);
        }
        
        /// <summary>
        /// Sets the current placement mode and updates the UI accordingly.
        /// </summary>
        /// <param name="mode">The placement mode to be set (e.g., Build, None).</param>
        private void SetPlacementMode(PlacementMode mode)
        {
            currentPlacementMode = mode;
            m_uiManager.SetCursor(currentPlacementMode);
        }
        
        /// <summary>
        /// Instantiates the currently selected object for placement based on the stored key.
        /// </summary>
        private void InstantiateSelectedObject()
        {
            DestroySelectedObject();

            objectToPlace = m_platformManager.CreateInstanceByKey(m_objectToPlaceKey);
            objectToPlace.transform.SetParent(m_navigationManager.GridParent.transform);

            m_objectToPlaceType = (objectToPlace is Platform) ? ObjectType.Platform : ObjectType.Prop;

            UpdateSelectedObjectsPosition();
        }
        
        /// <summary>
        /// Destroys the currently selected object, if one exists.
        /// </summary>
        private void DestroySelectedObject()
        {
            // Delete current object if there is one
            if (objectToPlace)
            {
                Destroy(objectToPlace.gameObject);
            }

            objectToPlace = null;
        }
        
        /// <summary>
        /// Updates the position of the selected object based on the current grid position and object type.
        /// </summary>
        private void UpdateSelectedObjectsPosition()
        {
            // Store objects rotation and reset it so it doesn't mess with the position
            Quaternion rotation = objectToPlace.transform.rotation;
            objectToPlace.transform.rotation = Quaternion.identity;

            // Get the selected objects offset vector
            m_objectToPlacePositionOffsetVector = GetSelectedObjectOffsetVector();

            // Snap the object to the grid using the correct grid and the offset vector
            var position = m_objectToPlaceType == ObjectType.Platform ?
                m_navigationManager.GridMain.CellToWorld(m_navigationManager.gridPositionMain) :
                m_navigationManager.GridSub.CellToWorld(m_navigationManager.gridPositionSub);
            position += m_objectToPlacePositionOffsetVector;

            // Set objects transform variables
            objectToPlace.transform.SetPositionAndRotation(position, rotation);

            // Show depth indicator line at objects position
            m_navigationManager.DepthIndicatorLine.SetActive(true);
            m_navigationManager.DepthIndicatorLine.transform.position = position;
        }
        
        /// <summary>
        /// Calculates the positional offset vector needed to center the selected object on the grid.
        /// </summary>
        /// <returns>A <see cref="Vector3"/> representing the offset to apply to the object's position.</returns>
        private Vector3 GetSelectedObjectOffsetVector()
        {
            float unitSize = m_settings.gridCellUnitSizeSub;
            float length = objectToPlace.Length;
            float width = objectToPlace.Width;

            // Default to zero offset
            Vector3 offset = Vector3.zero;

            if (m_objectToPlaceType == ObjectType.Platform)
            {
                offset.x = (length / 2f) * unitSize;
                offset.z = (width / 2f) * unitSize;
            }
            else
            {
                float xOffset = (length % 2 == 0) ? 0f : 0.5f;
                float zOffset = (width % 2 == 0) ? 0f : 0.5f;

                bool rotated90 = Mathf.Approximately(objectToPlace.Rotation, 90f) ||
                                 Mathf.Approximately(objectToPlace.Rotation, 270f);

                offset.x = (rotated90 ? zOffset : xOffset) * unitSize;
                offset.z = (rotated90 ? xOffset : zOffset) * unitSize;
            }

            return offset;
        }
        
        /// <summary>
        /// Returns the level editor object currently hovered over by the mouse cursor.
        /// </summary>
        /// <returns>
        /// A <see cref="LevelEditorObject"/> representing the object the mouse is currently pointing at,
        /// or <c>null</c> if no object is hovered.
        /// </returns>
        private LevelEditorObject GetHoveredObject()
        {
            Ray ray = m_navigationManager.SceneCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, m_settings.gridMouseRaycastLength,
                m_navigationManager.ObjectLayer))
            {
                LevelEditorObject obj = hit.collider.gameObject.GetComponent<LevelEditorObject>();
                return obj;
            }

            return null;
        }
        
        /// <summary>
        /// Checks if the space under the current hovered position is available for placing an object.
        /// </summary>
        private void GetHoveredSpaceIsFree()
        {
            if (m_objectToPlaceType == ObjectType.Platform)
            {
                hoveredSpaceIsFree = m_platformManager.GetPlatformAtPosition(m_navigationManager.currentPlatformId)
                    is null;
                return;
            }

            hoveredSpaceIsFree = false;

            Platform platform = m_platformManager.GetPlatformAtPosition(m_navigationManager.currentPlatformId);
            if (!platform) return;

            int subCells = m_settings.subCellsPerMainCell;

            // Calculate platform bounds in subgrid space
            int xMin = m_navigationManager.gridPositionMain.x * subCells;
            int zMin = (-m_navigationManager.gridPositionMain.z - 1) * subCells;
            int xMax = (m_navigationManager.gridPositionMain.x + 1) * subCells - 1;
            int zMax = -m_navigationManager.gridPositionMain.z * subCells - 1;
            Vector4 platformBounds = new Vector4(xMin, zMin, xMax, zMax);

            // Calculate new object's bounds
            Vector2 subPos = new Vector2(m_navigationManager.gridPositionSub.x, -m_navigationManager.gridPositionSub.z);
            Vector4 newObjectBounds = GetSubgridObjectsGridCoordinates((Prop)objectToPlace, subPos);

            // Check if new object stays within platform bounds
            if (!IsWithinBounds(newObjectBounds, platformBounds)) return;

            // Check for collision with existing objects
            foreach (var propPair in platform.Props)
            {
                Prop placedProp = propPair.Value;

                Vector2 placedPos = new Vector2(
                    placedProp.SubgridId.Column + platformBounds.x - 1,
                    placedProp.SubgridId.Row + platformBounds.y + 1
                );

                Vector4 placedBounds = GetSubgridObjectsGridCoordinates(placedProp, placedPos);

                if (IsOverlapping(newObjectBounds, placedBounds))
                    return;
            }

            hoveredSpaceIsFree = true;
        }

        private bool IsWithinBounds(Vector4 obj, Vector4 bounds)
        {
            return obj.x >= bounds.x && obj.y >= bounds.y && obj.z <= bounds.z && obj.w <= bounds.w;
        }

        private bool IsOverlapping(Vector4 a, Vector4 b)
        {
            return !(a.z < b.x || b.z < a.x || a.y > b.w || b.y > a.w);
        }

        /// <summary>
        /// Calculates the grid coordinates for a prop object within a subgrid based on its rotation and dimensions.
        /// </summary>
        /// <param name="o">The prop object for which the grid coordinates are being calculated.</param>
        /// <param name="subgridPosition">The current position of the object within the subgrid (top-left corner).</param>
        /// <returns>
        /// A <see cref="Vector4"/> representing the grid coordinates of the prop object as (left, top, right, bottom).
        /// These coordinates define the object's bounding box in the subgrid, adjusted for its rotation.
        /// </returns>
        private Vector4 GetSubgridObjectsGridCoordinates(Prop o, Vector2 subgridPosition)
        {
            bool rotated = Mathf.Approximately(o.Rotation, 90f) || Mathf.Approximately(o.Rotation, 270f);

            float length = rotated ? o.Width : o.Length;
            float width = rotated ? o.Length : o.Width;

            float halfLength = Mathf.Floor(length / 2f);
            float halfWidth = Mathf.Floor(width / 2f);

            float left = halfLength;
            float right = length - halfLength;
            float top = width - halfWidth;
            float bottom = halfWidth;

            return new Vector4(
                subgridPosition.x - left,
                subgridPosition.y - top,
                subgridPosition.x + right - 1,
                subgridPosition.y + bottom - 1
            );
        }
        
        #region Events
        private void SelectPlacementMode(EventInputSetPlacementMode e)
        {
            DeselectObjectToPlace();
            SetPlacementMode(e.Mode);
        }
        
        private void RotateSelectedObject(EventInputRotateSelectedObject e)
        {
            if (currentPlacementMode != PlacementMode.Build) return;

            // Rotate
            objectToPlace.Rotation = (int)Mathf.Repeat(objectToPlace.Rotation + 90 * e.InputValue, 360);
            objectToPlace.transform.rotation = Quaternion.Euler(objectToPlace.transform.rotation.eulerAngles.x,
                objectToPlace.Rotation, objectToPlace.transform.rotation.eulerAngles.z);
        }
        
        private void PlaceSelectedObject(EventInputPlaceSelectedObject e)
        {
            // Don't do anything when we're hovering a UI element
            if (m_navigationManager.MouseInputBlocked) return;

            switch (currentPlacementMode)
            {
                case PlacementMode.Build when hoveredSpaceIsFree:
                {
                    // Un attach object from parent
                    objectToPlace.transform.parent = null;

                    // Place object
                    if (m_objectToPlaceType == ObjectType.Platform)
                    {
                        m_platformManager.PlacePlatform(m_navigationManager.currentPlatformId, (Platform)objectToPlace);
                    }
                    else
                    {
                        // Check for level start / end, make sure only one exists
                        if (objectToPlace.GetType() == typeof(LevelStart))
                        {
                            m_platformManager.DeletePropsOfType(typeof(LevelStart));
                            m_uiManager.AddLevelStart(m_navigationManager.currentLayer);
                        }
                        else if (objectToPlace.GetType() == typeof(LevelGoal))
                        {
                            m_platformManager.DeletePropsOfType(typeof(LevelGoal));
                            m_uiManager.AddLevelEnd(m_navigationManager.currentLayer);
                        }

                        // Place object
                        Platform platform = m_platformManager.GetPlatformAtPosition(m_navigationManager.currentPlatformId);
                        platform.PlaceOnPlatform(m_navigationManager.currentSubgridId, (Prop)objectToPlace);
                    }

                    // Remove reference so it won't be deleted when we spawn a new object
                    objectToPlace = null;

                    // Create new instance for the next object
                    InstantiateSelectedObject();
                    break;
                }
                case PlacementMode.Duplicate when m_currentHoveredObject is not null:
                    // Copy hovered object and toggle build mode
                    m_objectToPlaceKey = m_currentHoveredObject.GetComponent<LevelEditorObject>().LookupKey;

                    InstantiateSelectedObject();

                    SetPlacementMode(PlacementMode.Build);
                    break;
                case PlacementMode.Delete when m_currentHoveredObject is not null:
                    // Delete hovered object
                    m_platformManager.DeletePlatform(m_currentHoveredObject.PlatformId);
                    break;
            }
        }
        #endregion
    }
}
