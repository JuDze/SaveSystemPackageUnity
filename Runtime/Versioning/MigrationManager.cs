using System.Collections.Generic;

namespace SaveSystem.Versioning
{
    // Manages data migrations between different save versions.
    public class MigrationManager<TData>
    {
        // List of registered migration steps
        private readonly List<ISaveMigration<TData>> migrations = new();

        // Registers a new migration step
        public void RegisterMigration(ISaveMigration<TData> migration)
        {
            migrations.Add(migration);
        }

        // Applies all necessary migrations from the current version
        // up to the target version
        public TData ApplyMigrations(
            TData data,
            int currentDataVersion,
            int targetVersion,
            System.Action<TData, int> updateVersionAction)
        {
            int version = currentDataVersion;

            // Keep migrating until we reach the target version
            while (version < targetVersion)
            {
                // Find the migration step for the current version
                ISaveMigration<TData> migration = migrations.Find(m => m.FromVersion == version);

                // If no migration is found for this step, stop the process
                if (migration == null)
                    break;

                // Apply the migration
                data = migration.Migrate(data);

                // Advance to the next version
                version = migration.ToVersion;

                // Update the version number stored in the data itself
                updateVersionAction?.Invoke(data, version);
            }

            return data;
        }
    }
}
