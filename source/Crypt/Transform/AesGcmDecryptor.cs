using Crypt.Keying;
using Crypt.Utils;
using Jose;

namespace Crypt.Transform
{
    /// <summary>
    /// Performs block decryption using AES-GCM.
    /// </summary>
    public class AesGcmDecryptor : GcmDecryptorBase
    {
        /// <summary>
        /// Initialises a new instance of <see cref="AesGcmDecryptor"/>.
        /// </summary>
        /// <param name="keyDeriver">Derives the crypto key.</param>
        /// <param name="resizer">Resizes arrays.</param>
        public AesGcmDecryptor(
            ICryptoKeyDeriver keyDeriver = null,
            IArrayResizer resizer = null)
            : base(
                  keyDeriver ?? new DefaultKeyDeriver(),
                  resizer ?? new ArrayResizer())
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
