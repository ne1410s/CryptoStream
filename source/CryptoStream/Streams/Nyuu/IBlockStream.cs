// <copyright file="IBlockStream.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace CryptoStream.Streams.Nyuu;

using System;

/// <summary>
/// A stream that translates arbitrary reads and writes to a sequence of discrete blocks.
/// </summary>
public interface IBlockStream : IDisposable
{
    /// <summary>
    /// Gets a pseudo URI (to identify the input stream).
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the length in bytes of the read buffer that will be allocated.
    /// </summary>
    int BufferLength { get; }

    /// <summary>
    /// Gets the total length in bytes of the input.
    /// </summary>
    long Length { get; }

    /// <summary>
    /// Gets a value indicating whether the input is readable.
    /// </summary>
    bool CanRead { get; }

    /// <summary>
    /// Gets a value indicating whether the input is writeable.
    /// </summary>
    bool CanWrite { get; }

    /// <summary>
    /// Gets a value indicating whether the input is seekable.
    /// </summary>
    bool CanSeek { get; }

    /// <summary>
    /// Reads an arbitrary number of bytes from an arbitrary position,
    /// up to the buffer length.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    /// <returns>The number of bytes read.</returns>
    public int Read(byte[] buffer);

    /// <summary>
    /// Writes bytes up to the input stream. Bytes are cached in the buffer
    /// until it is full, at which point the block is committed to the input.
    /// </summary>
    /// <param name="bytes">The bytes to write.</param>
    /// <returns>The number of bytes written.</returns>
    public int Write(byte[] bytes);

    /// <summary>
    /// Seeks on the input stream, to the absolute position (i.e. offset from the beginning).
    /// </summary>
    /// <param name="position">The requested position.</param>
    /// <returns>The resulting position.</returns>
    public long Seek(long position);
}
