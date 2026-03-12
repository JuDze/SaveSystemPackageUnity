using System;
using System.IO;
using UnityEngine;

namespace SaveSystem.Storage
{
    // Implements the file storage service.
    // Handles writing the save file to disk, reading data from it,
    // checking file existence and deleting the save file.
    public class FileStorageService : IStorageService
    {
        private const string LOG_PREFIX = "[SaveSystem] ";

        // Full path to the save file
        private readonly string filePath;

        // Constructor that initializes the save file path.
        // Application.persistentDataPath provides a safe location
        // for storing game data across different operating systems.
        public FileStorageService(string fileName = "save.json")
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name cannot be empty.", nameof(fileName));

            filePath = Path.Combine(Application.persistentDataPath, fileName);
        }

        // Writes text content to the save file
        public void SaveText(string content)
        {
            try
            {
                File.WriteAllText(filePath, content);
                Debug.Log(LOG_PREFIX + "Save file written successfully.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LOG_PREFIX}Error while saving file: {ex.Message}");
                throw;
            }
        }

        // Reads text content from the save file
        public string LoadText()
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Debug.LogWarning(LOG_PREFIX + "Save file does not exist.");
                    return null;
                }

                Debug.Log(LOG_PREFIX + "Loading save file.");
                return File.ReadAllText(filePath);
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LOG_PREFIX}Error while reading file: {ex.Message}");
                throw;
            }
        }

        // Returns true if the save file exists on disk
        public bool Exists() => File.Exists(filePath);

        // Deletes the save file from disk
        public void Delete()
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Debug.Log(LOG_PREFIX + "Save file deleted.");
                }
                else
                {
                    Debug.LogWarning(LOG_PREFIX + "Delete requested but save file not found.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LOG_PREFIX}Error while deleting file: {ex.Message}");
                throw;
            }
        }

        // Returns the full path to the save file
        public string GetPath() => filePath;
    }
}
