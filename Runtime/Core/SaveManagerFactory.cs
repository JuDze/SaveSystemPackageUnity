using SaveSystem.Crypto;
using SaveSystem.Serialization;
using SaveSystem.Storage;
using SaveSystem.Validation;
using SaveSystem.Versioning;
using System;

namespace SaveSystem.Core
{

    //Factory for creating a SaveManager instance in game projects.
    public static class SaveManagerFactory
    {
        
        // Creates a SaveManager with AES + HMAC encryption (recommended for release builds).
        public static SaveManager<TData> Create<TData>(
            Func<TData> createDefaultData,
            ISaveDataValidator<TData> validator,
            Action<MigrationManager<TData>> registerMigrations,
            Func<TData, int> getVersion,
            Action<TData, int> setVersion,
            int currentVersion,
            string masterSecret = "create-your-own-master-secret",
            string fileName = "save.json") where TData : class
        {
            var saveSerializer     = new JsonSaveSerializer<TData>();
            var envelopeSerializer = new JsonSaveEnvelopeSerializer();
            var cryptoService      = new AesCryptoService();
            var checksumService    = new ChecksumService();
            var keyProvider        = new StaticKeyProvider(masterSecret);
            var storage            = new FileStorageService(fileName);

            var migrationManager = new MigrationManager<TData>();
            registerMigrations?.Invoke(migrationManager);

            var versionManager = new VersionManager<TData>(
                migrationManager,
                getVersion,
                setVersion,
                currentVersion);

            return new SaveManager<TData>(
                saveSerializer,
                envelopeSerializer,
                cryptoService,
                checksumService,
                keyProvider,
                storage,
                createDefaultData,
                versionManager,
                validator);
        }


        // For testing creates a SaveManager without encryption

        public static SaveManager<TData> CreateUnencrypted<TData>(
            TData defaultData,
            ISaveDataValidator<TData> validator,
            Action<MigrationManager<TData>> registerMigrations,
            Func<TData, int> getVersion,
            Action<TData, int> setVersion,
            int currentVersion,
            string fileName = "save_dev.json") where TData : class
        {
            return Create(
                createDefaultData: () => defaultData,
                validator,
                registerMigrations,
                getVersion,
                setVersion,
                currentVersion,
                masterSecret: "create-your-own-master-secret",
                fileName: fileName);
        }
    }
}
