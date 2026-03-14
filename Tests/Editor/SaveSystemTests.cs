using NUnit.Framework;
using System;
using SaveSystem.Crypto;
using SaveSystem.Models;
using SaveSystem.Serialization;
using SaveSystem.Versioning;
using SaveSystem.Validation;

namespace SaveSystem.Tests
{
    // ------------------------------------------------------------------ //
    //  Test save data model
    // ------------------------------------------------------------------ //
    [Serializable]
    public class TestData
    {
        public int version = 1;
        public string playerName = "Hero";
        public int score = 0;
    }

    // ------------------------------------------------------------------ //
    //  Crypto: AES + HMAC
    // ------------------------------------------------------------------ //
    [TestFixture]
    public class AesCryptoServiceTests
    {
        private AesCryptoService _crypto;
        private byte[] _key;
        private byte[] _iv;

        [SetUp]
        public void Setup()
        {
            _crypto = new AesCryptoService();
            _key = new byte[32]; // 256-bit key
            _iv  = _crypto.GenerateIv();
            new System.Security.Cryptography.RNGCryptoServiceProvider().GetBytes(_key);
        }

        [Test]
        public void Encrypt_Decrypt_RoundTrip()
        {
            var original = System.Text.Encoding.UTF8.GetBytes("Hello Save System!");
            var encrypted = _crypto.Encrypt(original, _key, _iv);
            var decrypted = _crypto.Decrypt(encrypted, _key, _iv);
            Assert.AreEqual(original, decrypted);
        }

        [Test]
        public void GenerateIv_Returns16Bytes()
        {
            Assert.AreEqual(16, _crypto.GenerateIv().Length);
        }

        [Test]
        public void Encrypt_EmptyData_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                _crypto.Encrypt(new byte[0], _key, _iv));
        }

        [Test]
        public void Decrypt_WrongKey_ThrowsException()
        {
            var data      = System.Text.Encoding.UTF8.GetBytes("secret");
            var encrypted = _crypto.Encrypt(data, _key, _iv);
            var wrongKey  = new byte[32]; // all zeros
            Assert.Throws<System.Security.Cryptography.CryptographicException>(() => _crypto.Decrypt(encrypted, wrongKey, _iv));
        }
    }

    // ------------------------------------------------------------------ //
    //  Crypto: ChecksumService (HMAC-SHA256)
    // ------------------------------------------------------------------ //
    [TestFixture]
    public class ChecksumServiceTests
    {
        private ChecksumService _checksum;
        private byte[] _key;

        [SetUp]
        public void Setup()
        {
            _checksum = new ChecksumService();
            _key = System.Text.Encoding.UTF8.GetBytes("test-mac-key");
        }

        [Test]
        public void ComputeMac_SameInput_SameOutput()
        {
            var data = System.Text.Encoding.UTF8.GetBytes("test data");
            var mac1 = _checksum.ComputeMac(data, _key);
            var mac2 = _checksum.ComputeMac(data, _key);
            Assert.AreEqual(mac1, mac2);
        }

        [Test]
        public void VerifyMac_TamperedData_ReturnsFalse()
        {
            var original = System.Text.Encoding.UTF8.GetBytes("original");
            var tampered = System.Text.Encoding.UTF8.GetBytes("tampered");
            var mac      = _checksum.ComputeMac(original, _key);
            Assert.IsFalse(_checksum.VerifyMac(tampered, _key, mac));
        }

        [Test]
        public void VerifyMac_CorrectData_ReturnsTrue()
        {
            var data = System.Text.Encoding.UTF8.GetBytes("correct");
            var mac  = _checksum.ComputeMac(data, _key);
            Assert.IsTrue(_checksum.VerifyMac(data, _key, mac));
        }

        [Test]
        public void VerifyMac_NullExpectedMac_ReturnsFalse()
        {
            var data = System.Text.Encoding.UTF8.GetBytes("data");
            Assert.IsFalse(_checksum.VerifyMac(data, _key, null));
        }
    }

    // ------------------------------------------------------------------ //
    //  Crypto: StaticKeyProvider
    // ------------------------------------------------------------------ //
    [TestFixture]
    public class StaticKeyProviderTests
    {
        [Test]
        public void GetEncryptionKey_Returns32Bytes()
        {
            var provider = new StaticKeyProvider("my-secret");
            Assert.AreEqual(32, provider.GetEncryptionKey().Length);
        }

        [Test]
        public void EncryptionKey_And_MacKey_AreDifferent()
        {
            var provider = new StaticKeyProvider("my-secret");
            CollectionAssert.AreNotEqual(
                provider.GetEncryptionKey(),
                provider.GetMacKey());
        }

        [Test]
        public void SameSecret_ProducesSameKeys()
        {
            var p1 = new StaticKeyProvider("same-secret");
            var p2 = new StaticKeyProvider("same-secret");
            Assert.AreEqual(p1.GetEncryptionKey(), p2.GetEncryptionKey());
        }
    }

    // ------------------------------------------------------------------ //
    //  Serialization
    // ------------------------------------------------------------------ //
    [TestFixture]
    public class SerializationTests
    {
        [Test]
        public void JsonSaveSerializer_RoundTrip()
        {
            var serializer = new JsonSaveSerializer<TestData>();
            var original   = new TestData { playerName = "Alice", score = 42, version = 1 };
            var json       = serializer.Serialize(original);
            var result     = serializer.Deserialize(json);
            Assert.AreEqual(original.playerName, result.playerName);
            Assert.AreEqual(original.score,      result.score);
        }

        [Test]
        public void JsonSaveEnvelopeSerializer_RoundTrip()
        {
            var serializer = new JsonSaveEnvelopeSerializer();
            var envelope   = new SaveEnvelope
            {
                formatVersion    = 1,
                algorithm        = "AES-CBC-HMACSHA256",
                ivBase64         = "aGVsbG8=",
                cipherTextBase64 = "d29ybGQ=",
                macBase64        = "dGVzdA=="
            };
            var json   = serializer.Serialize(envelope);
            var result = serializer.Deserialize(json);
            Assert.AreEqual(envelope.algorithm,        result.algorithm);
            Assert.AreEqual(envelope.ivBase64,         result.ivBase64);
            Assert.AreEqual(envelope.cipherTextBase64, result.cipherTextBase64);
        }
    }

    // ------------------------------------------------------------------ //
    //  Versioning / Migration
    // ------------------------------------------------------------------ //

    public class TestMigration_V1_V2 : ISaveMigration<TestData>
    {
        public int FromVersion => 1;
        public int ToVersion   => 2;
        public TestData Migrate(TestData old)
        {
            old.playerName = old.playerName + "_migrated";
            return old;
        }
    }
    
    public class AnotherTestMigration_V1_V2 : ISaveMigration<TestData>
    {
        public int FromVersion => 1;
        public int ToVersion => 2;

        public TestData Migrate(TestData old)
        {
            old.score += 1;
            return old;
        }
    }

    public class InvalidMigration_V2_V1 : ISaveMigration<TestData>
    {
        public int FromVersion => 2;
        public int ToVersion => 1;

        public TestData Migrate(TestData old)
        {
            return old;
        }
    }

    [TestFixture]
    public class MigrationManagerTests
    {
        [Test]
        public void ApplyMigrations_V1_To_V2_TransformsData()
        {
            var manager = new MigrationManager<TestData>();
            manager.RegisterMigration(new TestMigration_V1_V2());

            var data   = new TestData { version = 1, playerName = "Hero" };
            var result = manager.ApplyMigrations(data, 1, 2, (d, v) => d.version = v);

            Assert.AreEqual(2, result.version);
            Assert.AreEqual("Hero_migrated", result.playerName);
        }

        [Test]
        public void ApplyMigrations_MissingStep_ThrowsInvalidOperationException()
        {
            var manager = new MigrationManager<TestData>();

            // No migration steps are registered, so reaching the target version is impossible.
            var data = new TestData { version = 1 };

            Assert.Throws<InvalidOperationException>(() =>
                manager.ApplyMigrations(data, 1, 3, (d, v) => d.version = v));
        }

        [Test]
        public void RegisterMigration_DuplicateFromVersion_Throws()
        {
            var manager = new MigrationManager<TestData>();
            manager.RegisterMigration(new TestMigration_V1_V2());

            Assert.Throws<InvalidOperationException>(() =>
                manager.RegisterMigration(new AnotherTestMigration_V1_V2()));
        }

        [Test]
        public void RegisterMigration_InvalidVersionStep_Throws()
        {
            var manager = new MigrationManager<TestData>();

            Assert.Throws<InvalidOperationException>(() =>
                manager.RegisterMigration(new InvalidMigration_V2_V1()));
        }

        [Test]
        public void ApplyMigrations_MissingStep_Throws()
        {
            var manager = new MigrationManager<TestData>();
            var data = new TestData { version = 1 };

            Assert.Throws<InvalidOperationException>(() =>
                manager.ApplyMigrations(data, 1, 3, (d, v) => d.version = v));
        }
    }


    [TestFixture]
    public class VersionManagerTests
    {
        [Test]
        public void EnsureLatestVersion_SameVersion_NoMigration()
        {
            // Create an empty migration manager (no migrations registered).
            var migManager = new MigrationManager<TestData>();

            // VersionManager configured with current version = 1.
            // It knows how to read and update the version field in TestData.
            var versionMgr = new VersionManager<TestData>(
                migManager,
                d => d.version,
                (d, v) => d.version = v,
                1);

            // Input data already matches the current version.
            var data = new TestData { version = 1 };

            // Execute version check / migration process.
            var result = versionMgr.EnsureLatestVersion(data);

            // No migration should occur and the version must remain unchanged.
            Assert.AreEqual(1, result.version);
        }

        [Test]
        public void EnsureLatestVersion_OldVersion_MigratesCorrectly()
        {
            // Create migration manager and register a migration step 1 -> 2.
            var migManager = new MigrationManager<TestData>();
            migManager.RegisterMigration(new TestMigration_V1_V2());

            // VersionManager expects the current save version to be 2.
            var versionMgr = new VersionManager<TestData>(
                migManager,
                d => d.version,
                (d, v) => d.version = v,
                2);

            // Simulate an old save file with version 1.
            var data = new TestData
            {
                version = 1,
                playerName = "Hero"
            };

            // Execute migration process.
            var result = versionMgr.EnsureLatestVersion(data);

            // After migration:
            // 1) Version must be updated to 2.
            // 2) Migration logic must modify the data (in this case playerName).
            Assert.AreEqual(2, result.version);
            StringAssert.Contains("_migrated", result.playerName);
        }

        [Test]
        public void EnsureLatestVersion_NewerSaveThanSupported_Throws()
        {
            // Create a migration manager with no migrations.
            var migManager = new MigrationManager<TestData>();

            // VersionManager supports saves up to version 2.
            var versionMgr = new VersionManager<TestData>(
                migManager,
                d => d.version,
                (d, v) => d.version = v,
                2);

            // Simulate loading a save file created by a newer version of the game.
            var data = new TestData { version = 3 };

            // System should reject this save because the game cannot downgrade data.
            Assert.Throws<InvalidOperationException>(() =>
                versionMgr.EnsureLatestVersion(data));
        }

        [Test]
        public void EnsureLatestVersion_NegativeSaveVersion_Throws()
        {
            // Create a migration manager.
            var migManager = new MigrationManager<TestData>();

            // VersionManager expects version 2 as the latest.
            var versionMgr = new VersionManager<TestData>(
                migManager,
                d => d.version,
                (d, v) => d.version = v,
                2);

            // Simulate corrupted or invalid save data with a negative version.
            var data = new TestData { version = -1 };

            // VersionManager should reject invalid version numbers.
            Assert.Throws<InvalidOperationException>(() =>
                versionMgr.EnsureLatestVersion(data));
        }

        [Test]
        public void EnsureLatestVersion_NullData_ThrowsArgumentNullException()
        {
            // Create migration manager.
            var migManager = new MigrationManager<TestData>();

            // Configure VersionManager with current version = 2.
            var versionMgr = new VersionManager<TestData>(
                migManager,
                d => d.version,
                (d, v) => d.version = v,
                2);

            // Passing null data is a programming error and should be rejected.
            Assert.Throws<ArgumentNullException>(() =>
                versionMgr.EnsureLatestVersion(null));
        }
    }

    // ------------------------------------------------------------------ //
    //  CryptoUtilities
    // ------------------------------------------------------------------ //
    [TestFixture]
    public class CryptoUtilitiesTests
    {
        [Test]
        public void Combine_TwoArrays_ResultIsCorrectLength()
        {
            var a      = new byte[] { 1, 2, 3 };
            var b      = new byte[] { 4, 5 };
            var result = CryptoUtilities.Combine(a, b);
            Assert.AreEqual(5, result.Length);
            Assert.AreEqual(1, result[0]);
            Assert.AreEqual(5, result[4]);
        }

        [Test]
        public void Combine_NullFirst_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                CryptoUtilities.Combine(null, new byte[] { 1 }));
        }
    }
}
