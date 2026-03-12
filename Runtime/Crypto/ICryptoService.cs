namespace SaveSystem.Crypto
{
    public interface ICryptoService
    {
        // Encrypts plainBytes using the provided key and initialization vector (iv).
        // Returns the encrypted bytes (cipherBytes).
        byte[] Encrypt(byte[] plainBytes, byte[] key, byte[] iv);

        // Decrypts cipherBytes using the same key and initialization vector (iv)
        // that were used during encryption.
        // Returns the original plainBytes.
        byte[] Decrypt(byte[] cipherBytes, byte[] key, byte[] iv);

        // Generates a new initialization vector (IV)
        // used during encryption for security purposes.
        byte[] GenerateIv();
    }
}
