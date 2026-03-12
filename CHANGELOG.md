# Changelog

## [1.0.0] — 2025

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
