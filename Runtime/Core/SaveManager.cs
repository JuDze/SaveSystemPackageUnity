using System;
using System.Text;
using SaveSystem.Crypto;
using SaveSystem.Models;
using SaveSystem.Serialization;
using SaveSystem.Storage;
using SaveSystem.Validation;
using SaveSystem.Versioning;
using UnityEngine;

namespace SaveSystem.Core
{
    // Main save system manager.
    // Responsible for data serialization, encryption, integrity checking,
    // version migration, validation and file storage.
    public class SaveManager<TData> where TData: class
    {
        private const string LOG_PREFIX = "[SaveSystem] ";

        // Service for game data serialization
        private readonly ISaveSerializer<TData> saveSerializer;

        // Service for save envelope serialization
        private readonly ISaveEnvelopeSerializer envelopeSerializer;

        // Cryptographic service for data encryption
        private readonly ICryptoService cryptoService;

        // Service for MAC computation (integrity check)
        private readonly ChecksumService checksumService;

        // Cryptographic key provider
        private readonly IKeyProvider keyProvider;

        // File storage service
        private readonly IStorageService storage;

        // Function for creating default data
        private readonly Func<TData> createDefaultDataFunc;

        // Version manager for data migration
        private readonly VersionManager<TData> versionManager;

        // Data validation service
        private readonly ISaveDataValidator<TData> validator;

        public SaveManager(
            ISaveSerializer<TData> saveSerializer,
            ISaveEnvelopeSerializer envelopeSerializer,
            ICryptoService cryptoService,
            ChecksumService checksumService,
            IKeyProvider keyProvider,
            IStorageService storage,
            Func<TData> createDefaultDataFunc,
            VersionManager<TData> versionManager,
            ISaveDataValidator<TData> validator)
        {
            this.saveSerializer = saveSerializer ?? throw new ArgumentNullException(nameof(saveSerializer));
            this.envelopeSerializer = envelopeSerializer ?? throw new ArgumentNullException(nameof(envelopeSerializer));
            this.cryptoService = cryptoService ?? throw new ArgumentNullException(nameof(cryptoService));
            this.checksumService = checksumService ?? throw new ArgumentNullException(nameof(checksumService));
            this.keyProvider = keyProvider ?? throw new ArgumentNullException(nameof(keyProvider));
            this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
            this.createDefaultDataFunc = createDefaultDataFunc ?? throw new ArgumentNullException(nameof(createDefaultDataFunc));
            this.versionManager = versionManager ?? throw new ArgumentNullException(nameof(versionManager));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        // Saves data to file with encryption and MAC protection
        public void Save(TData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Save data cannot be null.");

            try
            {
                // Serialize data to JSON
                string saveJson = saveSerializer.Serialize(data);
                byte[] plainBytes = Encoding.UTF8.GetBytes(saveJson);

                // Get encryption and MAC keys
                byte[] encryptionKey = keyProvider.GetEncryptionKey();
                byte[] macKey = keyProvider.GetMacKey();
                byte[] iv = cryptoService.GenerateIv();

                // Encrypt data
                byte[] cipherBytes = cryptoService.Encrypt(plainBytes, encryptionKey, iv);

                // Compute MAC for integrity verification
                byte[] macData = CryptoUtilities.Combine(iv, cipherBytes);
                byte[] mac = checksumService.ComputeMac(macData, macKey);

                // Build save envelope
                SaveEnvelope envelope = new SaveEnvelope
                {
                    formatVersion    = 1,
                    algorithm        = "AES-CBC-HMACSHA256",
                    ivBase64         = Convert.ToBase64String(iv),
                    cipherTextBase64 = Convert.ToBase64String(cipherBytes),
                    macBase64        = Convert.ToBase64String(mac)
                };

                // Serialize envelope and write to file
                string envelopeJson = envelopeSerializer.Serialize(envelope);
                storage.SaveText(envelopeJson);

                Debug.Log($"{LOG_PREFIX}Save completed successfully. Path: {storage.GetPath()}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LOG_PREFIX}Error while saving data: {ex.GetType().Name}: {ex.Message}");
                throw;
            }
        }

        // Loads saved data from file
        // Loads save data with fallback and recovery logic
        public TData Load()
        {
            try
            {
                // If no save file exists, return default data
                if (!storage.Exists())
                {
                    Debug.LogWarning($"{LOG_PREFIX}Save file not found. Using default data.");
                    return createDefaultDataFunc();
                }

                try
                {
                    // Attempt to load the main save file
                    string mainSaveText = storage.LoadText();
                    TData mainData = TryLoadFromText(mainSaveText, "main");

                    // If main save is valid, return it
                    if (mainData != null)
                        return mainData;
                }
                catch (Exception ex)
                {
                    // Log failure but continue with backup recovery
                    Debug.LogError($"{LOG_PREFIX}Main save load failed: {ex.GetType().Name}: {ex.Message}");
                }

                // If main save failed, try loading backup
                if (storage.BackupExists())
                {
                    try
                    {
                        // Attempt to load backup save file
                        string backupSaveText = storage.LoadBackupText();
                        TData backupData = TryLoadFromText(backupSaveText, "backup");

                        // If backup is valid, use it
                        if (backupData != null)
                        {
                            Debug.LogWarning($"{LOG_PREFIX}Main save is invalid. Backup save loaded successfully.");
                            return backupData;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log backup failure and continue to default data
                        Debug.LogError($"{LOG_PREFIX}Backup save load failed: {ex.GetType().Name}: {ex.Message}");
                    }
                }

                // If both main and backup failed, return default data
                Debug.LogWarning($"{LOG_PREFIX}No valid save could be loaded. Using default data.");
                return createDefaultDataFunc();
            }
            catch (Exception ex)
            {
                // Catch unexpected errors to prevent game crash
                Debug.LogError($"{LOG_PREFIX}Unexpected error while loading data: {ex.GetType().Name}: {ex.Message}");
                return createDefaultDataFunc();
            }
        }

        // Deletes the save file
        public void DeleteSave()
        {
            try
            {
                storage.Delete();
                Debug.Log($"{LOG_PREFIX}Save file deleted.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LOG_PREFIX}Error while deleting save file: {ex.GetType().Name}: {ex.Message}");
                throw;
            }
        }

        // Returns true if a save file exists
        public bool HasSave()
        {
            return storage.Exists();
        }

        // Returns the full path to the save file
        public string GetSavePath()
        {
            return storage.GetPath();
        }

        // Validates that the save envelope contains all required fields
        private void ValidateEnvelope(SaveEnvelope envelope)
        {
            if (envelope == null)
                throw new ArgumentNullException(nameof(envelope));

            if (envelope.formatVersion != 1)
                throw new InvalidOperationException(
                    $"Unsupported save envelope format version: {envelope.formatVersion}");

            if (envelope.algorithm != "AES-CBC-HMACSHA256")
                throw new InvalidOperationException(
                    $"Unsupported save envelope algorithm: {envelope.algorithm}");

            if (string.IsNullOrWhiteSpace(envelope.ivBase64))
                throw new InvalidOperationException("Save envelope IV is missing.");

            if (string.IsNullOrWhiteSpace(envelope.cipherTextBase64))
                throw new InvalidOperationException("Save envelope ciphertext is missing.");

            if (string.IsNullOrWhiteSpace(envelope.macBase64))
                throw new InvalidOperationException("Save envelope MAC is missing.");
        }

        private TData TryLoadFromText(string envelopeJson, string sourceLabel)
        {
            // Check if the loaded text is empty or contains only whitespace.
            // This may happen if the save file exists but contains no valid data.
            if (string.IsNullOrWhiteSpace(envelopeJson))
            {
                Debug.LogWarning($"{LOG_PREFIX}{sourceLabel} save file is empty.");
                return null;
            }

            // Deserialize the encrypted save envelope (metadata + encrypted payload).
            SaveEnvelope envelope = envelopeSerializer.Deserialize(envelopeJson);

            // If the envelope cannot be parsed, the save file is considered invalid.
            if (envelope == null)
            {
                Debug.LogWarning($"{LOG_PREFIX}Failed to deserialize {sourceLabel} save envelope.");
                return null;
            }

            // Validate envelope structure and required fields.
            // This ensures the save format and algorithm fields are correct.
            ValidateEnvelope(envelope);

            // Decode Base64 encoded components from the envelope.
            byte[] iv = Convert.FromBase64String(envelope.ivBase64);
            byte[] cipherBytes = Convert.FromBase64String(envelope.cipherTextBase64);
            byte[] expectedMac = Convert.FromBase64String(envelope.macBase64);

            // Recreate the original MAC input (IV + ciphertext).
            // This must match the data used during save.
            byte[] macData = CryptoUtilities.Combine(iv, cipherBytes);

            // Retrieve the MAC verification key.
            byte[] macKey = keyProvider.GetMacKey();

            // Verify integrity using HMAC-SHA256.
            // If verification fails, the save may be corrupted or tampered with.
            bool isValidMac = checksumService.VerifyMac(macData, macKey, expectedMac);

            if (!isValidMac)
            {
                Debug.LogError($"{LOG_PREFIX}{sourceLabel} save integrity check failed.");
                return null;
            }

            // Retrieve the encryption key used to decrypt the save payload.
            byte[] encryptionKey = keyProvider.GetEncryptionKey();

            // Decrypt the ciphertext using AES.
            byte[] plainBytes = cryptoService.Decrypt(cipherBytes, encryptionKey, iv);

            // Convert decrypted bytes back to JSON string.
            string saveJson = Encoding.UTF8.GetString(plainBytes);

            // Deserialize the actual game save data.
            TData data = saveSerializer.Deserialize(saveJson);

            // If the payload cannot be parsed, the save is considered invalid.
            if (data == null)
            {
                Debug.LogWarning($"{LOG_PREFIX}Failed to deserialize {sourceLabel} game data.");
                return null;
            }

            // Apply version migration pipeline if the save format is outdated.
            data = versionManager.EnsureLatestVersion(data);

            // Validate and sanitize loaded data to ensure it is within valid bounds.
            data = validator.Validate(data);

            // Return the fully validated and migrated save data.
            return data;
        }
    }
}
