// <copyright file="IEncryptor.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Crypt.Transform;

using System.Collections.Generic;
using System.IO;

/// <summary>
/// Encrypts a stream of contiguous bytes (end to end).
/// </summary>
public interface IEncryptor
{
    /// <summary>
    /// Encrypts a stream.
    /// </summary>
    /// <param name="input">The input stream.</param>
    /// <param name="output">The output stream.</param>
    /// <param name="userKey">The originally supplied key.</param>
    /// <param name="metadata">Any metadata.</param>
    /// <param name="bufferLength">The buffer length.</param>
    /// <param name="mac">The authentication stream, if capture is needed.</param>
    /// <returns>The salt.</returns>
    byte[] Encrypt(
        Stream input,
        Stream output,
        byte[] userKey,
        Dictionary<string, string> metadata,
        int bufferLength = 32768,
        Stream mac = null);
}
