using System;

namespace SaveSystem.Crypto
{
    // Helper class for cryptographic operations.
    // Contains static methods for working with byte arrays.
    // Combines two data blocks (IV and ciphertext) before computing the MAC.
    public static class CryptoUtilities
    {
        // Concatenates two byte arrays into a single array
        public static byte[] Combine(byte[] first, byte[] second)
        {
            if (first == null)  throw new ArgumentNullException(nameof(first));
            if (second == null) throw new ArgumentNullException(nameof(second));

            byte[] result = new byte[first.Length + second.Length];

            // Buffer.BlockCopy is the fastest copy method as it uses low-level operations
            Buffer.BlockCopy(first,  0, result, 0,            first.Length);
            Buffer.BlockCopy(second, 0, result, first.Length, second.Length);

            return result;
        }
    }
}
