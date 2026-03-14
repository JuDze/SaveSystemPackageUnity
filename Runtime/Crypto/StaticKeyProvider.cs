using System.Security.Cryptography;
using System.Text;

namespace SaveSystem.Crypto
{
    // Simple IKeyProvider implementation that derives cryptographic keys
    // from a single master secret string.
    public class StaticKeyProvider : IKeyProvider
    {
        // Encryption key (AES)
        private readonly byte[] encryptionKey;

        // MAC key (HMAC)
        private readonly byte[] macKey;

        // Constructor that derives both keys from the master secret
        public StaticKeyProvider(string masterSecret)
        {
            if (string.IsNullOrWhiteSpace(masterSecret))
                throw new ArgumentException("Master secret cannot be null or empty.", nameof(masterSecret));

            byte[] masterBytes = Encoding.UTF8.GetBytes(masterSecret);

            using SHA256 sha256 = SHA256.Create();

            encryptionKey = sha256.ComputeHash(
                CryptoUtilities.Combine(masterBytes, Encoding.UTF8.GetBytes("ENC")));

            macKey = sha256.ComputeHash(
                CryptoUtilities.Combine(masterBytes, Encoding.UTF8.GetBytes("MAC")));
        }

        // Returns the encryption key
        public byte[] GetEncryptionKey() => encryptionKey;

        // Returns the MAC key
        public byte[] GetMacKey() => macKey;

        // Helper method to concatenate two byte arrays
        private static byte[] Combine(byte[] first, byte[] second)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));

            if (second == null)
                throw new ArgumentNullException(nameof(second));

            byte[] result = new byte[first.Length + second.Length];

            Buffer.BlockCopy(first, 0, result, 0, first.Length);
            Buffer.BlockCopy(second, 0, result, first.Length, second.Length);

            return result;
        }
    }
}
