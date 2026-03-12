using System;

namespace SaveSystem.Models
{
    // Data structure used for secure storage of save data.
    // Acts as an "envelope" for the encrypted payload.
    // Stores the encryption algorithm used, the initialization vector,
    // the ciphertext and the MAC — all in Base64 format.
    [Serializable]
    public class SaveEnvelope
    {
        public int    formatVersion;     // Save format version
        public string algorithm;         // Encryption algorithm identifier
        public string ivBase64;          // Initialization vector (IV) in Base64
        public string cipherTextBase64;  // Encrypted data in Base64
        public string macBase64;         // Message Authentication Code in Base64
    }
}
