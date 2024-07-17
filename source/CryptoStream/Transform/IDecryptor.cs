// <copyright file="IDecryptor.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace CryptoStream.Transform;

using System.Collections.Generic;
using System.IO;

/// <summary>
/// Decrypts a stream of contiguous bytes (end to end).
/// </summary>
public interface IDecryptor
{
    /// <summary>
    /// Decrypts a stream.
    /// </summary>
    /// <param name="source">The input stream.</param>
    /// <param name="target">The output stream.</param>
    /// <param name="userKey">The originally supplied key.</param>
    /// <param name="salt">The salt generated in encryption.</param>
    /// <param name="expectMetadata">Whether metadata was originally included.</param>
    /// <param name="bufferLength">The buffer length.</param>
    /// <param name="mac">The authentication stream, if capture is needed.</param>
    /// <returns>Any metadata.</returns>
    Dictionary<string, string> Decrypt(
        Stream source,
        Stream target,
        byte[] userKey,
        byte[] salt,
        bool? expectMetadata,
        int bufferLength = 32768,
        Stream? mac = null);
}
