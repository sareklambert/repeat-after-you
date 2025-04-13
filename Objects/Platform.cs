using System.Collections.Generic;
using UnityEngine;
using Plamb.LevelEditor.Core;

namespace Plamb.LevelEditor.Placeables
{
    /// <summary>
    /// Class <c>Platform</c> defines a generic platform object for the player to walk on.
    /// It may store props placed on it.
    /// </summary>
    public class Platform : LevelEditorObject
    {
        // Define properties
        public bool TakesProps { get; private set; }
        public Dictionary<SubgridId, Prop> Props { get; set; } = new Dictionary<SubgridId, Prop>();
        
        // Expose properties to inspector with backing fields
        [Header("Platform settings")]
        [SerializeField] private bool takesProps;
        
        private void Awake()
        {
            InitializePlatform(takesProps);
        }

        private void InitializePlatform(bool takesProps)
        {
            TakesProps = takesProps;
        }
        
        /// <summary>
        /// Places the prop on the platform if it's not null and if the subgridId does not already exist.
        /// </summary>
        public void PlaceOnPlatform(SubgridId subgridId, Prop prop)
        {
            // Make sure the prop exists and the space is not taken
            if (!prop || Props.ContainsKey(subgridId)) return;
            
            // Assign IDs and add to the dict
            prop.PlatformId = PlatformId;
            prop.SubgridId = subgridId;
            Props.Add(subgridId, prop);
        }

        /// <summary>
        /// Removes the object from the platform if it exists.
        /// </summary>
        public void RemoveFromPlatform(SubgridId subgridId)
        {
            // Make sure there is a prop at the subgrid id
            if (!Props.ContainsKey(subgridId)) return;
            
            // Destroy instance and remove from dict
            Destroy(Props[subgridId].Prefab);
            Props.Remove(subgridId);
        }
    }
}
