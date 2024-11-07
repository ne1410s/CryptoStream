// <copyright file="BlockStream.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace CryptoStream.Streams;

using System;
using System.IO;
using System.Linq;
using CryptoStream.Utils;

/// <inheritdoc cref="IBlockStream"/>
/// <param name="stream">The input stream.</param>
/// <param name="bufferLength">The buffer length.</param>
public class BlockStream(Stream stream, int bufferLength = 32768) : Stream, IBlockStream
{
    private readonly MemoryStream writeCache = new(bufferLength);
    private readonly byte[] zeroBuffer = new byte[bufferLength];

    /// <inheritdoc/>
    public string Id { get; protected set; } = $"{Guid.NewGuid()}";

    /// <inheritdoc/>
    public int BufferLength => bufferLength;

    /// <inheritdoc/>
    public override bool CanRead => stream.CanRead;

    /// <inheritdoc/>
    public override bool CanSeek => stream.CanSeek;

    /// <inheritdoc/>
    public override bool CanWrite => stream.CanWrite;

    /// <inheritdoc/>
    public override long Length => stream.Length;

    /// <inheritdoc/>
    public override long Position { get => stream.Position; set => stream.Position = value; }

    /// <summary>
    /// Gets the internal block buffer.
    /// </summary>
    protected byte[] BlockBuffer { get; } = new byte[bufferLength];

    /// <summary>
    /// Gets the inner stream.
    /// </summary>
    protected Stream Inner => stream;

    /// <inheritdoc/>
    public override int Read(byte[] buffer, int offset, int count)
    {
        var originalPosition = stream.Position;
        stream.Position = StreamBlockUtils.BlockPosition(
            stream.Position, bufferLength, out var block1, out var remainder);
        var blockSpan = (int)Math.Ceiling((double)(remainder + count) / bufferLength);
        var totalBytesRead = 0;

        foreach (var blockIndex in Enumerable.Range(0, blockSpan))
        {
            var blockRead = stream.Read(this.BlockBuffer, 0, bufferLength);
            var pertinentBlockRead = Math.Min(blockRead - remainder, count - totalBytesRead);
            pertinentBlockRead = (int)Math.Min(
                (double)pertinentBlockRead,
                stream.Length - (originalPosition + totalBytesRead));
            this.TransformBufferForRead(block1 + blockIndex);
            Array.Copy(this.BlockBuffer, remainder, buffer, totalBytesRead, pertinentBlockRead);
            totalBytesRead += pertinentBlockRead;
            remainder = 0;
        }

        stream.Position = originalPosition + totalBytesRead;
        return totalBytesRead;
    }

    /// <inheritdoc/>
    public void FlushCache()
    {
        // We've been writing zeros, so rewind to overwrite them
        stream.Position -= this.writeCache.Length;

        StreamBlockUtils.BlockPosition(stream.Position, bufferLength, out var blockNo, out _);
        Array.Copy(this.writeCache.ToArray(), this.BlockBuffer, (int)this.writeCache.Length);
        this.TransformBufferForWrite(blockNo);
        stream.Write(this.BlockBuffer, 0, (int)this.writeCache.Length);
        this.writeCache.SetLength(0);
    }

    /// <inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count)
    {
        var relativeOffset = 0;
        while (count > 0)
        {
            var cacheRoom = bufferLength - (int)this.writeCache.Length;
            var cacheable = Math.Min(cacheRoom, count);
            this.writeCache.Write(buffer, relativeOffset, cacheable);

            // Write zeros so that length and position still track ;)
            this.Inner.Write(this.zeroBuffer, 0, cacheable);
            if (this.writeCache.Length == bufferLength)
            {
                this.FlushCache();
            }

            relativeOffset += cacheable;
            count -= cacheable;
        }
    }

    /// <inheritdoc/>
    public virtual void FinaliseWrite() => this.FlushCache();

    /// <inheritdoc/>
    public override long Seek(long offset, SeekOrigin origin) => stream.Seek(offset, SeekOrigin.Begin);

    /// <inheritdoc/>
    public override void Flush() => stream.Flush();

    /// <inheritdoc/>
    public override void SetLength(long value) => stream.SetLength(value);

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        this.Inner.Dispose();
    }

    /// <summary>
    /// Maps the block buffer for read.
    /// </summary>
    /// <param name="blockNo">The discrete block number.</param>
    protected virtual void TransformBufferForRead(long blockNo)
    { }

    /// <summary>
    /// Maps the block buffer for write.
    /// </summary>
    /// <param name="blockNo">The discrete block number.</param>
    protected virtual void TransformBufferForWrite(long blockNo)
    { }
}
