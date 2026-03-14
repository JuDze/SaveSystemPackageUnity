using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using SaveSystem.Core;
using SaveSystem.Crypto;
using SaveSystem.Models;
using SaveSystem.Serialization;
using SaveSystem.Storage;
using SaveSystem.Validation;
using SaveSystem.Versioning;
using UnityEngine.TestTools;

namespace SaveSystem.Tests
{
    [TestFixture]
    public class SaveManagerRecoveryTests
    {
        private string _fileName;
        private string _mainPath;
        private SaveManager<TestData> _saveManager;

        [SetUp]
        public void SetUp()
        {
            // Use a unique file name for every test to avoid collisions
            // with other tests and with real game save files.
            _fileName = $"save_test_{Guid.NewGuid():N}.json";

            _saveManager = CreateSaveManager(_fileName);
            _mainPath = _saveManager.GetSavePath();

            // Ensure a clean environment before each test starts.
            DeleteIfExists(_mainPath);
            DeleteIfExists(GetBackupPath(_mainPath));
            DeleteIfExists(GetTempPath(_mainPath));
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up all files created by the test.
            DeleteIfExists(_mainPath);
            DeleteIfExists(GetBackupPath(_mainPath));
            DeleteIfExists(GetTempPath(_mainPath));
        }

        [Test]
        public void Save_CreatesMainFile_AndDoesNotLeaveTempFile()
        {
            // Arrange
            var data = new TestData
            {
                version = 1,
                playerName = "Alice",
                score = 10
            };

            // Act
            _saveManager.Save(data);

            // Assert
            Assert.IsTrue(File.Exists(_mainPath), "Main save file was not created.");
            Assert.IsFalse(File.Exists(GetTempPath(_mainPath)), "Temporary save file should not remain after a successful save.");
        }

        [Test]
        public void Save_SecondTime_CreatesBackupFile()
        {
            // Arrange
            var firstData = new TestData
            {
                version = 1,
                playerName = "First",
                score = 10
            };

            var secondData = new TestData
            {
                version = 1,
                playerName = "Second",
                score = 20
            };

            // Act
            _saveManager.Save(firstData);
            _saveManager.Save(secondData);

            // Assert
            Assert.IsTrue(File.Exists(_mainPath), "Main save file should exist after saving.");
            Assert.IsTrue(File.Exists(GetBackupPath(_mainPath)), "Backup save file should exist after overwriting the main save.");
        }

        [Test]
        public void Load_WhenMainSaveIsCorrupted_LoadsBackup()
        {
            // Arrange
            var firstData = new TestData
            {
                version = 1,
                playerName = "BackupData",
                score = 111
            };

            var secondData = new TestData
            {
                version = 1,
                playerName = "MainData",
                score = 222
            };

            // First save creates the main file.
            // Second save should move the old main file into backup.
            _saveManager.Save(firstData);
            _saveManager.Save(secondData);

            // Corrupt the main save file so loading from main must fail.
            File.WriteAllText(_mainPath, "corrupted main save content");

            // Act
            // Expect an error log because the main save file is intentionally corrupted
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Main save load failed"));
            var loaded = _saveManager.Load();

            // Assert
            Assert.IsNotNull(loaded, "Loaded data should not be null.");
            Assert.AreEqual("BackupData", loaded.playerName, "Backup data should be loaded when the main save is corrupted.");
            Assert.AreEqual(111, loaded.score, "Loaded score should match the backup save.");
        }

        [Test]
        public void Load_WhenMainAndBackupAreCorrupted_ReturnsDefaultData()
        {
            // Arrange
            var firstData = new TestData
            {
                version = 1,
                playerName = "BackupData",
                score = 111
            };

            var secondData = new TestData
            {
                version = 1,
                playerName = "MainData",
                score = 222
            };

            _saveManager.Save(firstData);
            _saveManager.Save(secondData);

            // Corrupt both main and backup files.
            File.WriteAllText(_mainPath, "corrupted main save content");
            File.WriteAllText(GetBackupPath(_mainPath), "corrupted backup save content");

            // Act
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Main save load failed"));
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Backup save load failed"));
            var loaded = _saveManager.Load();

            // Assert
            Assert.IsNotNull(loaded, "Default data should be returned when both save files are invalid.");
            Assert.AreEqual("Hero", loaded.playerName, "Default player name should be returned.");
            Assert.AreEqual(0, loaded.score, "Default score should be returned.");
            Assert.AreEqual(1, loaded.version, "Default version should be returned.");
        }

        [Test]
        public void Load_WhenMainFileIsEmpty_LoadsBackup()
        {
            // Arrange
            var firstData = new TestData
            {
                version = 1,
                playerName = "BackupData",
                score = 333
            };

            var secondData = new TestData
            {
                version = 1,
                playerName = "MainData",
                score = 444
            };

            _saveManager.Save(firstData);
            _saveManager.Save(secondData);

            // Replace the main save content with an empty file.
            File.WriteAllText(_mainPath, string.Empty);

            // Act
            var loaded = _saveManager.Load();

            // Assert
            Assert.IsNotNull(loaded, "Loaded data should not be null.");
            Assert.AreEqual("BackupData", loaded.playerName, "Backup data should be loaded when the main save file is empty.");
            Assert.AreEqual(333, loaded.score, "Loaded score should match the backup save.");
        }

        private SaveManager<TestData> CreateSaveManager(string fileName)
        {
            // Create all required services for a full end-to-end save manager instance.
            var serializer = new JsonSaveSerializer<TestData>();
            var envelopeSerializer = new JsonSaveEnvelopeSerializer();
            var cryptoService = new AesCryptoService();
            var checksumService = new ChecksumService();
            var keyProvider = new StaticKeyProvider("UnitTestSecretKey");
            var storage = new FileStorageService(fileName);

            var migrationManager = new MigrationManager<TestData>();
            var versionManager = new VersionManager<TestData>(
                migrationManager,
                d => d.version,
                (d, v) => d.version = v,
                1);

            var validator = new TestDataValidator();

            return new SaveManager<TestData>(
                serializer,
                envelopeSerializer,
                cryptoService,
                checksumService,
                keyProvider,
                storage,
                CreateDefaultData,
                versionManager,
                validator
            );
        }

        private static TestData CreateDefaultData()
        {
            // Return a deterministic default model for fallback scenarios.
            return new TestData
            {
                version = 1,
                playerName = "Hero",
                score = 0
            };
        }

        private static string GetBackupPath(string mainPath)
        {
            // Adjust this if your FileStorageService uses a different backup naming convention.
            return mainPath + ".bak";
        }

        private static string GetTempPath(string mainPath)
        {
            // Adjust this if your FileStorageService uses a different temp naming convention.
            return mainPath + ".tmp";
        }

        private static void DeleteIfExists(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }

        private class TestDataValidator : ISaveDataValidator<TestData>
        {
            public TestData Validate(TestData data)
            {
                // Keep validation minimal for recovery tests.
                if (data.score < 0)
                    data.score = 0;

                if (string.IsNullOrWhiteSpace(data.playerName))
                    data.playerName = "Hero";

                if (data.version <= 0)
                    data.version = 1;

                return data;
            }
        }
    }
}