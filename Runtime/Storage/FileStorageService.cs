using System;
using System.IO;
using UnityEngine;

namespace SaveSystem.Storage
{
    // Handles saving and loading text files for the save system
    public class FileStorageService : IStorageService
    {
        private const string LOG_PREFIX = "[SaveSystem] ";

        // Main save file path
        private readonly string filePath;

        // Temporary file used for safe writing
        private readonly string tempFilePath;

        // Backup file created during file replacement
        private readonly string backupFilePath;

        public FileStorageService(string fileName = "save.json")
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name cannot be empty.", nameof(fileName));

            // Build file paths inside Unity persistent data folder
            filePath = Path.Combine(Application.persistentDataPath, fileName);
            tempFilePath = filePath + ".tmp";
            backupFilePath = filePath + ".bak";
        }

        // Writes save content using atomic file replacement
        public void SaveText(string content)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            try
            {
                // Ensure directory exists
                string directory = Path.GetDirectoryName(filePath);

                if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                // Remove leftover temp file if present
                if (File.Exists(tempFilePath))
                    File.Delete(tempFilePath);

                // Write new content to temporary file first
                File.WriteAllText(tempFilePath, content);

                // Replace main file atomically and create backup
                if (File.Exists(filePath))
                    File.Replace(tempFilePath, filePath, backupFilePath, true);
                else
                    File.Move(tempFilePath, filePath);

                Debug.Log($"{LOG_PREFIX}Save file written safely. Path: {filePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LOG_PREFIX}Error while saving file: {ex.GetType().Name}: {ex.Message}");

                // Cleanup temp file if something failed
                try
                {
                    if (File.Exists(tempFilePath))
                        File.Delete(tempFilePath);
                }
                catch
                {
                }

                throw;
            }
        }

        // Loads text from the main save file
        public string LoadText()
        {
            try
            {
                // If file does not exist return null
                if (!File.Exists(filePath))
                {
                    Debug.LogWarning($"{LOG_PREFIX}Main save file does not exist.");
                    return null;
                }

                Debug.Log($"{LOG_PREFIX}Loading main save file.");
                return File.ReadAllText(filePath);
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LOG_PREFIX}Error while reading main save file: {ex.GetType().Name}: {ex.Message}");
                throw;
            }
        }

        // Loads text from backup save file
        public string LoadBackupText()
        {
            try
            {
                // If backup file does not exist return null
                if (!File.Exists(backupFilePath))
                {
                    Debug.LogWarning($"{LOG_PREFIX}Backup save file does not exist.");
                    return null;
                }

                Debug.Log($"{LOG_PREFIX}Loading backup save file.");
                return File.ReadAllText(backupFilePath);
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LOG_PREFIX}Error while reading backup save file: {ex.GetType().Name}: {ex.Message}");
                throw;
            }
        }

        // Checks if main save file exists
        public bool Exists() => File.Exists(filePath);

        // Checks if backup save file exists
        public bool BackupExists() => File.Exists(backupFilePath);

        // Deletes all save-related files
        public void Delete()
        {
            try
            {
                bool deletedAny = false;

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    deletedAny = true;
                }

                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                    deletedAny = true;
                }

                if (File.Exists(backupFilePath))
                {
                    File.Delete(backupFilePath);
                    deletedAny = true;
                }

                if (deletedAny)
                    Debug.Log($"{LOG_PREFIX}Save file(s) deleted.");
                else
                    Debug.LogWarning($"{LOG_PREFIX}Delete requested but no save files were found.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LOG_PREFIX}Error while deleting file: {ex.GetType().Name}: {ex.Message}");
                throw;
            }
        }

        // Returns the main save file path
        public string GetPath() => filePath;
    }
}