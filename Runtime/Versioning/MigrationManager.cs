using System;
using System.Collections.Generic;

namespace SaveSystem.Versioning
{
    // Manages registered migration steps and applies them in order.
    public class MigrationManager<TData> where TData : class
    {
        // Store migrations by starting version to prevent duplicates
        // and allow fast lookup.
        private readonly Dictionary<int, ISaveMigration<TData>> migrations = new();

        // Registers a migration step.
        // Throws if the migration is invalid or conflicts with an existing one.
        public void RegisterMigration(ISaveMigration<TData> migration)
        {
            if (migration == null)
                throw new ArgumentNullException(nameof(migration));

            if (migration.ToVersion <= migration.FromVersion)
            {
                throw new InvalidOperationException(
                    $"Invalid migration {migration.GetType().Name}: " +
                    $"ToVersion ({migration.ToVersion}) must be greater than FromVersion ({migration.FromVersion}).");
            }

            if (migrations.ContainsKey(migration.FromVersion))
            {
                throw new InvalidOperationException(
                    $"A migration starting from version {migration.FromVersion} is already registered.");
            }

            migrations.Add(migration.FromVersion, migration);
        }

        // Applies all required migrations from the current data version
        // up to the target version.
        public TData ApplyMigrations(
            TData data,
            int currentDataVersion,
            int targetVersion,
            Action<TData, int> updateVersionAction)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (updateVersionAction == null)
                throw new ArgumentNullException(nameof(updateVersionAction));

            if (currentDataVersion < 0)
                throw new ArgumentOutOfRangeException(nameof(currentDataVersion), "Save version cannot be negative.");

            if (targetVersion < 0)
                throw new ArgumentOutOfRangeException(nameof(targetVersion), "Target version cannot be negative.");

            if (currentDataVersion > targetVersion)
            {
                throw new InvalidOperationException(
                    $"Cannot migrate backwards from version {currentDataVersion} to {targetVersion}.");
            }

            int version = currentDataVersion;

            // Apply migrations step by step until the target version is reached.
            while (version < targetVersion)
            {
                if (!migrations.TryGetValue(version, out ISaveMigration<TData> migration))
                {
                    throw new InvalidOperationException(
                        $"Missing migration step from version {version} to the next version. " +
                        $"Cannot reach target version {targetVersion}.");
                }

                data = migration.Migrate(data);

                if (data == null)
                {
                    throw new InvalidOperationException(
                        $"Migration {migration.GetType().Name} returned null.");
                }

                version = migration.ToVersion;
                updateVersionAction(data, version);
            }

            return data;
        }
    }
}