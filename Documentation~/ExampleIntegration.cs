// =============================================================================
//  SAVE SYSTEM INTEGRATION EXAMPLE
//
//  This file demonstrates how to integrate the SaveSystem package
//  into a Unity game project.
//
//  IMPORTANT:
//  This file should be placed inside your GAME project (Assets/),
//  NOT inside the SaveSystem package itself.
//
//  Integration steps shown in this example:
//      1. Define a save data model
//      2. Implement a validator for loaded data
//      3. Define optional save migrations
//      4. Create the SaveManager during game startup
//      5. Use the SaveManager from gameplay scripts
//
//  NOTE:
//  This is example code provided for integration reference.
//  Modify it according to your project's needs.
// =============================================================================

using System;
using UnityEngine;
using SaveSystem.Core;
using SaveSystem.Validation;
using SaveSystem.Versioning;

// -----------------------------------------------------------------------
// 1. SAVE DATA MODEL
//
// Defines what game data will be stored in the save file.
// The 'version' field is required for migration support.
// -----------------------------------------------------------------------
[Serializable]
public class MyGameData
{
    public int    version      = 1;   // required — used for save migrations
    public string playerName   = "Hero";
    public int    level        = 1;
    public int    score        = 0;
    public float  playTime     = 0f;
    public bool   tutorialDone = false;
}

// -----------------------------------------------------------------------
// 2. SAVE DATA VALIDATOR
//
// Runs after loading save data.
// Ensures values are valid and fixes corrupted or invalid data.
// -----------------------------------------------------------------------
public class MyGameDataValidator : ISaveDataValidator<MyGameData>
{
    public MyGameData Validate(MyGameData data)
    {
        // Protect against invalid values after loading or migration
        if (data.level < 1) data.level = 1;
        if (data.score < 0) data.score = 0;
        if (data.playTime < 0) data.playTime = 0f;

        if (string.IsNullOrWhiteSpace(data.playerName))
            data.playerName = "Hero";

        return data;
    }
}

// -----------------------------------------------------------------------
// 3. SAVE MIGRATION EXAMPLE
//
// Demonstrates how to upgrade save data from version 1 to version 2.
// Migrations allow old save files to remain compatible with newer
// versions of the game.
// -----------------------------------------------------------------------
public class MyGameMigration_V1_V2 : ISaveMigration<MyGameData>
{
    public int FromVersion => 1;
    public int ToVersion   => 2;

    public MyGameData Migrate(MyGameData old)
    {
        // Older saves did not contain tutorialDone — set it to true
        // (the player has already played before, no need to show tutorial again)
        old.tutorialDone = true;
        return old;
    }
}

// -----------------------------------------------------------------------
// 4. SAVE SYSTEM BOOTSTRAPPER
//
// Initializes the SaveManager once when the game starts.
// The instance is stored statically so it can be accessed globally.
// -----------------------------------------------------------------------
public class GameBootstrapper : MonoBehaviour
{
    // Global access from anywhere in the game
    public static SaveManager<MyGameData> SaveManager { get; private set; }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        SaveManager = SaveManagerFactory.Create(
            defaultData:        new MyGameData(),
            validator:          new MyGameDataValidator(),
            registerMigrations: migrations =>
            {
                migrations.RegisterMigration(new MyGameMigration_V1_V2());
                // migrations.RegisterMigration(new MyGameMigration_V2_V3());
            },
            getVersion:     data => data.version,
            setVersion:     (data, v) => data.version = v,
            currentVersion: 2,
            masterSecret:   "my-game-super-secret-2025",  // change to your own!
            fileName:       "my_game_save.json"
        );

        Debug.Log("[Game] Save system ready. Path: " + SaveManager.GetSavePath());
    }
}

// -----------------------------------------------------------------------
// 5. GAMEPLAY USAGE EXAMPLE
//
// Shows how game systems can load, update and save player progress.
// -----------------------------------------------------------------------
public class GameController : MonoBehaviour
{
    private MyGameData _data;

    private void Start()
    {
        // Load save data on game start
        _data = GameBootstrapper.SaveManager.Load();
        Debug.Log($"Loaded: {_data.playerName}, Level {_data.level}, Score {_data.score}");
    }

    public void OnLevelComplete(int newLevel, int earnedScore)
    {
        _data.level     = newLevel;
        _data.score    += earnedScore;
        _data.playTime += Time.realtimeSinceStartup / 3600f;

        GameBootstrapper.SaveManager.Save(_data);
        Debug.Log("Game saved!");
    }

    public void OnResetProgress()
    {
        GameBootstrapper.SaveManager.DeleteSave();
        _data = new MyGameData();
        Debug.Log("Progress reset.");
    }
}