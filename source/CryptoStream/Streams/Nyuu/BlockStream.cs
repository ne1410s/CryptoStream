// <copyright file="BlockStream.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace CryptoStream.Streams.Nyuu;

using System;
using System.IO;
using System.Linq;
using CryptoStream.Utils;

/// <inheritdoc cref="IBlockStream"/>
/// <param name="stream">The input stream.</param>
/// <param name="bufferLength">The buffer length.</param>
public class BlockStream(Stream stream, int bufferLength = 32768) : IBlockStream
{
    /// <inheritdoc/>
    public string Id { get; } = $"{Guid.NewGuid()}";

    /// <inheritdoc/>
    public int BufferLength => bufferLength;

    /// <inheritdoc/>
    public long Length => stream.Length;

    /// <inheritdoc/>
    public bool CanRead => stream.CanRead;

    /// <inheritdoc/>
    public bool CanWrite => stream.CanWrite;

    /// <inheritdoc/>
    public bool CanSeek => stream.CanSeek;

    /// <summary>
    /// Gets the internal block buffer.
    /// </summary>
    protected byte[] BlockBuffer { get; } = new byte[bufferLength];

    /// <inheritdoc/>
    public int Read(byte[] buffer)
    {
        var count = bufferLength;
        var originalPosition = stream.Position;
        stream.Position = StreamBlockUtils.BlockPosition(
            stream.Position, this.BufferLength, out var block1, out var remainder);
        var blockSpan = (int)Math.Ceiling((double)(remainder + count) / this.BufferLength);
        var totalBytesRead = 0;

        foreach (var blockIndex in Enumerable.Range(0, blockSpan))
        {
            var blockRead = stream.Read(this.BlockBuffer, 0, this.BufferLength);
            var pertinentBlockRead = Math.Min(blockRead - remainder, count - totalBytesRead);
            pertinentBlockRead = (int)Math.Min(
                (double)pertinentBlockRead,
                this.Length - (originalPosition + totalBytesRead));
            this.MapReadBlock(block1 + blockIndex);
            Array.Copy(this.BlockBuffer, remainder, buffer, totalBytesRead, pertinentBlockRead);
            totalBytesRead += pertinentBlockRead;
            remainder = 0;
        }

        stream.Position = originalPosition + totalBytesRead;
        return totalBytesRead;
    }

    /// <inheritdoc/>
    public int Write(byte[] bytes)
    {
        // 
    }

    /// <inheritdoc/>
    public long Seek(long position) => stream.Seek(position, SeekOrigin.Begin);

    /// <inheritdoc/>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        stream.Dispose();
    }

    /// <summary>
    /// Maps the block buffer for read.
    /// </summary>
    /// <param name="blockNo">The discrete block number.</param>
    protected virtual void MapReadBlock(long blockNo)
    { }

    /// <summary>
    /// Maps the block buffer for write.
    /// </summary>
    /// <param name="blockNo">The discrete block number.</param>
    protected virtual void MapWriteBlock(long blockNo)
    { }
}
