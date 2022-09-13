using Jose;

namespace Crypto.Transform
{
    /// <summary>
    /// Performs block decryption using AES-GCM.
    /// </summary>
    public class AesGcmDecryptor : GcmDecryptorBase
    {
        /// <inheritdoc/>
        public override byte[] DecryptBlock(
            GcmEncryptedBlock block,
            byte[] cryptoKey,
            byte[] counter,
            bool authenticate) => authenticate
                ? AesGcm.Decrypt(cryptoKey, counter, new byte[0], block.MessageBuffer, block.MacBuffer)
                : AesGcm.Encrypt(cryptoKey, counter, new byte[0], block.MessageBuffer)[0];
    }
}
