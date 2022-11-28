// <copyright file="SimpleFileStream.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Crypt.Streams
{
    using System.IO;
    using Crypt.Utils;

    /// <summary>
    /// A file stream supporting basic read operations.
    /// </summary>
    public class SimpleFileStream : FileStream, ISimpleReadStream
    {
        private readonly IArrayResizer arrayResizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleFileStream"/> class.
        /// </summary>
        /// <param name="fi">The source file.</param>
        /// <param name="bufferLength">The buffer length.</param>
        /// <param name="resizer">An array resizer.</param>
        public SimpleFileStream(FileInfo fi, int bufferLength = 32768, IArrayResizer resizer = null)
            : base(
                fi?.FullName,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferLength)
        {
            this.arrayResizer = resizer ?? new ArrayResizer();
            this.BufferLength = bufferLength;
            this.Uri = fi.FullName;
        }

        /// <inheritdoc/>
        public string Uri { get; }

        /// <inheritdoc/>
        public int BufferLength { get; }

        /// <inheritdoc/>
        public byte[] Read()
        {
            var retVal = new byte[this.BufferLength];
            var actuallyRead = this.Read(retVal, 0, this.BufferLength);
            if (actuallyRead < this.BufferLength)
            {
                this.arrayResizer.Resize(ref retVal, actuallyRead);
            }

            return retVal;
        }

        /// <inheritdoc/>
        public long Seek(long position) => this.Seek(position, SeekOrigin.Begin);
    }
}
