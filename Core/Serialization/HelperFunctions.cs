using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Plamb.LevelEditor.Core
{
    /// <summary>
    /// Defines helper functions for serialization.
    /// </summary>
    public class HelperFunctions : MonoBehaviour
    {
        private const string COMPANY_FOLDER = "OMCS";
        private const string GAME_FOLDER = "Repeat After Me";
        private const string BUILTIN_LEVEL_FOLDER = "Levels";
        private const string RESOURCES_FOLDER = "Resources";
        private const string CUSTOM_LEVEL_FOLDER = "CustomLevels";
        private const string LEVEL_EXTENSION = ".json";
        private static string BaseGameFolderPath => Path.Combine(GetLocalAppDataFolder(), COMPANY_FOLDER, GAME_FOLDER);

        /// <summary>
        /// Returns the local app data folder based on the current OS platform.
        /// </summary>
        /// <returns>String containing path of local app data folder.</returns>
        private static string GetLocalAppDataFolder()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return Environment.GetEnvironmentVariable("LOCALAPPDATA");
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return Environment.GetEnvironmentVariable("XDG_DATA_HOME") ?? Path.Combine(
                    Environment.GetEnvironmentVariable("HOME") ?? throw new InvalidOperationException(), ".local",
                    "share");
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return Path.Combine(Environment.GetEnvironmentVariable("HOME") ?? throw new InvalidOperationException(),
                    "Library", "Application Support");
            }

            throw new NotImplementedException("Unknown OS Platform");
        }

        /// <summary>
        /// Creates a folder inside the game's main directory.
        /// </summary>
        /// <param name="folderToCreate">The name of the folder to create.</param>
        private static void CreateFolderInGameFolder(string folderToCreate)
        {
            string folderPath = Path.Combine(BaseGameFolderPath, folderToCreate);
            Directory.CreateDirectory(folderPath);
        }

        /// <summary>
        /// Gets the path of the resources folder and creates it if it doesn't exist.
        /// </summary>
        /// <returns>String containing path of resources folder.</returns>
        private static string GetResourcesFolder()
        {
            return GetOrCreateFolderPath(RESOURCES_FOLDER);
        }

        /// <summary>
        /// Gets the path of a built-in level.
        /// </summary>
        /// <param name="levelName">The name of the level.</param>
        /// <returns>String containing path of the built-in level, or an empty string if the file doesn't exist.</returns>
        public static string GetBuiltInLevelPath(string levelName)
        {
            return GetLevelPath(GetBuiltinLevelFolder(), levelName);
        }

        /// <summary>
        /// Gets the path of a custom level.
        /// </summary>
        /// <param name="levelName">The name of the level.</param>
        /// <returns>String containing path of the custom level, or an empty string if the file doesn't exist.</returns>
        public static string GetCustomLevelPath(string levelName)
        {
            return GetLevelPath(GetCustomLevelFolder(), levelName);
        }

        private static string GetLevelPath(string folder, string levelName)
        {
            string levelFile = Path.Combine(folder, Path.ChangeExtension(levelName, LEVEL_EXTENSION));
            return File.Exists(levelFile) ? levelFile : "";
        }

        /// <summary>
        /// Gets the path of the built-in levels folder and creates it if it doesn't exist.
        /// </summary>
        /// <returns>String containing path of built-in levels folder.</returns>
        public static string GetBuiltinLevelFolder()
        {
            return GetOrCreateFolderPath(BUILTIN_LEVEL_FOLDER);
        }

        /// <summary>
        /// Gets the list of custom level files.
        /// </summary>
        /// <returns>Array of strings containing paths of custom level files.</returns>
        public static string[] GetCustomLevels()
        {
            return Directory.GetFiles(GetCustomLevelFolder(), "*" + LEVEL_EXTENSION);
        }

        /// <summary>
        /// Gets the list of built-in level files.
        /// </summary>
        /// <returns>Array of strings containing paths of built-in level files.</returns>
        public static string[] GetBuiltInLevels()
        {
            return Directory.GetFiles(GetBuiltinLevelFolder(), "*" + LEVEL_EXTENSION);
        }

        /// <summary>
        /// Gets the path of the custom levels folder and creates it if it doesn't exist.
        /// </summary>
        /// <returns>String containing path of custom levels folder.</returns>
        public static string GetCustomLevelFolder()
        {
            return GetOrCreateFolderPath(CUSTOM_LEVEL_FOLDER);
        }

        private static string GetOrCreateFolderPath(string folderName)
        {
            string folderPath = Path.Combine(BaseGameFolderPath, folderName);
            
            if (Directory.Exists(folderPath)) return folderPath;
            
            Debug.LogError($"{folderPath} does not exist.");
            CreateFolderInGameFolder(folderName);

            return folderPath;
        }

        /// <summary>
        /// Loads an image from the resources folder.
        /// </summary>
        /// <param name="imageName">Name of the image file.</param>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        /// <returns>A Texture2D object if the image was loaded successfully; otherwise, null.</returns>
        public static Texture2D LoadImageFromResources(string imageName, int width, int height)
        {
            if (string.IsNullOrWhiteSpace(imageName)) return null;

            string file = Path.Combine(GetResourcesFolder(), imageName);
            Texture2D img = null;

            if (!File.Exists(file)) return img;
            
            try
            {
                if (!ImageHeaderValidator.ValidateImageHeader(file))
                {
                    throw new FileLoadException("File does not contain a valid image signature.");
                }

                byte[] fileData = File.ReadAllBytes(file);
                img = new Texture2D(width, height);
                img.LoadImage(fileData);
            }
            catch (Exception err)
            {
                Debug.LogError($"Comic Strip could not be loaded from {file}.\n{err}");
                return null;
            }

            return img;
        }

        /// <summary>
        /// Provides functionality to validate image headers to ensure that a given file is a legitimate and
        /// recognized image format.
        /// </summary>
        private class ImageHeaderValidator
        {
            /// <summary>
            /// A dictionary mapping common image file extensions to their respective byte signatures.
            /// </summary>
            private static readonly Dictionary<string, byte[]> ImageSignatures = new Dictionary<string, byte[]>
            {
                { ".jpeg", new byte[] { 0xFF, 0xD8, 0xFF } },
                { ".jpg", new byte[] { 0xFF, 0xD8, 0xFF } },
                { ".png", new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } },
                { ".gif", new byte[] { 0x47, 0x49, 0x46, 0x38 } },
                { ".bmp", new byte[] { 0x42, 0x4D } },
                { ".tif", new byte[] { 0x49, 0x20, 0x49 } },
                { ".tiff", new byte[] { 0x49, 0x20, 0x49 } }
            };

            /// <summary>
            /// Validates the image header of the specified file.
            /// </summary>
            /// <param name="filePath">The path to the image file.</param>
            /// <returns>true if the file has a valid image header; otherwise, false.</returns>
            public static bool ValidateImageHeader(string filePath)
            {
                using FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                if (fileStream.Length < 12) return false;

                byte[] headerBytes = new byte[12];
                fileStream.Read(headerBytes, 0, headerBytes.Length);

                return ImageSignatures.Values.Any(signature => IsMatch(headerBytes, signature));
            }

            /// <summary>
            /// Checks if the file header matches any of the known image signatures.
            /// </summary>
            /// <param name="fileHeader">The header bytes of the file being checked.</param>
            /// <param name="imageSignature">The byte signature of a known image format.</param>
            /// <returns>true if the file header matches the image signature; otherwise, false.</returns>
            private static bool IsMatch(byte[] fileHeader, byte[] imageSignature)
            {
                return !imageSignature.Where((t, i) => fileHeader[i] != t).Any();
            }
        }
    }
}
