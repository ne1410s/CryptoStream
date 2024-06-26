﻿// <copyright file="ReadStream.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace CryptoStream.Streams;

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

/// <summary>
/// A read stream to assist with testing / diagnosis of derived types.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ReadStream"/> class.
/// </remarks>
/// <param name="stream">The source stream.</param>
public class ReadStream(Stream stream) : Stream
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReadStream"/> class.
    /// </summary>
    /// <param name="fi">The source file.</param>
    public ReadStream(FileInfo fi)
        : this(new FileStream(fi?.FullName, FileMode.Open, FileAccess.Read))
    { }

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

    /// <summary>
    /// Not supported.
    /// </summary>
    public override void Flush()
        => throw new NotSupportedException("This stream is read-only.");

    /// <inheritdoc/>
    /// <remarks>Not supported.</remarks>
    public override void SetLength(long value)
        => throw new NotSupportedException("This stream is read-only.");

    /// <inheritdoc/>
    /// <remarks>Not supported.</remarks>
    public override void Write(byte[] buffer, int offset, int count)
        => throw new NotSupportedException("This stream is read-only.");

    /// <inheritdoc/>
    [SuppressMessage(
        "Usage", "CA2215:Dispose methods should call base class dispose", Justification = "Nothing to dispose")]
    protected override void Dispose(bool disposing)
    {
        this.Inner.Dispose();
    }
}
