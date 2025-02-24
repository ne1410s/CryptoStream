﻿// <copyright file="IBlockStream.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace CryptoStream.Streams;

/// <summary>
/// A stream that translates arbitrary reads and writes to a sequence of discrete blocks.
/// </summary>
public interface IBlockStream
{
    /// <summary>
    /// Gets a pseudo URI (to identify the input stream).
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the length in bytes of the read buffer that will be allocated.
    /// </summary>
    public int BufferLength { get; }

    /// <summary>
    /// Gets the block number.
    /// </summary>
    public long BlockNumber { get; }

    /// <summary>
    /// Flushes any local cache to the underlying device.
    /// </summary>
    public void FlushCache();

    /// <summary>
    /// Finalises the write operation.
    /// </summary>
    public void FinaliseWrite();
}
