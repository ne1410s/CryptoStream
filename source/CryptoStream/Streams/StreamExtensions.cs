﻿// <copyright file="StreamExtensions.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace CryptoStream.Streams;

using System;
using System.IO;
using CryptoStream.IO;

/// <summary>
/// Stream extensions.
/// </summary>
public static class StreamExtensions
{
    /// <summary>
    /// Opens a simple block read stream from a file.
    /// </summary>
    /// <param name="fi">The source file.</param>
    /// <param name="bufferLength">The buffer length.</param>
    /// <returns>The stream.</returns>
    public static BlockStream OpenBlockRead(this FileInfo fi, int bufferLength = 32768)
        => new(fi.NotNull().OpenRead(), bufferLength);

    /// <summary>
    /// Opens a simple block write stream from a file.
    /// </summary>
    /// <param name="fi">The target file.</param>
    /// <param name="bufferLength">The buffer length.</param>
    /// <returns>The stream.</returns>
    public static BlockStream OpenBlockWrite(this FileInfo fi, int bufferLength = 32768)
        => new(fi.NotNull().OpenWrite(), bufferLength);

    /// <summary>
    /// Opens a crypto read stream from a file.
    /// </summary>
    /// <param name="fi">The source file.</param>
    /// <param name="key">The cryptographic key.</param>
    /// <param name="bufferLength">The buffer length.</param>
    /// <returns>The stream.</returns>
    public static GcmCryptoStream OpenCryptoRead(this FileInfo fi, byte[] key, int bufferLength = 32768)
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
    /// <param name="ext">The target extension.</param>
    /// <param name="bufferLength">The buffer length.</param>
    /// <returns>The stream.</returns>
    public static GcmCryptoStream OpenCryptoWrite(
        this FileInfo fi, byte[] salt, byte[] key, string ext, int bufferLength = 32768)
    {
        return new GcmCryptoStream(fi.NotNull().OpenWrite(), salt, key, ext, bufferLength);
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
