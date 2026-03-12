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
    public class SaveManager<TData>
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
                Debug.LogError($"{LOG_PREFIX}Error while saving data: {ex.Message}");
                throw;
            }
        }

        // Loads saved data from file
        public TData Load()
        {
            try
            {
                if (!storage.Exists())
                {
                    Debug.LogWarning($"{LOG_PREFIX}Save file not found. Using default data.");
                    return createDefaultDataFunc();
                }

                string envelopeJson = storage.LoadText();

                if (string.IsNullOrWhiteSpace(envelopeJson))
                {
                    Debug.LogWarning($"{LOG_PREFIX}Save file is empty. Using default data.");
                    return createDefaultDataFunc();
                }

                // Deserialize save envelope
                SaveEnvelope envelope = envelopeSerializer.Deserialize(envelopeJson);

                if (envelope == null)
                {
                    Debug.LogWarning($"{LOG_PREFIX}Failed to deserialize save envelope. Using default data.");
                    return createDefaultDataFunc();
                }

                ValidateEnvelope(envelope);

                // Decode Base64 fields
                byte[] iv            = Convert.FromBase64String(envelope.ivBase64);
                byte[] cipherBytes   = Convert.FromBase64String(envelope.cipherTextBase64);
                byte[] expectedMac   = Convert.FromBase64String(envelope.macBase64);

                // Verify data integrity
                byte[] macData = CryptoUtilities.Combine(iv, cipherBytes);
                byte[] macKey  = keyProvider.GetMacKey();

                bool isValidMac = checksumService.VerifyMac(macData, macKey, expectedMac);

                if (!isValidMac)
                {
                    Debug.LogError($"{LOG_PREFIX}Save integrity check failed. Using default data.");
                    return createDefaultDataFunc();
                }

                // Decrypt data
                byte[] encryptionKey = keyProvider.GetEncryptionKey();
                byte[] plainBytes    = cryptoService.Decrypt(cipherBytes, encryptionKey, iv);
                string saveJson      = Encoding.UTF8.GetString(plainBytes);

                // Deserialize game data
                TData data = saveSerializer.Deserialize(saveJson);

                if (data == null)
                {
                    Debug.LogWarning($"{LOG_PREFIX}Failed to deserialize game data. Using default data.");
                    return createDefaultDataFunc();
                }

                // Apply version migration and validation
                data = versionManager.EnsureLatestVersion(data);
                data = validator.Validate(data);

                return data;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LOG_PREFIX}Error while loading data: {ex.Message}");
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
                Debug.LogError($"{LOG_PREFIX}Error while deleting save file: {ex.Message}");
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
            if (string.IsNullOrWhiteSpace(envelope.algorithm))
                throw new InvalidOperationException("Save envelope algorithm is missing.");

            if (string.IsNullOrWhiteSpace(envelope.ivBase64))
                throw new InvalidOperationException("Save envelope IV is missing.");

            if (string.IsNullOrWhiteSpace(envelope.cipherTextBase64))
                throw new InvalidOperationException("Save envelope ciphertext is missing.");

            if (string.IsNullOrWhiteSpace(envelope.macBase64))
                throw new InvalidOperationException("Save envelope MAC is missing.");
        }
    }
}
