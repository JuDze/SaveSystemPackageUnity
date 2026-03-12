namespace SaveSystem.Crypto
{
    // Interface that defines the mechanism for providing cryptographic keys.
    // Retrieves the encryption key and the MAC computation key.
    // Key retrieval logic is separated from cryptographic services
    // to allow easy replacement of the key storage mechanism.
    public interface IKeyProvider
    {
        // Returns the encryption key used for
        // data encryption and decryption (e.g. in AES algorithm).
        byte[] GetEncryptionKey();

        // Returns the MAC key used for
        // data integrity verification (HMAC algorithm).
        byte[] GetMacKey();
    }
}
