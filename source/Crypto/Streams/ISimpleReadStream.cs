﻿using System;

namespace Crypto.Streams
{
    /// <summary>
    /// Defines the properties and methods necessary for implementing a
    /// custom media input stream.
    /// </summary>
    public interface ISimpleReadStream : IDisposable
    {
        /// <summary>
        /// Gets a pseudo URI (only to identify the stream).
        /// </summary>
        string Uri { get; }

        /// <summary>
        /// Gets a value indicating whether this stream is seekable.
        /// </summary>
        bool CanSeek { get; }

        /// <summary>
        /// Gets the length in bytes of the read buffer that will be allocated.
        /// Something like 4096 is recommended.
        /// </summary>
        int BufferLength { get; }

        /// <summary>
        /// Gets the total length in bytes of the source.
        /// </summary>
        long Length { get; }

        /// <summary>
        /// Reads the stream at its current position into the supplied buffer up
        /// to the maximum according to the buffer physical size. It is vital
        /// that the length of the array returned matches the number of bytes
        /// that were read.
        /// </summary>
        /// <returns>The bytes read.</returns>
        byte[] Read();

        /// <summary>
        /// Seeks to the specified offset.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <returns>The new position after the seek has occurred.</returns>
        long Seek(long offset);
    }
}
