using UnityEngine;
using UnityEngine.InputSystem;
using Plamb.Events;
using Plamb.Events.LevelEditor;

namespace Plamb.LevelEditor.Core
{
    // Forwards input events to the event bus and keeps track of the mouse position
    public class LevelEditorInput : Singleton<LevelEditorInput>
    {
        // Private members
        private LevelEditorInputs m_levelEditorInputs;

        private void Start()
        {
            m_levelEditorInputs = new LevelEditorInputs();
            SubscribeToEvents();
        }
        
        private void OnEnable()
        {
            if (m_levelEditorInputs == null) return;
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            if (m_levelEditorInputs == null) return;
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            m_levelEditorInputs.Navigation.Enable();
            m_levelEditorInputs.Placement.Enable();
            
            m_levelEditorInputs.Placement.ToggleDuplicateMode.performed += OnToggleDuplicateMode;
            m_levelEditorInputs.Placement.ToggleDeleteMode.performed += OnToggleDeleteMode;
            m_levelEditorInputs.Placement.ToggleBuildMode.performed += OnToggleBuildMode;
            m_levelEditorInputs.Placement.AbortAction.performed += OnAbortAction;
            
            m_levelEditorInputs.Placement.RotateSelectedObject.performed += OnRotateSelectedObject;
            m_levelEditorInputs.Placement.PlaceSelectedObject.performed += OnPlaceSelectedObject;

            m_levelEditorInputs.Navigation.LayerUpDown.performed += OnLayerUpDown;
            
            m_levelEditorInputs.Navigation.CameraPanDrag.started += OnCameraPanDrag;
            m_levelEditorInputs.Navigation.CameraPanDrag.canceled += OnCameraPanDrag;
            m_levelEditorInputs.Navigation.CameraPanSprint.started += OnCameraPanSprint;
            m_levelEditorInputs.Navigation.CameraPanSprint.canceled += OnCameraPanSprint;

            m_levelEditorInputs.Navigation.CameraRotateDrag.started += OnCameraRotateDrag;
            m_levelEditorInputs.Navigation.CameraRotateDrag.canceled += OnCameraRotateDrag;
            m_levelEditorInputs.Navigation.CameraRotateDeltaValue.performed += OnCameraRotateDeltaValue;

            m_levelEditorInputs.Navigation.CameraZoom.performed += OnCameraZoom;
            m_levelEditorInputs.Navigation.ToggleLayerZoomMode.started += OnToggleLayerZoomMode;
            m_levelEditorInputs.Navigation.ToggleLayerZoomMode.canceled += OnToggleLayerZoomMode;
        }

        private void UnsubscribeFromEvents()
        {
            m_levelEditorInputs.Navigation.Disable();
            m_levelEditorInputs.Placement.Disable();
            
            m_levelEditorInputs.Placement.ToggleDuplicateMode.performed -= OnToggleDuplicateMode;
            m_levelEditorInputs.Placement.ToggleDeleteMode.performed -= OnToggleDeleteMode;
            m_levelEditorInputs.Placement.ToggleBuildMode.performed -= OnToggleBuildMode;
            m_levelEditorInputs.Placement.AbortAction.performed -= OnAbortAction;
            
            m_levelEditorInputs.Placement.RotateSelectedObject.performed -= OnRotateSelectedObject;
            m_levelEditorInputs.Placement.PlaceSelectedObject.performed -= OnPlaceSelectedObject;

            m_levelEditorInputs.Navigation.LayerUpDown.performed -= OnLayerUpDown;
            
            m_levelEditorInputs.Navigation.CameraPanDrag.started -= OnCameraPanDrag;
            m_levelEditorInputs.Navigation.CameraPanDrag.canceled -= OnCameraPanDrag;
            m_levelEditorInputs.Navigation.CameraPanSprint.started -= OnCameraPanSprint;
            m_levelEditorInputs.Navigation.CameraPanSprint.canceled -= OnCameraPanSprint;

            m_levelEditorInputs.Navigation.CameraRotateDrag.started -= OnCameraRotateDrag;
            m_levelEditorInputs.Navigation.CameraRotateDrag.canceled -= OnCameraRotateDrag;
            m_levelEditorInputs.Navigation.CameraRotateDeltaValue.performed -= OnCameraRotateDeltaValue;

            m_levelEditorInputs.Navigation.CameraZoom.performed -= OnCameraZoom;
            m_levelEditorInputs.Navigation.ToggleLayerZoomMode.started -= OnToggleLayerZoomMode;
            m_levelEditorInputs.Navigation.ToggleLayerZoomMode.canceled -= OnToggleLayerZoomMode;
        }
        
        // Mouse position input
        public Vector2 MousePositionScreen { get; private set; }
        
        // Update mouse position and keys that can be held down
        private void Update()
        {
            MousePositionScreen = Mouse.current.position.ReadValue();

            Vector2 panKeyInput = m_levelEditorInputs.Navigation.CameraPanKey.ReadValue<Vector2>();
            if (panKeyInput != Vector2.zero)
            {
                EventBus.Publish(new EventInputCameraPanKey(panKeyInput));
            }
            
            float rotateKeyInput = m_levelEditorInputs.Navigation.CameraRotateKey.ReadValue<float>();
            if (rotateKeyInput != 0f)
            {
                EventBus.Publish(new EventInputCameraRotateKey(rotateKeyInput));
            }
        }
        
        #region Forward input events
        // Placement mode
        public void OnToggleDuplicateMode(InputAction.CallbackContext _)
        {
            EventBus.Publish(new EventInputSetPlacementMode(PlacementMode.Duplicate));
        }
        public void OnToggleDeleteMode(InputAction.CallbackContext _)
        {
            EventBus.Publish(new EventInputSetPlacementMode(PlacementMode.Delete));
        }
        public void OnToggleBuildMode(InputAction.CallbackContext _)
        {
            EventBus.Publish(new EventInputSetPlacementMode(PlacementMode.Build));
        }
        public void OnAbortAction(InputAction.CallbackContext _)
        {
            EventBus.Publish(new EventInputSetPlacementMode(PlacementMode.None));
        }
        
        // Object manipulation
        public void OnRotateSelectedObject(InputAction.CallbackContext context)
        {
            EventBus.Publish(new EventInputRotateSelectedObject((int)context.ReadValue<float>()));
        }
        public void OnPlaceSelectedObject(InputAction.CallbackContext _)
        {
            EventBus.Publish(new EventInputPlaceSelectedObject());
        }
        
        // Navigation
        // Layer
        public void OnLayerUpDown(InputAction.CallbackContext context)
        {
            EventBus.Publish(new EventInputLayerUpDown((int)context.ReadValue<float>()));
        }
        
        // Camera pan
        public void OnCameraPanDrag(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Started:
                    EventBus.Publish(new EventInputCameraPanDrag(true));
                    break;
                case InputActionPhase.Canceled:
                    EventBus.Publish(new EventInputCameraPanDrag(false));
                    break;
            }
        }
        public void OnCameraPanSprint(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Started:
                    EventBus.Publish(new EventInputCameraPanSprint(true));
                    break;
                case InputActionPhase.Canceled:
                    EventBus.Publish(new EventInputCameraPanSprint(false));
                    break;
            }
        }

        // Camera rotate
        public void OnCameraRotateDrag(InputAction.CallbackContext context)
        {
            bool isPerformed = context.phase == InputActionPhase.Started;
            EventBus.Publish(new EventInputCameraRotateDragToggle(isPerformed));
        }
        public void OnCameraRotateDeltaValue(InputAction.CallbackContext context)
        {
            EventBus.Publish(new EventInputCameraRotateMouseVector(context.ReadValue<Vector2>()));
        }
        
        // Camera zoom
        public void OnCameraZoom(InputAction.CallbackContext context)
        {
            EventBus.Publish(new EventInputCameraZoomVector(context.ReadValue<Vector2>()));
        }
        public void OnToggleLayerZoomMode(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Started:
                    EventBus.Publish(new EventInputCameraZoomToggle(true));
                    break;
                case InputActionPhase.Canceled:
                    EventBus.Publish(new EventInputCameraZoomToggle(false));
                    break;
            }
        }
        #endregion
    }
}
