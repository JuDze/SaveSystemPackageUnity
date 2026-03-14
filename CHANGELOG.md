# Changelog

## [1.0.0] — 2026

### Added
- `SaveManager<TData>` — generic core manager (serialization, encryption, integrity, migration)
- `SaveManagerFactory` — builder/factory for game-side instantiation
- `AesCryptoService` — AES-CBC-256 encryption
- `ChecksumService` — HMAC-SHA256 integrity verification
- `StaticKeyProvider` — key derivation from master secret via SHA256
- `CryptoUtilities` — byte array helpers
- `SaveEnvelope` — encrypted save container model
- `JsonSaveSerializer<T>` + `JsonSaveEnvelopeSerializer` — Unity JsonUtility serialization
- `FileStorageService` — local filesystem storage
- `ISaveDataValidator<T>` — validation interface
- `MigrationManager<T>` + `VersionManager<T>` — versioned migration pipeline
- `ISaveMigration<T>` — single migration step interface
- `SaveSystemEditorWindow` — Editor inspector (Tools > Save System)
- NUnit test suite covering all modules

### Improved
- Strict versioning rules: invalid or incomplete migration chains now throw exceptions
- Validation for migration registration (duplicate steps and invalid version ranges)
- Handling of saves from newer game versions (`future save` protection)

### Added (Reliability)
- Safe write mechanism using temporary `.tmp` file
- Automatic backup `.bak` file rotation
- Recovery logic: fallback to backup save if the main save is corrupted

### Tests
- Extended test coverage for migration pipeline
- Added recovery tests for corrupted save files
- Added tests for versioning edge cases (future saves, invalid versions)
