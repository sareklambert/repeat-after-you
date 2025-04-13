using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Plamb.LevelEditor.Placeables;

namespace Plamb.LevelEditor.Core
{
    /// <summary>
    /// Class <c>OnionSkinManager</c> manages applying onion skinning materials of level editor objects.
    /// </summary>
    public class OnionSkinManager : MonoBehaviour
    {
        // Second dictionary that holds instantiated versions of all LevelEditorObjects
        // This is used to get the original materials when resetting them for onion skinning
        // The objects must be instantiated, as all values in the lookupTableObject are private
        // serialized fields and are only accessible once the objects initialization function has run.
        private Dictionary<string, LevelEditorObject> m_objectInstanceDict =
            new Dictionary<string, LevelEditorObject>();

        // Onion skinning materials
        [SerializeField] private Material matTransparent1TOP;
        [SerializeField] private Material matTransparent1BOT;
        [SerializeField] private Material matTransparent2TOP;
        [SerializeField] private Material matTransparent2BOT;

        // Manager references
        private NavigationManager m_navigationManager;
        private PlatformManager m_platformManager;
        
        /// <summary>
        /// Initializes references.
        /// </summary>
        public void Initialize(NavigationManager navigationManager, PlatformManager platformManager)
        {
            // Get references
            m_navigationManager = navigationManager;
            m_platformManager = platformManager;
            
            // Instantiate levelEditorObjects in second dict
            foreach (TableObject t in m_platformManager.LookupTableObject.objectList)
            {
                LevelEditorObject obj = m_platformManager.CreateInstanceByKey(t.id);
                m_objectInstanceDict.Add(t.id, obj);
                obj.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// Set different transparency materials for onion skinning based on the objects levels distance to the
        /// currently selected layer
        /// </summary>
        public void UpdateOnionSkinMaterials(int currentLayer)
        {
            // Loop through platforms
            foreach (KeyValuePair<PlatformId, Platform> platform in m_platformManager.platformObjects)
            {
                // Get distance
                int distanceToCenterLayer = platform.Key.Level - currentLayer;

                // Get visibility option (current layer is always visible)
                LayerVisibilityOption visibilityOption = distanceToCenterLayer == 0 ? LayerVisibilityOption.Visible :
                    m_navigationManager.layerVisibilityOptions[platform.Key.Level];

                // Get platform object
                Platform obj = platform.Value;

                // Set materials
                SetObjectMaterial(obj, visibilityOption, distanceToCenterLayer);

                // Loop through subgrid objects
                foreach (KeyValuePair<SubgridId, Prop> prop in platform.Value.Props)
                {
                    // Get subgrid object
                    Prop obj2 = prop.Value;

                    // Set materials
                    SetObjectMaterial(obj2, visibilityOption, distanceToCenterLayer);
                }
            }
        }
        
        /// <summary>
        /// Gets the onion skinning material to use based on the layer's distance to the center layer.
        /// </summary>
        private Material GetOnionSkinMaterial(int distanceToCenterLayer)
        {
            return distanceToCenterLayer switch
            {
                -1 => matTransparent1TOP,
                1 => matTransparent1BOT,
                -2 => matTransparent2TOP,
                _ => matTransparent2BOT
            };
        }

        /// <summary>
        /// Apples the original material to an object.
        /// </summary>
        private void ApplyOriginalMaterials(LevelEditorObject obj)
        {
            for (int i = 0; i < obj.Renderers.Count; i++)
            {
                var originalMaterials = m_objectInstanceDict[obj.LookupKey].Renderers[i].materials;
                obj.Renderers[i].materials = (Material[])originalMaterials.Clone(); // Cleaner than Array.Copy
            }
        }

        /// <summary>
        /// Apples the onion skinning material to an object.
        /// </summary>
        private void ApplyOnionSkinMaterial(LevelEditorObject obj, Material mat)
        {
            foreach (Renderer rend in obj.Renderers)
            {
                var newMats = Enumerable.Repeat(mat, rend.materials.Length).ToArray();
                rend.materials = newMats;
            }
        }

        /// <summary>
        /// Sets an object's onion skinning or original material based on its visibility option.
        /// </summary>
        private void SetObjectMaterial(LevelEditorObject obj, LayerVisibilityOption visibilityOption,
            int distanceToCenterLayer)
        {
            switch (visibilityOption)
            {
                case LayerVisibilityOption.Visible:
                    obj.SetVisibility(true);
                    ApplyOriginalMaterials(obj);
                    break;

                case LayerVisibilityOption.OnionSkinning:
                    if (Mathf.Abs(distanceToCenterLayer) > 2)
                        obj.SetVisibility(false);
                    else
                    {
                        obj.SetVisibility(true);
                        ApplyOnionSkinMaterial(obj, GetOnionSkinMaterial(distanceToCenterLayer));
                    }
                    break;
            }
        }
    }
}
