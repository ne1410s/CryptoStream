using System.IO;
using Crypto.IO;
using Crypto.Keying;
using Crypto.Transform;
using Jose;

namespace Crypto.Streams
{
    /// <summary>
    /// Provides a stream for reading a cryptographic file.
    /// </summary>
    public class CryptoBlockReadStream : BlockReadStream
    {
        private readonly int pepperLength;
        private readonly byte[] cryptoKey;

        /// <summary>
        /// Creates a new cryptographic file read stream.
        /// </summary>
        /// <param name="fi">The source file.</param>
        /// <param name="key">The key.</param>
        /// <param name="bufferSize">The buffer size.</param>
        public CryptoBlockReadStream(FileInfo fi, byte[] key, int bufferSize = 32768)
            : this(
                  new FileStream(fi.FullName, FileMode.Open, FileAccess.Read),
                  fi.ToSalt(),
                  key,
                  bufferSize)
        { }

        /// <summary>
        /// Creates a new cryptographic file read stream.
        /// </summary>
        /// <param name="stream">The source stream.</param>
        /// <param name="salt">The salt.</param>
        /// <param name="userKey">The key.</param>
        /// <param name="bufferSize">The buffer size.</param>
        public CryptoBlockReadStream(Stream stream, byte[] salt, byte[] userKey, int bufferSize = 32768)
            : base(stream, bufferSize)
        {
            var pepper = new AesGcmDecryptor().ReadPepper(stream);
            pepperLength = pepper.Length;
            cryptoKey = new KeyDeriver().DeriveCryptoKey(userKey, salt, pepper);
        }

        /// <inheritdoc/>
        public override long Length => inner.Length - pepperLength;

        /// <inheritdoc/>
        protected override byte[] MapBlock(byte[] sourceBuffer, long chunkNumber)
        {
            var counter = chunkNumber.RaiseBits();
            return AesGcm.Encrypt(cryptoKey, counter, new byte[0], sourceBuffer)[0];
        }
    }
}
