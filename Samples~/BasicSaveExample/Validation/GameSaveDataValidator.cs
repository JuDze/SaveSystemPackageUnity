using System.Collections.Generic;
using SaveSystem.Samples.BasicSaveExample.Data;
using SaveSystem.Validation;

namespace SaveSystem.Samples.BasicSaveExample.Validation
{
    // Validates and sanitizes save data after loading or migration.
    // Ensures all fields stay within acceptable bounds.
    public class GameSaveDataValidator : ISaveDataValidator<GameSaveData>
    {
        public GameSaveData Validate(GameSaveData data)
        {
            // Clamp player health to valid range
            if (data.playerHealth < 0)   data.playerHealth = 0;
            if (data.playerHealth > 100) data.playerHealth = 100;

            // Coins cannot be negative
            if (data.coins < 0) data.coins = 0;

            // Gems cannot be negative
            if (data.gems < 0) data.gems = 0;

            // Fall back to starting level if missing
            if (string.IsNullOrWhiteSpace(data.currentLevelId))
                data.currentLevelId = "Level_01";

            // Ensure position exists
            if (data.playerPosition == null)
                data.playerPosition = new Vector3Data(0f, 0f, 0f);

            // Ensure settings exist with defaults
            if (data.settings == null)
                data.settings = new GameSettingsData
                {
                    soundEnabled = true,
                    musicVolume  = 1.0f
                };

            // Clamp volume to valid range
            if (data.settings.musicVolume < 0f) data.settings.musicVolume = 0f;
            if (data.settings.musicVolume > 1f) data.settings.musicVolume = 1f;

            // Ensure inventory list exists
            if (data.inventory == null)
                data.inventory = new List<GameInventoryItemData>();

            return data;
        }
    }
}
