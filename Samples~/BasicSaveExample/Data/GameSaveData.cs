using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaveSystem.Samples.BasicSaveExample.Data
{
    // Example save data model used in the Basic Save Example sample.
    // All fields are serialized to JSON and encrypted before being written to disk.
    [Serializable]
    public class GameSaveData
    {
        // Save data version used by the migration system
        public int saveVersion = 2;

        // Current level identifier
        public string currentLevelId = "Level_01";

        // Player health value
        public int playerHealth = 100;

        // Example currency
        public int coins = 0;

        public int gems;

        // Player world position
        public Vector3Data playerPosition;

        // Player settings
        public GameSettingsData settings;

        // Player inventory
        public List<GameInventoryItemData> inventory = new();
    }

    // Serializable wrapper for Unity's Vector3.
    // Unity JsonUtility does not reliably support nested Vector3 in custom data models,
    // so we convert it into a simple serializable structure.
    [Serializable]
    public class Vector3Data
    {
        public float x;
        public float y;
        public float z;

        public Vector3Data() { }

        public Vector3Data(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        // Convert to Unity Vector3
        public Vector3 ToVector3() => new Vector3(x, y, z);

        // Convert from Unity Vector3
        public static Vector3Data FromVector3(Vector3 v)
        {
            return new Vector3Data(v.x, v.y, v.z);
        }
    }

    // Player settings saved between sessions
    [Serializable]
    public class GameSettingsData
    {
        public bool soundEnabled = true;
        public float musicVolume = 1.0f;
    }

    // Represents a single item in the player's inventory
    [Serializable]
    public class GameInventoryItemData
    {
        public string itemId;
        public int amount;

        public GameInventoryItemData() { }

        public GameInventoryItemData(string itemId, int amount)
        {
            this.itemId = itemId;
            this.amount = amount;
        }
    }
}