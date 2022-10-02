using System;
using System.IO;

namespace Crypt.Streams
{
    /// <summary>
    /// A file stream supporting basic read operations.
    /// </summary>
    public class SimpleFileStream : FileStream, ISimpleReadStream
    {
        /// <summary>
        /// Initialises a new <see cref="SimpleFileStream"/>.
        /// </summary>
        /// <param name="fi">The source file.</param>
        /// <param name="bufferLength">The buffer length.</param>
        public SimpleFileStream(FileInfo fi, int bufferLength = 32768)
            : base(fi.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, bufferLength)
        {
            BufferLength = bufferLength;
            Uri = fi.FullName;
        }

        /// <inheritdoc/>
        public string Uri { get; }

        /// <inheritdoc/>
        public int BufferLength { get; }

        /// <inheritdoc/>
        public byte[] Read()
        {
            var retVal = new byte[BufferLength];
            var actuallyRead = Read(retVal, 0, BufferLength);
            if (actuallyRead < BufferLength)
            {
                Array.Resize(ref retVal, actuallyRead);
            }

            return retVal;
        }

        /// <inheritdoc/>
        public long Seek(long position) => Seek(position, SeekOrigin.Begin);
    }
}
