// <copyright file="SimpleStream.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace CryptoStream.Streams;

using System.Diagnostics.CodeAnalysis;
using System.IO;

/// <summary>
/// A stream to assist with testing / diagnosis of derived types.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SimpleStream"/> class.
/// </remarks>
/// <param name="stream">The inner stream.</param>
public class SimpleStream(Stream stream) : Stream
{
    /// <inheritdoc/>
    public override bool CanRead => this.Inner.CanRead;

    /// <inheritdoc/>
    public override bool CanSeek => this.Inner.CanSeek;

    /// <inheritdoc/>
    public override bool CanWrite => false;

    /// <inheritdoc/>
    public override long Length => this.Inner.Length;

    /// <inheritdoc/>
    public override long Position { get => this.Inner.Position; set => this.Inner.Position = value; }

    /// <summary>
    /// Gets the inner stream.
    /// </summary>
    protected Stream Inner { get; } = stream;

    /// <inheritdoc/>
    public override long Seek(long offset, SeekOrigin origin) => this.Inner.Seek(offset, origin);

    /// <inheritdoc/>
    public override int Read(byte[] buffer, int offset, int count)
        => this.Inner.Read(buffer, offset, count);

    /// <inheritdoc/>
    public override void Flush() => this.Inner.Flush();

    /// <inheritdoc/>
    public override void SetLength(long value) => this.Inner.SetLength(value);

    /// <inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count)
        => this.Inner.Write(buffer, offset, count);

    /// <inheritdoc/>
    [SuppressMessage(
        "Usage", "CA2215:Dispose methods should call base class dispose", Justification = "Nothing to dispose")]
    protected override void Dispose(bool disposing)
    {
        this.Inner.Dispose();
    }
}
