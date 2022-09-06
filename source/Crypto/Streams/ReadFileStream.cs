using System;
using System.IO;

namespace Crypto.Streams
{
    /// <summary>
    /// A read-only file stream that is provided to assist with testing of derived streams.
    /// </summary>
    public class ReadFileStream : Stream
    {
        /// <summary>
        /// The inner stream.
        /// </summary>
        protected readonly Stream inner;

        /// <summary>
        /// Creates a new file read stream.
        /// </summary>
        /// <param name="fi">The source file.</param>
        public ReadFileStream(FileInfo fi)
            : this(new FileStream(fi.FullName, FileMode.Open, FileAccess.Read))
        { }

        /// <summary>
        /// Creates a new file read stream.
        /// </summary>
        /// <param name="stream">The source stream.</param>
        public ReadFileStream(Stream stream)
        {
            inner = stream;
        }

        /// <inheritdoc/>
        public override bool CanRead => inner.CanRead;

        /// <inheritdoc/>
        public override bool CanSeek => inner.CanSeek;

        /// <inheritdoc/>
        public override bool CanWrite => false;

        /// <inheritdoc/>
        public override long Length => inner.Length;

        /// <inheritdoc/>
        public override long Position { get => inner.Position; set => inner.Position = value; }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin) => inner.Seek(offset, origin);

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
            => inner.Read(buffer, offset, count);

        /// <summary>
        /// Not supported.
        /// </summary>
        public override void Flush()
            => throw new NotSupportedException("This stream is read-only.");

        /// <summary>
        /// Not supported.
        /// </summary>
        public override void SetLength(long value)
            => throw new NotSupportedException("This stream is read-only.");

        /// <summary>
        /// Not supported.
        /// </summary>
        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotSupportedException("This stream is read-only.");

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            inner.Dispose();
        }
    }
}
