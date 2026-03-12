using System;

namespace SaveSystem.Versioning
{
    // Ensures that loaded data matches the current system version.
    // If the data is from an older version, the migration process is triggered.
    public class VersionManager<TData>
    {
        // Migration manager that executes version transitions
        private readonly MigrationManager<TData> migrationManager;

        // Function that reads the version number from the data
        private readonly Func<TData, int> getVersionFunc;

        // Action that updates the version number stored in the data
        private readonly Action<TData, int> setVersionAction;

        // The current (latest) version of the save data format
        private readonly int currentVersion;

        public VersionManager(
            MigrationManager<TData> migrationManager,
            Func<TData, int> getVersionFunc,
            Action<TData, int> setVersionAction,
            int currentVersion)
        {
            this.migrationManager = migrationManager;
            this.getVersionFunc   = getVersionFunc;
            this.setVersionAction = setVersionAction;
            this.currentVersion   = currentVersion;
        }

        // Checks the data version and runs migrations if needed
        // to bring the data up to the latest version
        public TData EnsureLatestVersion(TData data)
        {
            int dataVersion = getVersionFunc(data);

            // If already at the current version, no migration needed
            if (dataVersion == currentVersion)
                return data;

            // Otherwise apply all necessary migrations
            return migrationManager.ApplyMigrations(
                data,
                dataVersion,
                currentVersion,
                setVersionAction);
        }
    }
}
