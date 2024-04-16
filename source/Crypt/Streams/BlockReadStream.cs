// <copyright file="BlockReadStream.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Crypt.Streams;

using System;
using System.IO;
using System.Linq;
using Crypt.Utils;

/// <summary>
/// A file stream that enforces discrete positions and read operations according to the
/// specified buffer length. Using this implementation directly is not generally recommended.
/// It is provided to separate logical processing and to assist with testing of derived streams.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="BlockReadStream"/> class.
/// </remarks>
/// <param name="stream">The source stream.</param>
/// <param name="bufferLength">The block buffer length.</param>
/// <param name="resizer">An array resizer.</param>
public class BlockReadStream(Stream stream, int bufferLength = 32768, IArrayResizer? resizer = null)
    : ReadStream(stream), ISimpleReadStream
{
    private readonly IArrayResizer arrayResizer = resizer ?? new ArrayResizer();

    /// <summary>
    /// Initializes a new instance of the <see cref="BlockReadStream"/> class.
    /// </summary>
    /// <param name="fi">The source file.</param>
    /// <param name="bufferLength">The block buffer length.</param>
    /// <param name="resizer">An array resizer.</param>
    public BlockReadStream(FileInfo fi, int bufferLength = 32768, IArrayResizer? resizer = null)
        : this(new FileStream(fi?.FullName, FileMode.Open, FileAccess.Read), bufferLength, resizer)
    { }

    /// <inheritdoc/>
    public string Uri { get; } = Guid.NewGuid().ToString();

    /// <inheritdoc/>
    public int BufferLength => this.BlockBuffer.Length;

    /// <summary>
    /// Gets the source buffer.
    /// </summary>
    protected byte[] BlockBuffer { get; } = new byte[bufferLength];

    /// <inheritdoc/>
    public override int Read(byte[] buffer, int offset, int count)
    {
        var originalPosition = this.Position;
        this.Position = StreamBlockUtils.BlockPosition(
            this.Position, this.BufferLength, out var block1, out var remainder);
        var blockSpan = (int)Math.Ceiling((double)(remainder + count) / this.BufferLength);
        var totalBytesRead = 0;

        foreach (var blockIndex in Enumerable.Range(0, blockSpan))
        {
            var blockRead = this.Inner.Read(this.BlockBuffer, 0, this.BufferLength);
            var pertinentBlockRead = Math.Min(blockRead - remainder, count - totalBytesRead);
            pertinentBlockRead = (int)Math.Min(
                (double)pertinentBlockRead,
                this.Length - (originalPosition + totalBytesRead));
            var mappedBlock = this.MapBlock(this.BlockBuffer, block1 + blockIndex);
            Array.Copy(mappedBlock, remainder, buffer, totalBytesRead, pertinentBlockRead);
            totalBytesRead += pertinentBlockRead;
            remainder = 0;
        }

        this.Position = originalPosition + totalBytesRead;
        return totalBytesRead;
    }

    /// <inheritdoc/>
    public byte[] Read()
    {
        var buffer = new byte[this.BufferLength];
        var actualRead = this.Read(buffer, 0, buffer.Length);
        if (actualRead < buffer.Length)
        {
            this.arrayResizer.Resize(ref buffer, actualRead);
        }

        return buffer;
    }

    /// <inheritdoc/>
    public virtual long Seek(long position) => this.Seek(position, SeekOrigin.Begin);

    /// <summary>
    /// Obtains a mapped block.
    /// </summary>
    /// <param name="sourceBuffer">The source buffer.</param>
    /// <param name="blockNo">The discrete block number.</param>
    /// <returns>Mapped bytes.</returns>
    protected virtual byte[] MapBlock(byte[] sourceBuffer, long blockNo)
    {
        sourceBuffer = sourceBuffer ?? throw new ArgumentNullException(nameof(sourceBuffer));
        var retVal = new byte[sourceBuffer.Length];
        Array.Copy(sourceBuffer, retVal, sourceBuffer.Length);
        return retVal;
    }
}
