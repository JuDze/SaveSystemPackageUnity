using SaveSystem.Versioning;
using SaveSystem.Samples.BasicSaveExample.Data;
using UnityEngine;

namespace SaveSystem.Samples.BasicSaveExample.Migrations
{
    // Migration from save version 1 to version 2.
    // Version 2 introduced a new currency field: gems.
    // Old save files do not contain this field, so we initialize it here.
    public class GameSaveMigration_V1_V2 : ISaveMigration<GameSaveData>
    {
        public int FromVersion => 1;
        public int ToVersion   => 2;

        public GameSaveData Migrate(GameSaveData data)
        {
            // Initialize the new field introduced in version 2
            Debug.Log("Applying migration V1 -> V2 (adding gems field)");
            data.gems = 0;

            return data;
        }
    }
}