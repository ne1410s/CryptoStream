using System.IO;
using Jose;

namespace Crypto.Streams
{
    /// <summary>
    /// Provides a stream for reading a cryptographic file.
    /// </summary>
    public class CryptoFileStream : ChunkingFileStream
    {
        private readonly int pepperLength;
        private readonly byte[] cryptoKey;

        /// <summary>
        /// Creates a new cryptographic file read stream.
        /// </summary>
        /// <param name="fi">The source file.</param>
        /// <param name="key">The key.</param>
        /// <param name="bufferSize">The buffer size.</param>
        public CryptoFileStream(FileInfo fi, byte[] key, int bufferSize = 32768)
            : this(
                  new FileStream(fi.FullName, FileMode.Open, FileAccess.Read),
                  fi.GenerateSalt(),
                  key,
                  bufferSize)
        { }

        /// <summary>
        /// Creates a new cryptographic file read stream.
        /// </summary>
        /// <param name="stream">The source stream.</param>
        /// <param name="salt">The salt.</param>
        /// <param name="key">The key.</param>
        /// <param name="bufferSize">The buffer size.</param>
        public CryptoFileStream(Stream stream, byte[] salt, byte[] key, int bufferSize = 32768)
            : base(stream, bufferSize)
        {
            var pepper = inner.GeneratePepper();
            pepperLength = pepper.Length;
            cryptoKey = key.Derive(salt, pepper);
        }

        /// <inheritdoc/>
        public override long Length => inner.Length - pepperLength;

        /// <inheritdoc/>
        protected override byte[] MapChunk(byte[] sourceBuffer, long chunkNumber)
        {
            var counter = chunkNumber.Pad12();
            return AesGcm.Encrypt(cryptoKey, counter, new byte[0], sourceBuffer)[0];
        }
    }
}
