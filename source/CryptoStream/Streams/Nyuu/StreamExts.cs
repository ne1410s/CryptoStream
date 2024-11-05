// <copyright file="StreamExts.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace CryptoStream.Streams.Nyuu;

using System.IO;

/// <summary>
/// Stream extensions.
/// </summary>
public static class StreamExts
{
    /// <summary>
    /// Initialises a block stream from a source stream.
    /// </summary>
    /// <param name="stream">The source stream.</param>
    /// <param name="salt">The salt.</param>
    /// <param name="key">The key.</param>
    /// <param name="bufferLength">The buffer length.</param>
    /// <returns>A block stream.</returns>
    public static BlockStream AsBlockStream(this Stream stream, byte[] salt, byte[] key, int bufferLength = 32768)
    {
        stream.NotNull().Reset(true);
        return new BlockStream(stream, bufferLength);
    }
}
