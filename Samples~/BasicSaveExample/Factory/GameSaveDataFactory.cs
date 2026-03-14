using System.Collections.Generic;
using SaveSystem.Samples.BasicSaveExample.Data;

namespace SaveSystem.Samples.BasicSaveExample.Factory
{
    // Creates default save data used when no save file exists.
    public class GameSaveDataFactory
    {
        public GameSaveData CreateDefault()
        {
            return new GameSaveData
            {
                // Current data version of the sample
                saveVersion = 2,

                currentLevelId = "Level_01",
                playerHealth = 100,
                coins = 0,
                gems = 0,

                playerPosition = new Vector3Data(0f, 0f, 0f),

                settings = new GameSettingsData
                {
                    soundEnabled = true,
                    musicVolume  = 1.0f
                },

                inventory = new List<GameInventoryItemData>
                {
                    new GameInventoryItemData("potion", 3)
                }
            };
        }
    }
}