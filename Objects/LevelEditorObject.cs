using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Plamb.LevelEditor.Core;

namespace Plamb.LevelEditor.Placeables
{
    /// <summary>
    /// Abstract Class <c>LevelEditorObject</c> defines a generic, placeable object in the level editor.
    /// Derived classes implement generic platforms and props.
    /// </summary>
    public abstract class LevelEditorObject : MonoBehaviour
    {
        // Define properties
        public GameObject Prefab { get; private set; }
        public List<Renderer> Renderers { get; private set; }
        public string LookupKey { get; set; }
        public int Length { get; private set; }
        public int Width { get; private set; }
        public PlatformId PlatformId { get; set; }
        
        private int m_rotation;
        public int Rotation
        {
            get => m_rotation;
            set
            {
                // Make sure the rotation is only set to one of the allowed angles
                int[] allowedAngles = { 0, 90, 180, 270 };
                m_rotation = allowedAngles.OrderBy(a => Mathf.Abs(a - value)).First();
            }
        }
        
        // Expose properties to inspector with backing fields
        [Header("General settings")]
        [SerializeField] private string lookupKey;
        [SerializeField] private int length = 8;
        [SerializeField] private int width = 8;
        public SurfaceTypes surfaceType;

        private void Awake()
        {
            InitializeLevelEditorObject(length, width, lookupKey);
        }

        /// <summary>
        /// Initializes the object's public fields.
        /// </summary>
        private void InitializeLevelEditorObject(int length, int width, string lookupKey)
        {
            // Check for invalid values
            if (length == 0 || width == 0) throw new Exception("Invalid dimensions.");
            if (lookupKey == "") throw new Exception("Invalid lookup key.");
            
            // Set properties
            Length = length;
            Width = width;
            LookupKey = lookupKey;
            Renderers = GetComponentsInChildren<Renderer>().ToList();
            Prefab = gameObject;
        }
        
        /// <summary>
        /// Toggles all attached renderers.
        /// </summary>
        public void SetVisibility(bool visibility)
        {
            if (Renderers.Count <= 0) return;
            
            foreach (Renderer rend in Renderers)
            {
                rend.enabled = visibility;
            }
        }
    }

    public enum SurfaceTypes
    {
        Metal,
        Plastic,
        Wood,
        Cloth,
        LiquidThin,
        LiquidThick,
        Glass,
        Dirt
    }
}
