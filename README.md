# Modular Save System — Unity Package

![Unity](https://img.shields.io/badge/Unity-2021.3+-black?logo=unity)
![License](https://img.shields.io/badge/license-MIT-green)
![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20macOS%20%7C%20Linux-lightgrey)

A modular save system for Unity with **AES encryption**, **HMAC-SHA256 integrity verification**, **JSON serialization**, and **versioned data migration**.

Designed for **production use**, **data safety**, and **long-term save compatibility**.

---

# Features

• AES-CBC encryption for save files  
• HMAC-SHA256 integrity protection (tamper detection)  
• JSON serialization using Unity JsonUtility  
• Generic architecture (`SaveManager<TData>`)  
• Versioned save migrations  
• Data validation layer  
• Modular architecture (crypto / storage / serialization / versioning)  
• Unity Package Manager compatible  
• Unit tests included  

---

# Package Structure

```
Runtime/
  Core/
    SaveManager.cs
    SaveManagerFactory.cs

  Crypto/
    ICryptoService.cs
    AesCryptoService.cs
    ChecksumService.cs
    CryptoUtilities.cs
    IKeyProvider.cs
    StaticKeyProvider.cs

  Models/
    SaveEnvelope.cs

  Serialization/
    ISaveSerializer.cs
    ISaveEnvelopeSerializer.cs
    JsonSaveSerializer.cs
    JsonSaveEnvelopeSerializer.cs

  Storage/
    IStorageService.cs
    FileStorageService.cs

  Validation/
    ISaveDataValidator.cs

  Versioning/
    ISaveMigration.cs
    MigrationManager.cs
    VersionManager.cs

Editor/
  SaveSystemEditorWindow.cs

Tests/
  Runtime/
    SaveSystemTests.cs

Documentation~/
  ExampleIntegration.cs
```

---

# Installation

## Install via Git URL

Unity → **Window → Package Manager → + → Add package from git URL**

```
https://github.com/yourname/com.savesystem.core.git
```

---

## Local Installation

Add to `Packages/manifest.json`

```json
{
  "dependencies": {
    "com.savesystem.core": "file:../../com.savesystem.core"
  }
}
```

---

# Quick Start

## 1. Define Save Data

```csharp
[Serializable]
public class MyGameData
{
    public int version = 1;
    public string playerName = "Hero";
    public int score = 0;
}
```

---

## 2. Create Validator

```csharp
public class MyValidator : ISaveDataValidator<MyGameData>
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

## 3. Create SaveManager

```csharp
var saveManager = SaveManagerFactory.Create(
    defaultData:        new MyGameData(),
    validator:          new MyValidator(),
    registerMigrations: m => m.RegisterMigration(new MyMigration_V1_V2()),
    getVersion:         d => d.version,
    setVersion:         (d, v) => d.version = v,
    currentVersion:     2,
    masterSecret:       "your-secret-key"
);
```

---

## 4. Save and Load

```csharp
saveManager.Save(myData);

var data = saveManager.Load();
```

---

# Save Version Migration

When your save structure changes, create a migration.

```csharp
public class MyMigration_V2_V3 : ISaveMigration<MyGameData>
{
    public int FromVersion => 2;
    public int ToVersion => 3;

    public MyGameData Migrate(MyGameData old)
    {
        // transform data
        return old;
    }
}
```

Then register migration and increase `currentVersion`.

---

# Save File Format

Encrypted save files are stored in the following format:

```json
{
  "formatVersion": 1,
  "algorithm": "AES-CBC-HMACSHA256",
  "ivBase64": "...",
  "cipherTextBase64": "...",
  "macBase64": "..."
}
```

---

# Security Model

Save files are protected by two layers:

### Encryption

AES-CBC encryption protects save data confidentiality.

### Integrity Check

HMAC-SHA256 ensures the save file was not modified or corrupted.

If tampering is detected:

• Save loading fails  
• Default data is returned  

---

# Running Tests

Unity → **Window → General → Test Runner**

Run:

```
SaveSystem.Tests.Runtime
```

---

# Editor Tools

An optional editor window is included:

```
Tools → Save System → Save Inspector
```

Allows inspecting save files during development.

---

# License

MIT License

You are free to use this package in personal or commercial projects.
