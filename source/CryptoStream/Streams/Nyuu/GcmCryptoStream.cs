// <copyright file="GcmCryptoStream.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace CryptoStream.Streams.Nyuu;

using System.IO;

/// <summary>
/// A block stream that transforms buffer reads and writes according to gcm.
/// </summary>
public class GcmCryptoStream(Stream stream, byte[] salt, byte[] key, int bufferLength = 32768)
    : BlockStream(stream, bufferLength)
{
    /// <inheritdoc/>
    protected override void TransformBufferForRead(long blockNo)
    {
        // Do this
    }

    /// <inheritdoc/>
    protected override void TransformBufferForWrite(long blockNo)
    {
        // Do this
    }
}
