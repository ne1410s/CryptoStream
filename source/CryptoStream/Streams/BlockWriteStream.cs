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
    /// <summary>
    /// Initializes a new instance of the <see cref="BlockWriteStream"/> class.
    /// </summary>
    /// <param name="fi">The target file.</param>
    /// <param name="bufferLength">The block buffer length.</param>
    public BlockWriteStream(FileInfo fi, int bufferLength = 32768)
        : this(new FileStream(fi?.FullName, FileMode.Create, FileAccess.Write), bufferLength)
    { }

    /// <inheritdoc/>
    public int BufferLength => this.BlockBuffer.Length;

    /// <summary>
    /// Gets the target buffer.
    /// </summary>
    protected byte[] BlockBuffer { get; } = new byte[bufferLength];

    /// <inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count)
    {
        if (buffer?.Length != bufferLength)
        {
            throw new InvalidOperationException("Invalid buffer length.");
        }

        var requiredPos = StreamBlockUtils.BlockPosition(this.Position, bufferLength, out var block1, out _);
        if (this.Position != requiredPos)
        {
            throw new InvalidOperationException("Unexpected position.");
        }

        var blockSpan = (int)Math.Ceiling((double)count / bufferLength);
        foreach (var blockIndex in Enumerable.Range(0, blockSpan))
        {
            Array.Copy(buffer, blockIndex * bufferLength, this.BlockBuffer, 0, bufferLength);

            var mappedBlock = this.MapBlock(this.BlockBuffer, block1 + blockIndex);
            this.Inner.Write(mappedBlock, 0, mappedBlock.Length);
        }
    }

    /// <inheritdoc/>
    public int Write(byte[] bytes)
    {
        bytes = bytes ?? throw new ArgumentNullException(nameof(bytes));
        this.Write(bytes, 0, bufferLength);
        return bytes.Length;
    }

    /// <summary>
    /// Obtains a mapped block.
    /// </summary>
    /// <param name="inputBuffer">The input buffer.</param>
    /// <param name="blockNo">The discrete block number.</param>
    /// <returns>The output buffer.</returns>
    protected virtual byte[] MapBlock(byte[] inputBuffer, long blockNo)
    {
        inputBuffer = inputBuffer ?? throw new ArgumentNullException(nameof(inputBuffer));
        var retVal = new byte[inputBuffer.Length];
        Array.Copy(inputBuffer, retVal, inputBuffer.Length);
        return retVal;
    }
}
