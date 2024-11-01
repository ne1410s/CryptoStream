// <copyright file="ISimpleWriteStream.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace CryptoStream.Streams;

using System;

/// <summary>
/// A simple write stream.
/// </summary>
public interface ISimpleWriteStream : IDisposable
{
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
    /// Writes the bytes to the implementation buffer.
    /// </summary>
    /// <param name="bytes">The bytes to write.</param>
    /// <returns>The number of bytes written.</returns>
    int Write(byte[] bytes);

    /// <summary>
    /// Finalises the stream.
    /// </summary>
    void WriteFinal();
}
