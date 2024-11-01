// <copyright file="BlockWriteStream.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace CryptoStream.Streams;

using System;
using System.IO;
using System.Linq;
using CryptoStream.Utils;

/// <summary>
/// A stream that writes in discrete blocks.
/// </summary>
/// <param name="stream">The target stream.</param>
/// <param name="bufferLength">The block buffer length.</param>
public class BlockWriteStream(Stream stream, int bufferLength = 32768)
    : SimpleStream(stream), ISimpleWriteStream
{
    /// <inheritdoc/>
    public int BufferLength => this.BlockBuffer.Length;

    /// <summary>
    /// Gets the target buffer.
    /// </summary>
    protected byte[] BlockBuffer { get; } = new byte[bufferLength];

    /// <inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count)
    {
        StreamBlockUtils.BlockPosition(this.Position, bufferLength, out var block1, out _);
        var blockSpan = (int)Math.Ceiling((double)count / bufferLength);
        foreach (var blockIndex in Enumerable.Range(0, blockSpan))
        {
            var sz = (count < bufferLength && blockIndex == blockSpan - 1) ? count % bufferLength : bufferLength;
            sz = Math.Min(count - (blockIndex * bufferLength), sz);
            Array.Copy(buffer, blockIndex * bufferLength, this.BlockBuffer, 0, sz);
            var mappedBlock = this.MapBlock(this.BlockBuffer, block1 + blockIndex);
            this.Inner.Write(mappedBlock, 0, sz);
        }

        Array.Clear(this.BlockBuffer, 0, bufferLength);
    }

    /// <inheritdoc/>
    public int Write(byte[] bytes)
    {
        this.Write(bytes, 0, bytes.NotNull().Length);
        return bytes.Length;
    }

    /// <inheritdoc/>
    public virtual void WriteFinal()
    { }

    /// <summary>
    /// Obtains a mapped block.
    /// </summary>
    /// <param name="inputBuffer">The input buffer.</param>
    /// <param name="blockNo">The discrete block number.</param>
    /// <returns>The output buffer.</returns>
    protected virtual byte[] MapBlock(byte[] inputBuffer, long blockNo)
    {
        var retVal = new byte[inputBuffer.NotNull().Length];
        Array.Copy(inputBuffer, retVal, inputBuffer.Length);
        return retVal;
    }
}
