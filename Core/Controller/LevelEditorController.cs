using UnityEngine;

namespace Plamb.LevelEditor.Core
{
    /// <summary>
    /// Stores level editor managers and relays references.
    /// </summary>
    [RequireComponent(typeof(PlatformManager)), RequireComponent(typeof(SerializationManager)),
     RequireComponent(typeof(NavigationManager)), RequireComponent(typeof(PlacementManager)),
     RequireComponent(typeof(UIManager)), RequireComponent(typeof(OnionSkinManager)),
     RequireComponent(typeof(LightingManager))]
    public class LevelEditorController : Singleton<LevelEditorController>
    {
        // Managers
        private PlatformManager m_platformManager;
        private SerializationManager m_serializationManager;
        private NavigationManager m_navigationManager;
        private PlacementManager m_placementManager;
        private UIManager m_uiManager;
        private OnionSkinManager m_onionSkinManager;
        private LightingManager m_lightingManager;
        
        // Camera
        [SerializeField] private LevelEditorCamera levelEditorCamera;
        
        // Settings
        [SerializeField] private LevelEditorSettings levelEditorSettings;
        
        private void Start()
        {
            // Get manager references
            m_platformManager = GetComponent<PlatformManager>();
            m_serializationManager = GetComponent<SerializationManager>();
            m_navigationManager = GetComponent<NavigationManager>();
            m_placementManager = GetComponent<PlacementManager>();
            m_uiManager = GetComponent<UIManager>();
            m_onionSkinManager = GetComponent<OnionSkinManager>();
            m_lightingManager = GetComponent<LightingManager>();
            
            // Relay injected dependencies
            m_platformManager.Initialize();
            m_serializationManager.Initialize(m_platformManager);
            m_navigationManager.Initialize(levelEditorSettings, m_placementManager, m_onionSkinManager, m_uiManager,
                levelEditorCamera.Camera);
            m_placementManager.Initialize(levelEditorSettings, m_navigationManager, m_platformManager, m_uiManager);
            m_uiManager.Initialize(m_navigationManager, m_platformManager, m_placementManager);
            m_onionSkinManager.Initialize(m_navigationManager, m_platformManager);
            m_lightingManager.Initialize();
            
            levelEditorCamera.Initialize(levelEditorSettings, m_navigationManager, m_placementManager);
        }
    }
}
