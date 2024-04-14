// <copyright file="AesGcmDecryptor.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Crypt.Transform;

using System;
using Crypt.Keying;
using Crypt.Utils;
using Jose;

/// <summary>
/// Performs block decryption using AES-GCM.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="AesGcmDecryptor"/> class.
/// </remarks>
/// <param name="keyDeriver">Derives the crypto key.</param>
/// <param name="resizer">Resizes arrays.</param>
public class AesGcmDecryptor(
    ICryptoKeyDeriver keyDeriver = null,
    IArrayResizer resizer = null) : GcmDecryptorBase(
          keyDeriver ?? new DefaultKeyDeriver(),
          resizer ?? new ArrayResizer())
{
    /// <inheritdoc/>
    public override byte[] DecryptBlock(
        GcmEncryptedBlock block,
        byte[] cryptoKey,
        byte[] counter,
        bool authenticate)
    {
        block = block ?? throw new ArgumentNullException(nameof(block));
        return authenticate
            ? AesGcm.Decrypt(cryptoKey, counter, [], block.MessageBuffer, block.MacBuffer)
            : AesGcm.Encrypt(cryptoKey, counter, [], block.MessageBuffer)[0];
    }
}
