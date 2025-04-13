using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Plamb.LevelEditor.Placeables;

namespace Plamb.LevelEditor.Core
{
    /// <summary>
    /// Class <c>PlatformManager</c> manages LevelEditorObjects and referencing.
    /// </summary>
    public class PlatformManager : MonoBehaviour
    {
        // Where we save the level in
        public Dictionary<PlatformId, Platform> platformObjects { get; private set; } =
            new Dictionary<PlatformId, Platform>();

        // Dictionary that holds all LevelEditorObjects
        [SerializeField] private LookupTableObject lookupTableObject;
        public LookupTableObject LookupTableObject => lookupTableObject;
        public Dictionary<string, LevelEditorObject> objectDict { get; private set; } =
            new Dictionary<string, LevelEditorObject>();

        // Public flag
        public bool LevelStartExists { get; private set; }

        /// <summary>
        /// Initializes the object dictionary.
        /// Ensures the lookup table is assigned and populates the internal dictionary with ID-prefab mappings.
        /// </summary>
        /// <exception cref="NullReferenceException">
        /// Thrown if <c>lookupTableObject</c> has not been assigned in the inspector or elsewhere before Start is called.
        /// </exception>
        public void Initialize()
        {
            // Ensure the lookup table is assigned before proceeding
            if (lookupTableObject is null)
            {
                throw new NullReferenceException($"{nameof(lookupTableObject)} must be assigned!");
            }
    
            // Populate the internal dictionary with ID-prefab mappings from the lookup table
            foreach (TableObject t in lookupTableObject.objectList)
            {
                objectDict.Add(t.id, t.prefab);
            }
        }

        /// <summary>
        /// Deletes all instances of Props of the specified type from each Platform in the collection.
        /// </summary>
        /// <param name="type">The Type of the Props to be deleted.</param>
        public void DeletePropsOfType(Type type)
        {
            // Iterate through each Platform object in the collection
            foreach (Platform platform in platformObjects.Values)
            {
                // Find all keys in the Props dictionary where the Prop's type matches the specified type
                List<SubgridId> keysToRemove = platform.Props
                    .Where(p => p.Value.GetType() == type)
                    .Select(p => p.Key)
                    .ToList(); // ToList is needed to avoid modifying the collection during iteration

                // For each matching key, delete the Prop and remove it from the dictionary
                foreach (var key in keysToRemove)
                {
                    GetPlatformAtPosition(platform.Props[key].PlatformId).
                        RemoveFromPlatform(platform.Props[key].SubgridId);
                    platform.Props.Remove(key);
                }
            }
        }

        /// <summary>
        /// Delete all platforms from the given layer.
        /// </summary>
        /// <param name="layerIndex">The index of the layer to clear.</param>
        public void ClearLayer(int layerIndex)
        {
            // Collect keys to delete to avoid modifying the collection while iterating
            foreach (var platformId in platformObjects.Keys.ToList().Where(platformId => platformId.Level
                         == layerIndex))
            {
                DeletePlatform(platformId);
            }
        }

        /// <summary>
        /// Retrieves the lookup key (ID) from the lookup table at the specified index.
        /// </summary>
        /// <param name="index">The index in the lookup table's object list.</param>
        /// <returns>The lookup key (ID) as a string.</returns>
        public string GetLookUpKeyByIndex(int index)
        {
            return lookupTableObject.objectList[index].id;
        }

        /// <summary>
        /// Retrieves the Platform object at the given PlatformId position.
        /// </summary>
        /// <param name="platformId">The ID of the platform to retrieve.</param>
        /// <returns>The Platform object if found; otherwise, null.</returns>
        public Platform GetPlatformAtPosition(PlatformId platformId)
        {
            return platformObjects.GetValueOrDefault(platformId);
        }

        /// <summary>
        /// Instantiates a LevelEditorObject using the provided lookup key.
        /// </summary>
        /// <param name="key">The lookup key used to retrieve the object template from the dictionary.</param>
        /// <returns>A new instance of the LevelEditorObject with the LookupKey set.</returns>
        public LevelEditorObject CreateInstanceByKey(string key)
        {
            LevelEditorObject obj = Instantiate(objectDict[key]);
            obj.LookupKey = key;
            return obj;
        }

        /// <summary>
        /// Places a Platform in the scene at the specified PlatformId location and adds it to the platform collection.
        /// </summary>
        /// <param name="platformId">The unique identifier for the platform's position.</param>
        /// <param name="obj">The Platform object to place.</param>
        public void PlacePlatform(PlatformId platformId, Platform obj)
        {
            obj.PlatformId = platformId;
            platformObjects.Add(platformId, obj);
        }

        /// <summary>
        /// Deletes the Platform at the specified PlatformId, including all associated Props.
        /// </summary>
        /// <param name="platformId">The ID of the platform to delete.</param>
        public void DeletePlatform(PlatformId platformId)
        {
            // Delete all subgrid objects
            Platform platform = GetPlatformAtPosition(platformId);

            foreach (SubgridId key in platform.Props.Keys.ToList())
            {
                GetPlatformAtPosition(platform.Props[key].PlatformId).
                    RemoveFromPlatform(platform.Props[key].SubgridId);
                platform.Props.Remove(key);
            }

            // Delete platform
            Destroy(platform.Prefab);
            platformObjects.Remove(platformId);
        }
    }
}
