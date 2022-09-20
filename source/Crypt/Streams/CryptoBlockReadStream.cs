using System.IO;
using Crypt.IO;
using Crypt.Keying;
using Crypt.Transform;

namespace Crypt.Streams
{
    /// <summary>
    /// Provides a stream for reading a cryptographic file.
    /// </summary>
    public class CryptoBlockReadStream : BlockReadStream
    {
        private readonly int pepperLength;
        private readonly byte[] cryptoKey;
        private readonly IGcmDecryptor decryptor;

        /// <summary>
        /// Creates a new cryptographic file read stream.
        /// </summary>
        /// <param name="fi">The source file.</param>
        /// <param name="key">The key.</param>
        /// <param name="bufferSize">The buffer size.</param>
        /// <param name="decryptor">Decryptor override (optional).</param>
        public CryptoBlockReadStream(FileInfo fi, byte[] key, int bufferSize = 32768, IGcmDecryptor decryptor = null)
            : this(
                  new FileStream(fi.FullName, FileMode.Open, FileAccess.Read),
                  fi.ToSalt(),
                  key,
                  bufferSize,
                  decryptor)
        { }

        /// <summary>
        /// Creates a new cryptographic file read stream.
        /// </summary>
        /// <param name="stream">The source stream.</param>
        /// <param name="salt">The salt.</param>
        /// <param name="userKey">The key.</param>
        /// <param name="bufferSize">The buffer size.</param>
        /// <param name="decryptor">Decryptor override (optional).</param>
        public CryptoBlockReadStream(Stream stream, byte[] salt, byte[] userKey, int bufferSize = 32768, IGcmDecryptor decryptor = null)
            : base(stream, bufferSize)
        {
            this.decryptor = decryptor ?? new AesGcmDecryptor();
            var pepper = this.decryptor.ReadPepper(stream);
            pepperLength = pepper.Length;
            cryptoKey = new DefaultKeyDeriver().DeriveCryptoKey(userKey, salt, pepper);
        }

        /// <inheritdoc/>
        public override long Length => inner.Length - pepperLength;

        /// <inheritdoc/>
        protected override byte[] MapBlock(byte[] sourceBuffer, long chunkNumber)
        {
            var counter = chunkNumber.RaiseBits();
            var encryptedBlock = new GcmEncryptedBlock(sourceBuffer, new byte[0]);
            return decryptor.DecryptBlock(encryptedBlock, cryptoKey, counter, false);
        }
    }
}
