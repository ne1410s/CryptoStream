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
    private readonly byte[] headerBuffer = new byte[bufferLength];
    private readonly byte[] zeroBuffer = new byte[bufferLength];
    private readonly MemoryStream writeCache = new();
    private readonly MemoryStream trailerCache = new();
    private long trailerStartBlock;

    /// <inheritdoc/>
    public string Id { get; protected set; } = $"{Guid.NewGuid()}";

    /// <inheritdoc/>
    public int BufferLength => bufferLength;

    /// <inheritdoc/>
    public long BlockNumber => 1 + (long)Math.Floor((double)this.Position / bufferLength);

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
    /// Gets or sets a value indicating whether writes are being cached. This is
    /// designed to capture arbitrary writes to the trailer.
    /// </summary>
    public bool CacheTrailer
    {
        get => this.trailerStartBlock > 0;
        set
        {
            if (value)
            {
                this.trailerStartBlock = this.BlockNumber;
                this.FlushCache();
            }
        }
    }

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
        // Rewind the zeros to overwrite them
        stream.Position -= this.writeCache.Length;

        var ahead = stream.Position % bufferLength;
        var bytes = this.writeCache.ToArray();
        var writable = Math.Min(bufferLength - ahead, bytes.Length);
        Array.Copy(bytes, 0, this.BlockBuffer, ahead, writable);
        if (this.BlockNumber == 1)
        {
            Array.Copy(bytes, 0, this.headerBuffer, ahead, writable);
        }

        var trailerStartPosition = this.CacheTrailer ? (this.trailerStartBlock - 1) * bufferLength : 0;
        var trailerRelativeSeek = this.Position - trailerStartPosition;
        var dirtyPostHeader = this.Position + bytes.Length > bufferLength && this.Position < this.Length;
        if (dirtyPostHeader && trailerRelativeSeek < 0)
        {
            throw new InvalidOperationException($"Unable to write dirty block {this.BlockNumber}.");
        }

        if (this.CacheTrailer && trailerRelativeSeek >= 0)
        {
            this.trailerCache.Seek(trailerRelativeSeek, SeekOrigin.Begin);
            this.trailerCache.Write(bytes, 0, bytes.Length);
        }

        this.TransformBufferForWrite(this.BlockNumber);
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
            stream.Write(this.zeroBuffer, 0, cacheable);
            if (this.writeCache.Length == bufferLength)
            {
                this.FlushCache();
            }

            relativeOffset += cacheable;
            count -= cacheable;
        }
    }

    /// <inheritdoc/>
    public virtual void FinaliseWrite()
    {
        this.FlushCache();

        // re-write header
        stream.Seek(0, SeekOrigin.Begin);
        Array.Copy(this.headerBuffer, this.BlockBuffer, bufferLength);
        this.TransformBufferForWrite(this.BlockNumber);
        stream.Write(this.BlockBuffer, 0, bufferLength);

        // re-write trailer
        if (this.CacheTrailer)
        {
            var trailerStartPosition = (this.trailerStartBlock - 1) * bufferLength;
            var excess = stream.Length - trailerStartPosition;
            if (excess != this.trailerCache.Length)
            {
                throw new InvalidOperationException("Unexpected trailer cache size.");
            }

            ////// Experimental / diagnosis
            ////var danglingBytes = new byte[excess];
            ////stream.Seek(trailerStartPosition, SeekOrigin.Begin);
            ////_ = stream.Read(danglingBytes, 0, (int)excess);
            ////var aboutToWrite = this.trailerCache.ToArray();

            stream.SetLength(trailerStartPosition);
            stream.Seek(trailerStartPosition, SeekOrigin.Begin);
            this.trailerCache.Seek(0, SeekOrigin.Begin);
            this.trailerCache.CopyTo(stream, bufferLength);
        }
    }

    /// <inheritdoc/>
    public override long Seek(long offset, SeekOrigin origin)
    {
        if (this.writeCache.Length > 0)
        {
            var containedToFirstBlock = this.Position + this.writeCache.Length <= bufferLength;
            if (!containedToFirstBlock && !this.CacheTrailer && this.Position < this.trailerStartBlock)
            {
                throw new InvalidOperationException($"Unable to abandon block {this.BlockNumber}.");
            }

            this.FlushCache();
        }

        return stream.Seek(offset, SeekOrigin.Begin);
    }

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
