using System.Collections.Generic;
using UnityEngine;

using SaveSystem.Core;
using SaveSystem.Crypto;
using SaveSystem.Serialization;
using SaveSystem.Storage;
using SaveSystem.Versioning;

using SaveSystem.Samples.BasicSaveExample.Data;
using SaveSystem.Samples.BasicSaveExample.Factory;
using SaveSystem.Samples.BasicSaveExample.Validation;
using SaveSystem.Samples.BasicSaveExample.Migrations;

namespace SaveSystem.Samples.BasicSaveExample
{
    // Example MonoBehaviour demonstrating how to initialize and use the save system.
    // This script creates the SaveManager and exposes simple actions for the sample scene.
    public class SaveSystemSampleController : MonoBehaviour
    {
        [SerializeField]
        private string saveFileName = "basic_save_example.json";

        private SaveManager<GameSaveData> saveManager;

        private void Awake()
        {
            saveManager = CreateSaveManager();

            Debug.Log("Save system initialized.");
            Debug.Log("Save file name: " + saveFileName);
            Debug.Log("Save path: " + saveManager.GetSavePath());
        }

        private SaveManager<GameSaveData> CreateSaveManager()
        {
            // Serializer for the actual game data model
            var serializer = new JsonSaveSerializer<GameSaveData>();

            // Serializer for the encrypted save envelope
            var envelopeSerializer = new JsonSaveEnvelopeSerializer();

            // Encryption service (AES-CBC)
            var cryptoService = new AesCryptoService();

            // Integrity verification service (HMAC-SHA256)
            var checksumService = new ChecksumService();

            // Key provider derived from a master secret
            var keyProvider = new StaticKeyProvider("MyGameSecretKey");

            // File storage service (writes to Application.persistentDataPath)
            var storage = new FileStorageService(saveFileName);

            // Factory used to create default save data
            var factory = new GameSaveDataFactory();

            // Migration manager that stores all version migration steps
            var migrationManager = new MigrationManager<GameSaveData>();

            // Register example migration from version 1 to version 2
            migrationManager.RegisterMigration(new GameSaveMigration_V1_V2());

            // Version manager controls migration pipeline
            var versionManager = new VersionManager<GameSaveData>(
                migrationManager,
                data => data.saveVersion,
                (data, version) => data.saveVersion = version,
                2
            );

            // Data validator that sanitizes loaded data
            var validator = new GameSaveDataValidator();

            // Create the main SaveManager instance
            return new SaveManager<GameSaveData>(
                serializer,
                envelopeSerializer,
                cryptoService,
                checksumService,
                keyProvider,
                storage,
                factory.CreateDefault,
                versionManager,
                validator
            );
        }

        private bool EnsureInitialized()
        {
            if (saveManager != null)
                return true;

            Debug.LogError("Save system is not initialized.");
            return false;
        }

        [ContextMenu("Save Sample Data")]
        public void SaveSampleData()
        {
            if (!EnsureInitialized())
                return;

            // Example save data used for testing
            GameSaveData testData = new GameSaveData
            {
                saveVersion = 2,
                currentLevelId = "Level_02",
                playerHealth = 75,
                coins = 150,
                gems = 5,

                playerPosition = new Vector3Data(3.5f, 1.0f, -2.0f),

                settings = new GameSettingsData
                {
                    soundEnabled = true,
                    musicVolume = 0.7f
                },

                inventory = new List<GameInventoryItemData>
                {
                    new GameInventoryItemData("sword_basic", 1),
                    new GameInventoryItemData("potion_small", 3)
                }
            };

            saveManager.Save(testData);

            Debug.Log("Sample data saved successfully.");
        }

        [ContextMenu("Load Sample Data")]
        public void LoadSampleData()
        {
            if (!EnsureInitialized())
                return;

            GameSaveData loadedData = saveManager.Load();

            Debug.Log("=== Loaded save data ===");
            Debug.Log("Version: " + loadedData.saveVersion);
            Debug.Log("Level: " + loadedData.currentLevelId);
            Debug.Log("Health: " + loadedData.playerHealth);
            Debug.Log("Coins: " + loadedData.coins);
            Debug.Log("Gems: " + loadedData.gems);

            if (loadedData.playerPosition != null)
            {
                Debug.Log(
                    "Position: " +
                    loadedData.playerPosition.x + ", " +
                    loadedData.playerPosition.y + ", " +
                    loadedData.playerPosition.z);
            }
            else
            {
                Debug.Log("Position: null");
            }

            if (loadedData.inventory != null)
            {
                Debug.Log("Inventory count: " + loadedData.inventory.Count);

                for (int i = 0; i < loadedData.inventory.Count; i++)
                {
                    GameInventoryItemData item = loadedData.inventory[i];
                    Debug.Log("Item " + i + ": " + item.itemId + " x" + item.amount);
                }
            }
            else
            {
                Debug.Log("Inventory: null");
            }

            if (loadedData.settings != null)
            {
                Debug.Log("Sound enabled: " + loadedData.settings.soundEnabled);
                Debug.Log("Music volume: " + loadedData.settings.musicVolume);
            }
            else
            {
                Debug.Log("Settings: null");
            }
        }

        [ContextMenu("Delete Save")]
        public void DeleteSave()
        {
            if (!EnsureInitialized())
                return;

            saveManager.DeleteSave();
            Debug.Log("Sample save file deleted.");
        }

        [ContextMenu("Check Save Exists")]
        public void CheckSaveExists()
        {
            if (!EnsureInitialized())
                return;

            Debug.Log("Save exists: " + saveManager.HasSave());
        }

        [ContextMenu("Create Legacy V1 Save")]
        public void CreateLegacyV1Save()
        {
            if (!EnsureInitialized())
                return;

            // This simulates an old save file created before version 2 existed.
            GameSaveData legacyData = new GameSaveData
            {
                saveVersion = 1,

                currentLevelId = "Level_01",
                playerHealth = 80,
                coins = 25,
                gems = 0,

                playerPosition = new Vector3Data(0f, 0f, 0f),

                settings = new GameSettingsData
                {
                    soundEnabled = true,
                    musicVolume = 0.5f
                },

                inventory = new List<GameInventoryItemData>
                {
                    new GameInventoryItemData("potion_small", 2)
                }
            };

            saveManager.Save(legacyData);

            Debug.Log("Legacy V1 save created with version = 1. Load it to trigger migration to version 2.");
        }
    }
}