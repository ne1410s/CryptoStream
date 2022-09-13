using Crypto.Keying;
using Jose;

namespace Crypto.Transform
{
    /// <summary>
    /// Performs block encryption using AES-GCM.
    /// </summary>
    public class AesGcmEncryptor : GcmEncryptorBase
    {
        /// <summary>
        /// Initialises a new instance of <see cref="AesGcmEncryptor"/>.
        /// </summary>
        /// <param name="keyDeriver"></param>
        public AesGcmEncryptor(ICryptoKeyDeriver keyDeriver = null)
            : base(keyDeriver ?? new DefaultKeyDeriver())
        { }

        /// <inheritdoc/>
        public override GcmEncryptedBlock EncryptBlock(byte[] source, byte[] cryptoKey, byte[] counter)
        {
            var outputBuffers = AesGcm.Encrypt(cryptoKey, counter, new byte[0], source);
            return new GcmEncryptedBlock(outputBuffers[0], outputBuffers[1]);
        }
    }
}
