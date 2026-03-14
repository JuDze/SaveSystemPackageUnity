using System;

namespace SaveSystem.Versioning
{
    // Ensures that loaded data matches the current save format version.
    public class VersionManager<TData> where TData : class
    {
        private readonly MigrationManager<TData> migrationManager;
        private readonly Func<TData, int> getVersionFunc;
        private readonly Action<TData, int> setVersionAction;
        private readonly int currentVersion;

        public VersionManager(
            MigrationManager<TData> migrationManager,
            Func<TData, int> getVersionFunc,
            Action<TData, int> setVersionAction,
            int currentVersion)
        {
            this.migrationManager = migrationManager ?? throw new ArgumentNullException(nameof(migrationManager));
            this.getVersionFunc = getVersionFunc ?? throw new ArgumentNullException(nameof(getVersionFunc));
            this.setVersionAction = setVersionAction ?? throw new ArgumentNullException(nameof(setVersionAction));

            if (currentVersion < 0)
                throw new ArgumentOutOfRangeException(nameof(currentVersion), "Current version cannot be negative.");

            this.currentVersion = currentVersion;
        }

        // Ensures that loaded save data is upgraded to the latest supported version.
        public TData EnsureLatestVersion(TData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            int dataVersion = getVersionFunc(data);

            if (dataVersion < 0)
                throw new InvalidOperationException("Loaded save has a negative version.");

            // Already current
            if (dataVersion == currentVersion)
                return data;

            // Save is from a newer game version and cannot be safely loaded.
            if (dataVersion > currentVersion)
            {
                throw new InvalidOperationException(
                    $"Save version {dataVersion} is newer than the supported version {currentVersion}.");
            }

            // Save is older and requires migration.
            return migrationManager.ApplyMigrations(
                data,
                dataVersion,
                currentVersion,
                setVersionAction);
        }
    }
}