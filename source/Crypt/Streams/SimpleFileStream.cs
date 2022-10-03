using System;
using System.IO;
using Crypt.Utils;

namespace Crypt.Streams
{
    /// <summary>
    /// A file stream supporting basic read operations.
    /// </summary>
    public class SimpleFileStream : FileStream, ISimpleReadStream
    {
        private readonly IArrayResizer arrayResizer;

        /// <summary>
        /// Initialises a new <see cref="SimpleFileStream"/>.
        /// </summary>
        /// <param name="fi">The source file.</param>
        /// <param name="bufferLength">The buffer length.</param>
        /// <param name="resizer">An array resizer.</param>
        public SimpleFileStream(FileInfo fi, int bufferLength = 32768, IArrayResizer resizer = null)
            : base(fi.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, bufferLength)
        {
            arrayResizer = resizer ?? new ArrayResizer();
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
                arrayResizer.Resize(ref retVal, actuallyRead);
            }

            return retVal;
        }

        /// <inheritdoc/>
        public long Seek(long position) => Seek(position, SeekOrigin.Begin);
    }
}
