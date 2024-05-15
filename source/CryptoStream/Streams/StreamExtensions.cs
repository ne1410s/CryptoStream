// <copyright file="StreamExtensions.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace CryptoStream.Streams;

using System;
using System.IO;

/// <summary>
/// Extensions for <see cref="Stream"/> class.
/// </summary>
public static class StreamExtensions
{
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
