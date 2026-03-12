# Modular Save System — Unity Package

![Unity](https://img.shields.io/badge/Unity-2022.3+-black?logo=unity)
![License](https://img.shields.io/badge/license-MIT-green)
![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20macOS%20%7C%20Linux-lightgrey)

A modular save system for Unity featuring AES encryption, HMAC-SHA256 integrity verification, JSON serialization, and versioned save data migration.

The system is designed to provide secure, extensible, and production-ready save functionality for Unity projects.

---

## Features

- AES-CBC encryption for save files
- HMAC-SHA256 integrity verification
- JSON serialization
- Generic architecture using SaveManager<TData>
- Versioned save data migrations
- Save data validation layer
- Modular architecture
- Unity Package Manager compatible
- Runtime tests included

---

## Package Structure

    Runtime
     ├── Core
     │   ├── SaveManager.cs
     │   └── SaveManagerFactory.cs
     │
     ├── Crypto
     │   ├── ICryptoService.cs
     │   ├── AesCryptoService.cs
     │   ├── ChecksumService.cs
     │   ├── CryptoUtilities.cs
     │   ├── IKeyProvider.cs
     │   └── StaticKeyProvider.cs
     │
     ├── Models
     │   └── SaveEnvelope.cs
     │
     ├── Serialization
     │   ├── ISaveSerializer.cs
     │   ├── ISaveEnvelopeSerializer.cs
     │   ├── JsonSaveSerializer.cs
     │   └── JsonSaveEnvelopeSerializer.cs
     │
     ├── Storage
     │   ├── IStorageService.cs
     │   └── FileStorageService.cs
     │
     ├── Validation
     │   └── ISaveDataValidator.cs
     │
     └── Versioning
         ├── ISaveMigration.cs
         ├── MigrationManager.cs
         └── VersionManager.cs

    Editor
     └── SaveSystemEditorWindow.cs

    Tests
     └── Runtime
         └── SaveSystemTests.cs

    Documentation~
     └── ExampleIntegration.cs

---

## Installation

### Install via Git URL

Open Unity Package Manager:

    Window → Package Manager

Click:

    + → Add package from git URL

Insert:

    https://github.com/JuDze/SaveSystemPacakgeUnity.git

Unity will automatically install the package.

---

### Local Installation

Edit:

    Packages/manifest.json

Add:

    {
      "dependencies": {
        "com.judze.savesystem.runtime": "file:../../SaveSystemPacakgeUnity"
      }
    }

Adjust the path according to your local folder structure.

---

## Quick Start

### 1. Define Save Data

    using System;

    [Serializable]
    public class MyGameData
    {
        public int version = 1;
        public string playerName = "Player";
        public int score = 0;
    }

---

### 2. Create Validator

    using SaveSystem.Validation;

    public class MyGameDataValidator : ISaveDataValidator<MyGameData>
    {
        public MyGameData Validate(MyGameData data)
        {
            if (data.score < 0)
                data.score = 0;

            return data;
        }
    }

---

### 3. Create SaveManager

    using SaveSystem.Core;

    var saveManager = SaveManagerFactory.Create(
        defaultData: new MyGameData(),
        validator: new MyGameDataValidator(),
        registerMigrations: m => { },
        getVersion: d => d.version,
        setVersion: (d, v) => d.version = v,
        currentVersion: 1,
        masterSecret: "my-secret-key",
        fileName: "gamesave.json"
    );

---

### 4. Save Data

    saveManager.Save(data);

---

### 5. Load Data

    var data = saveManager.Load();

---

## Save Version Migration

If the save structure changes between versions, create a migration.

    using SaveSystem.Versioning;

    public class Migration_V1_V2 : ISaveMigration<MyGameData>
    {
        public int FromVersion => 1;
        public int ToVersion => 2;

        public MyGameData Migrate(MyGameData oldData)
        {
            oldData.score = 0;
            return oldData;
        }
    }

Register migrations when creating the manager:

    registerMigrations: m =>
    {
        m.RegisterMigration(new Migration_V1_V2());
    },

---

## Save File Format

Example encrypted save structure:

    {
      "formatVersion": 1,
      "algorithm": "AES-CBC-HMACSHA256",
      "ivBase64": "...",
      "cipherTextBase64": "...",
      "macBase64": "..."
    }

---

## Security Model

Encryption  
AES-CBC protects save data confidentiality.

Integrity verification  
HMAC-SHA256 detects tampering or corruption.

If verification fails, the system can reject the save file and fall back to default data.

---

## Running Tests

Open Unity Test Runner:

    Window → General → Test Runner

Run the runtime tests included in the package.

---

## Package Information

Package name:

    com.judze.savesystem.runtime

Assembly name:

    JuDze.SaveSystem.Runtime

Root namespace:

    SaveSystem

---

## License

MIT License

This package can be used in both personal and commercial projects.