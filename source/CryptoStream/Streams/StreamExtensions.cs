// <copyright file="StreamExtensions.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace CryptoStream.Streams;

using System;
using System.Collections.Generic;
using System.IO;
using CryptoStream.IO;

/// <summary>
/// Stream extensions.
/// </summary>
public static class StreamExtensions
{
    /// <summary>
    /// Opens a simple block stream from a file. This can be used to read or write.
    /// </summary>
    /// <param name="fi">The source file.</param>
    /// <param name="bufferLength">The buffer length.</param>
    /// <returns>The stream.</returns>
    public static BlockStream OpenSimple(this FileInfo fi, int bufferLength = 32768)
        => new(fi.NotNull().OpenRead(), bufferLength);

    /// <summary>
    /// Opens a crypto read stream from a file.
    /// </summary>
    /// <param name="fi">The source file.</param>
    /// <param name="key">The cryptographic key.</param>
    /// <param name="bufferLength">The buffer length.</param>
    /// <returns>The stream.</returns>
    public static GcmCryptoStream OpenRead(this FileInfo fi, byte[] key, int bufferLength = 32768)
    {
        var salt = fi.ToSalt();
        return new GcmCryptoStream(fi.NotNull().OpenRead(), salt, key, bufferLength);
    }

    /// <summary>
    /// Opens a crypto write stream from a file.
    /// </summary>
    /// <param name="fi">The target file to write. The extension should match the original.</param>
    /// <param name="salt">The salt.</param>
    /// <param name="key">The cryptographic key.</param>
    /// <param name="bufferLength">The buffer length.</param>
    /// <returns>The stream.</returns>
    public static GcmCryptoStream OpenWrite(
        this FileInfo fi, byte[] salt, byte[] key, int bufferLength = 32768)
    {
        var meta = new Dictionary<string, string> { ["filename"] = "_" + fi.NotNull().Extension };
        return new GcmCryptoStream(fi.OpenWrite(), salt, key, meta, bufferLength);
    }

    /// <summary>
    /// Resets the stream position to the beginning.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="skipIfUnable">If true, unseekable streams wont be affected.</param>
    public static void Reset(this Stream stream, bool skipIfUnable = false)
    {
        if ((stream ?? throw new ArgumentNullException(nameof(stream))).Position != 0
            && (!skipIfUnable || stream.CanSeek))
        {
            stream.Seek(0, SeekOrigin.Begin);
        }
    }
}
