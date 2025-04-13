using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Plamb.LevelEditor.Placeables;

namespace Plamb.LevelEditor.Core
{
    /// <summary>
    /// Class <c>SerializationManager</c> manages level serialization and file handling.
    /// </summary>
    public class SerializationManager : MonoBehaviour
    {
        // References
        private PlatformManager m_platformManager;

        /// <summary>
        /// Initializes references.
        /// </summary>
        public void Initialize(PlatformManager platformManager)
        {
            m_platformManager = platformManager;
        }
        
        /// <summary>
        /// Saves the current level and enters playmode.
        /// </summary>
        public void EnterPlayMode()
        {
            if (!m_platformManager.LevelStartExists)
            {
                Debug.Log("Can't enter play mode when no level start element was placed!");
                return;
            }

            // Get file path
            string filePath = HelperFunctions.GetCustomLevelFolder();
            filePath = Path.Combine(filePath, "level.json");

            RAMSceneManager.Instance.SaveLevel(filePath);
            RAMSceneManager.Instance.SwitchToPlayMode(filePath);
        }
        
        /// <summary>
        /// Checks whether the level has both a start and end point.
        /// </summary>
        /// <returns>
        /// Returns true if both a LevelStart and a LevelGoal are found amongst the props
        /// and PlatformObjects respectively, and false otherwise.
        /// </returns>
        /// <remarks>A return value of true implies that the level is considered to be theoretically playable.</remarks>
        public bool LevelIsCompleted()
        {
            bool hasEnd = m_platformManager.platformObjects.Values.Any(p => p is LevelGoal);
            bool hasStart = m_platformManager.platformObjects.Values
                .SelectMany(p => p.Props.Values)
                .Any(p => p is LevelStart);

            return hasStart && hasEnd;
        }
        
        /// <summary>
        /// Method <c>DumpToJson</c> dumps the current m_platformObjects dict into a file at the defined filepath.
        /// <param name="fp">The filepath to dump the data too.</param>
        /// <param name="mayOverwrite">Defines if it's allowed to overwrite existing files.</param>
        /// <exception cref="IOException">Thrown when file could not be written.</exception>
        /// </summary>
        public void DumpToJson(string fp, bool mayOverwrite, string levelName, float levelRating, bool levelCompleted,
            int levelLightingSetting, string author, Guid guid)
        {
            // Check if we can overwrite the file
            if (File.Exists(fp) && !mayOverwrite)
            {
                throw new IOException("File exists and can't be overridden");
            }

            // Build json data
            LevelDataWrapper levelDataWrapper = new LevelDataWrapper
            {
                platforms = new List<PlatformWrapper>(),
                levelName = levelName,
                levelRating = levelRating,
                levelCompleted = levelCompleted,
                levelLightingSetting = levelLightingSetting,
                revision = 1,
                author = author,
                dateTime = new SerializableDateTime(DateTime.Now).DateTimeValueString,
                followUpLevel = "", // Will be overwritten again if a LevelGoal is found
                loadingDoneImage = ""
            };

            guid = guid == Guid.Empty ? Guid.NewGuid() : guid;
            levelDataWrapper.guid = guid.ToString();

            // Get platform objects
            foreach (var platformPair in m_platformManager.platformObjects)
            {
                // Get object
                Platform platform = platformPair.Value;

                // Create platform object wrapper
                PlatformWrapper platformWrapper = new PlatformWrapper
                {
                    // Get platform data
                    rotation = platform.Rotation,
                    platformId = platform.PlatformId.AsString(),
                    lookupKey = platform.LookupKey,
                    props = new List<PropWrapper>()
                };

                if (platform is LevelGoal goal) // Set follow-up level if set by author
                {
                    levelDataWrapper.followUpLevel = goal.NextLevelName;
                }

                // Get props objects
                foreach (var propPair in platform.Props)
                {
                    // Get object
                    Prop prop = propPair.Value;

                    // Create prop object wrapper
                    PropWrapper propWrapper = new PropWrapper
                    {
                        // Get prop object data
                        rotation = prop.Rotation,
                        subgridId = prop.SubgridId.AsString(),
                        lookupKey = prop.LookupKey
                    };

                    if (prop is ConnectableObject connectableObject)
                    {
                        ConnectionId connectionId = connectableObject.GetConnectionId();
                        if (connectionId is not null) propWrapper.connectionId = connectionId.AsString();
                    }

                    // Add prop object wrapper to platform wrapper
                    platformWrapper.props.Add(propWrapper);
                }

                // Add platform object wrapper to level wrapper
                levelDataWrapper.platforms.Add(platformWrapper);
            }

            // Write data to file
            File.WriteAllText(fp, JsonUtility.ToJson(levelDataWrapper, true));
        }

        /// <summary>
        /// Method <c>LoadFromJson</c> loads a json file and returns the contained dict.
        /// <param name="path">The filepath to read the json data from.</param>
        /// <exception cref="FileNotFoundException">Thrown when file could not be found.</exception>
        /// <exception cref="FormatException">Thrown when file is in the wrong format.</exception>
        /// <returns>The dictionary contained in the json.</returns>
        /// </summary>
        public Dictionary<PlatformId, LevelEditorObject> LoadFromJson(string path)
        {
            // Check if file exists
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("File could not be found");
            }

            // Create dictionary
            var dict = new Dictionary<PlatformId, LevelEditorObject>();

            // Load the json data
            string jsonData = File.ReadAllText(path);

            // Deserialize json data
            LevelDataWrapper levelDataWrapper = JsonUtility.FromJson<LevelDataWrapper>(jsonData);

            // Check if the data could be deserialized
            if (levelDataWrapper is null)
            {
                throw new FormatException("File is in the wrong format");
            }

            // Parse the data into the dictionary
            foreach (var platformWrapper in levelDataWrapper.platforms)
            {
                // Create a new platform object
                Platform platform = (Platform)m_platformManager.CreateInstanceByKey(platformWrapper.lookupKey);
                platform.Rotation = platformWrapper.rotation;
                platform.PlatformId = new PlatformId(platformWrapper.platformId);
                platform.LookupKey = platformWrapper.lookupKey;

                // Add props to platform
                foreach (var propWrapper in platformWrapper.props)
                {
                    // Create a new prop
                    Prop prop = (Prop)Instantiate(m_platformManager.objectDict[propWrapper.lookupKey]);
                    prop.Rotation = propWrapper.rotation;
                    prop.SubgridId = new SubgridId(propWrapper.subgridId);
                    prop.LookupKey = propWrapper.lookupKey;
                    if (propWrapper.connectionId is not null)
                    {
                        ConnectableObject connectable = prop as ConnectableObject;
                        if (connectable) connectable.SetConnectionId(new ConnectionId(propWrapper.connectionId));
                    }

                    // Add the prop to the platform
                    platform.Props ??= new Dictionary<SubgridId, Prop>();
                    platform.Props[prop.SubgridId] = prop;
                }

                // Add the platform object to the dictionary
                dict[platform.PlatformId] = platform;
            }

            // Return dictionary
            return dict;
        }
    }
}
