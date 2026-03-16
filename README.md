# Modular Save System — Unity Package

![Unity](https://img.shields.io/badge/Unity-2022.3+-black?logo=unity)
![License](https://img.shields.io/badge/license-MIT-green)
![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20macOS%20%7C%20Linux-lightgrey)

A modular save system for Unity featuring AES encryption, HMAC-SHA256 integrity verification, JSON serialization, and versioned save data migration.

The system is designed to provide secure, extensible, and production-ready save functionality for Unity projects.

---

## Design Goals

The save system is designed with the following goals:

- **Security** — encrypted save files with integrity verification
- **Reliability** — backup and recovery mechanisms prevent data loss
- **Extensibility** — modular architecture allows replacing storage, serialization, or encryption
- **Version safety** — strict migration pipeline prevents loading incompatible saves
- **Unity integration** — designed for Unity projects and compatible with Unity Package Manager

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
- EditMode tests included

---

## Save Reliability

The system implements a safe-write mechanism to prevent save corruption.

Save procedure:

1. Data is written to a temporary `.tmp` file
2. The existing save is moved to `.bak`
3. The temporary file replaces the main save

Load procedure:

1. The system attempts to load the main save
2. If the main save is corrupted, the backup `.bak` file is loaded
3. If both fail, default data is returned

This ensures that partially written saves or crashes during saving do not permanently corrupt player data.

---

## Migration Rules

Each migration must:

- handle a single version step
- modify data fields only
- not change the version number manually

The version field is updated automatically by `MigrationManager`.

Invalid migrations (duplicate steps or invalid version ranges) will throw exceptions.

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

    Tests
     └── Editor
         └── SaveSystem.Tests.Editor.asmdef
         └── SaveSystemTests.cs
         └── SaveManagerRecoveryTests.cs

    Documentation~
     └── ExampleIntegration.cs
     
    Samples~
    └── Basic Save Example
        ├── README.md
        ├── BasicSaveExample.asmdef
        ├── Scenes
        │   └── BasicSaveExample.unity
        └── Scripts
            └── SaveSystemSampleController.cs


---

## Installation

### Install via Git URL

Open Unity Package Manager:

    Window → Package Manager

Click:

    + → Add package from git URL

Insert:

    https://github.com/JuDze/SaveSystemPackageUnity.git

Unity will automatically install the package.

---

### Local Installation

Edit:

    Packages/manifest.json

Add:

    {
      "dependencies": {
        "com.judze.savesystem.runtime": "file:../../SaveSystemPackageUnity"
      }
    }

Adjust the path according to your local folder structure.

---

## Quick Start

### 1. Define Save Data

```csharp
    using System;

    [Serializable]
    public class MyGameData
    {
        public int version = 1;
        public string playerName = "Player";
        public int score = 0;
    }
```
---

### 2. Create Validator
```csharp
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
```
---

### 3. Create SaveManager
```csharp
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
```
---

### 4. Save Data
```csharp
    saveManager.Save(data);
```
---

### 5. Load Data

If the save file is corrupted, the system will automatically attempt recovery using the backup save.
```csharp
    var data = saveManager.Load();
```

---


## Save Version Migration

If the save structure changes between versions, create a migration.
```csharp
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
```

Register migrations when creating the manager:
```csharp
    registerMigrations: m =>
    {
        m.RegisterMigration(new Migration_V1_V2());
    },
```
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

## Samples


The package includes a sample scene demonstrating how to use the save system.

Import it via:

Package Manager → Modular Save System → Samples → Import

The sample demonstrates:
• saving data
• loading data
• deleting saves
• checking save existence
• version migration

---

## Running Tests

Open Unity Test Runner:

    Window → General → Test Runner

Run the EditMode tests included in the package.

---

## Package Information

Package name:

    com.judze.savesystem.runtime

Assembly name:

    JuDze.SaveSystem.Runtime

Root namespace:

    SaveSystem

---

## Architecture

### Save Pipeline

```text
Game Data (TData)
        ↓
Serialization → (JsonSaveSerializer)
        ↓
Encryption → (AES-CBC)
        ↓
Integrity Protection → (HMAC-SHA256)
        ↓
SaveEnvelope
{ iv + cipherText + mac }
        ↓
Storage Layer → (FileStorageService)
        ↓
Disk
(Application.persistentDataPath)
```

### Load Pipeline

```text
Disk
   ↓
Storage → FileStorageService
   ↓
Integrity Check → HMAC-SHA256
   ↓
Decrypt → AES-CBC
   ↓
Deserialize → JsonSaveSerializer
   ↓
Migration → MigrationManager
   ↓
Validation → ISaveDataValidator
   ↓
Game Data (TData)
```
---

## License

MIT License

This package can be used in both personal and commercial projects.
