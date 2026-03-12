using System;
using System.IO;
using System.Security.Cryptography;

namespace SaveSystem.Crypto
{
    // Cryptographic service that implements data encryption and decryption
    // using the AES algorithm.
    public class AesCryptoService : ICryptoService
    {
        // AES block size is 128 bits (16 bytes),
        // therefore the initialization vector (IV) size is 16 bytes.
        private const int IvSizeBytes = 16;

        // Encrypts data using the AES algorithm.
        // plainBytes – original (unencrypted) data
        // key – encryption key
        // iv – initialization vector
        public byte[] Encrypt(byte[] plainBytes, byte[] key, byte[] iv)
        {
            if (plainBytes == null || plainBytes.Length == 0)
                throw new ArgumentException("Plain data cannot be empty.", nameof(plainBytes));

            ValidateKey(key);
            ValidateIv(iv);

            using Aes aes = Aes.Create();
            aes.Key     = key;
            aes.IV      = iv;
            aes.Mode    = CipherMode.CBC;       // Cipher Block Chaining mode
            aes.Padding = PaddingMode.PKCS7;    // Padding for block alignment

            using ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using MemoryStream memoryStream  = new MemoryStream();
            using CryptoStream cryptoStream  = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);

            cryptoStream.Write(plainBytes, 0, plainBytes.Length);
            cryptoStream.FlushFinalBlock();

            return memoryStream.ToArray();
        }

        // Decrypts AES-encrypted data
        public byte[] Decrypt(byte[] cipherBytes, byte[] key, byte[] iv)
        {
            if (cipherBytes == null || cipherBytes.Length == 0)
                throw new ArgumentException("Cipher data cannot be empty.", nameof(cipherBytes));

            ValidateKey(key);
            ValidateIv(iv);

            using Aes aes = Aes.Create();
            aes.Key     = key;
            aes.IV      = iv;
            aes.Mode    = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using MemoryStream memoryStream  = new MemoryStream(cipherBytes);
            using CryptoStream cryptoStream  = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            using MemoryStream outputStream  = new MemoryStream();

            cryptoStream.CopyTo(outputStream);
            return outputStream.ToArray();
        }

        // Generates a new initialization vector (IV).
        // The IV is used together with the encryption key
        // to ensure cryptographic security.
        public byte[] GenerateIv()
        {
            byte[] iv = new byte[IvSizeBytes];
            // RandomNumberGenerator provides cryptographically secure random number generation
            RandomNumberGenerator.Fill(iv);
            return iv;
        }

        // Validates that the AES key length is acceptable
        private static void ValidateKey(byte[] key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            // AES supports only 16, 24 or 32 byte keys
            if (key.Length != 16 && key.Length != 24 && key.Length != 32)
                throw new ArgumentException("AES key must be 16, 24, or 32 bytes long.");
        }

        // Validates the initialization vector (IV) size
        private static void ValidateIv(byte[] iv)
        {
            if (iv == null)
                throw new ArgumentNullException(nameof(iv));

            if (iv.Length != IvSizeBytes)
                throw new ArgumentException($"IV must be {IvSizeBytes} bytes long.");
        }
    }
}
