using SaveSystem.Crypto;
using SaveSystem.Serialization;
using SaveSystem.Storage;
using SaveSystem.Validation;
using SaveSystem.Versioning;
using System;

namespace SaveSystem.Core
{
    /// <summary>
    /// Factory for creating a SaveManager instance in game projects.
    /// Hides the complexity of wiring up all dependencies.
    ///
    /// Usage:
    ///   var manager = SaveManagerFactory.Create(
    ///       new MyGameSaveData(),
    ///       new MyGameValidator(),
    ///       migrations => migrations.RegisterMigration(new MyMigration_V1_V2()),
    ///       data => data.version,
    ///       (data, v) => data.version = v,
    ///       currentVersion: 2
    ///   );
    /// </summary>
    public static class SaveManagerFactory
    {
        /// <summary>
        /// Creates a SaveManager with AES + HMAC encryption (recommended for release builds).
        /// </summary>
        public static SaveManager<TData> Create<TData>(
            TData defaultData,
            ISaveDataValidator<TData> validator,
            Action<MigrationManager<TData>> registerMigrations,
            Func<TData, int> getVersion,
            Action<TData, int> setVersion,
            int currentVersion,
            string masterSecret = "change-this-secret-in-production",
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
                () => defaultData,
                versionManager,
                validator);
        }

        /// <summary>
        /// Creates a SaveManager without encryption — convenient for development and testing.
        /// Data is saved in readable JSON. Do NOT use in release builds!
        /// </summary>
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
                defaultData,
                validator,
                registerMigrations,
                getVersion,
                setVersion,
                currentVersion,
                masterSecret: "dev-only-not-secure",
                fileName: fileName);
        }
    }
}
