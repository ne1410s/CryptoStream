// <copyright file="IGcmDecryptor.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace CryptoStream.Transform;

using System.Collections.Generic;
using System.IO;

/// <summary>
/// Decrypts a stream of contiguous bytes (end to end).
/// </summary>
public interface IGcmDecryptor : IDecryptor
{
    /// <summary>
    /// Reads the pepper from a stream.
    /// </summary>
    /// <param name="input">The stream.</param>
    /// <param name="userKey">The user key.</param>
    /// <param name="expectMetadata">Whether metadata was originally included.</param>
    /// <param name="originalLength">The original length.</param>
    /// <param name="metadata">The metadata.</param>
    /// <returns>The pepper.</returns>
    byte[] ReadPepper(
        Stream input,
        byte[] userKey,
        bool? expectMetadata,
        out long originalLength,
        out Dictionary<string, string> metadata);

    /// <summary>
    /// Decrypts a block.
    /// </summary>
    /// <param name="block">The encrypted block.</param>
    /// <param name="cryptoKey">The cryptographic key.</param>
    /// <param name="counter">The counter.</param>
    /// <param name="authenticate">Whether to authenticate with mac.</param>
    /// <returns>A block of decrypted bytes.</returns>
    byte[] DecryptBlock(
        GcmEncryptedBlock block,
        byte[] cryptoKey,
        byte[] counter,
        bool authenticate);
}
