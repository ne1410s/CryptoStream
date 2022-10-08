// <copyright file="ISimpleReadStream.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Crypt.Streams
{
    using System;

    /// <summary>
    /// A simple read stream.
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
        /// Seeks to the specified absolute position.
        /// </summary>
        /// <param name="position">The absolute position.</param>
        /// <returns>The new position after the seek has occurred.</returns>
        long Seek(long position);
    }
}
