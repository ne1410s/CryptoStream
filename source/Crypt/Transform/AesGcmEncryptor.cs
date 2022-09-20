using Crypt.Keying;
using Crypt.Utils;
using Jose;

namespace Crypt.Transform
{
    /// <summary>
    /// Performs block encryption using AES-GCM.
    /// </summary>
    public class AesGcmEncryptor : GcmEncryptorBase
    {
        /// <summary>
        /// Initialises a new instance of <see cref="AesGcmEncryptor"/>.
        /// </summary>
        /// <param name="keyDeriver">Derives a crypto key.</param>
        /// <param name="resizer">Resizes arrays.</param>
        public AesGcmEncryptor(
            ICryptoKeyDeriver keyDeriver = null,
            IArrayResizer resizer = null)
            : base(
                  keyDeriver ?? new DefaultKeyDeriver(),
                  resizer ?? new ArrayResizer())
        { }

        /// <inheritdoc/>
        public override GcmEncryptedBlock EncryptBlock(byte[] source, byte[] cryptoKey, byte[] counter)
        {
            var outputBuffers = AesGcm.Encrypt(cryptoKey, counter, new byte[0], source);
            return new GcmEncryptedBlock(outputBuffers[0], outputBuffers[1]);
        }
    }
}
