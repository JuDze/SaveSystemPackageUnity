using System;
using System.Security.Cryptography;

namespace SaveSystem.Crypto
{
    // Provides data integrity verification using the HMAC-SHA256 algorithm.
    // MAC (Message Authentication Code) is used to ensure
    // that save data has not been tampered with.
    public class ChecksumService
    {
        // Computes a MAC (Message Authentication Code) for the given data.
        // data – the data to compute the MAC for
        // key  – the secret key used in the HMAC algorithm
        public byte[] ComputeMac(byte[] data, byte[] key)
        {
            if (data == null || data.Length == 0)
                throw new ArgumentException("MAC input data cannot be empty.", nameof(data));

            if (key == null || key.Length == 0)
                throw new ArgumentException("MAC key cannot be empty.", nameof(key));

            using HMACSHA256 hmac = new HMACSHA256(key);
            return hmac.ComputeHash(data);
        }

        // Verifies whether the data matches the expected MAC.
        // Used to check data integrity after loading.
        public bool VerifyMac(byte[] data, byte[] key, byte[] expectedMac)
        {
            if (data == null || data.Length == 0)
                return false;

            if (key == null || key.Length == 0)
                return false;

            if (expectedMac == null || expectedMac.Length == 0)
                return false;

            byte[] actualMac = ComputeMac(data, key);
            return CryptographicOperations.FixedTimeEquals(actualMac, expectedMac);
        }
    }
}
