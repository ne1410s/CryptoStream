// <copyright file="IGcmEncryptor.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace CryptoStream.Transform;

using System.Collections.Generic;
using System.IO;

/// <summary>
/// Encrypts a stream of contiguous bytes (end to end).
/// </summary>
public interface IGcmEncryptor : IEncryptor
{
    /// <summary>
    /// Generates a pepper from a stream.
    /// </summary>
    /// <param name="input">The stream.</param>
    /// <returns>A pepper.</returns>
    byte[] GeneratePepper(Stream input);

    /// <summary>
    /// Generates a salt from a stream.
    /// </summary>
    /// <param name="input">The stream.</param>
    /// <param name="key">The key.</param>
    /// <returns>A salt.</returns>
    byte[] GenerateSalt(Stream input, byte[] key);

    /// <summary>
    /// Encrypts a block.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="cryptoKey">The cryptographic key.</param>
    /// <param name="counter">The counter.</param>
    /// <returns>An encrypted block.</returns>
    GcmEncryptedBlock EncryptBlock(byte[] source, byte[] cryptoKey, byte[] counter);

    /// <summary>
    /// Obtains suitable byte representation of metadata.
    /// </summary>
    /// <param name="metadata">The input data.</param>
    /// <param name="originalLength">The original length.</param>
    /// <param name="pepper">The pepper.</param>
    /// <param name="userKey">The user key.</param>
    /// <returns>The metadata bytes.</returns>
    byte[] GetMetaBytes(
        Dictionary<string, string> metadata,
        long originalLength,
        byte[] pepper,
        byte[] userKey);
}
