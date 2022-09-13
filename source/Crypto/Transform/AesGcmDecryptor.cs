using Crypto.Keying;
using Jose;

namespace Crypto.Transform
{
    /// <summary>
    /// Performs block decryption using AES-GCM.
    /// </summary>
    public class AesGcmDecryptor : GcmDecryptorBase
    {
        /// <summary>
        /// Initialises a new instance of <see cref="AesGcmDecryptor"/>.
        /// </summary>
        /// <param name="keyDeriver"></param>
        public AesGcmDecryptor(ICryptoKeyDeriver keyDeriver = null)
            : base(keyDeriver ?? new DefaultKeyDeriver())
        { }

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
