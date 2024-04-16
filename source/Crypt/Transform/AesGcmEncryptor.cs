// <copyright file="AesGcmEncryptor.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Crypt.Transform;

using Crypt.Keying;
using Crypt.Utils;
using Jose;

/// <summary>
/// Performs block encryption using AES-GCM.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="AesGcmEncryptor"/> class.
/// </remarks>
/// <param name="keyDeriver">Derives a crypto key.</param>
/// <param name="resizer">Resizes arrays.</param>
public class AesGcmEncryptor(
    ICryptoKeyDeriver? keyDeriver = null,
    IArrayResizer? resizer = null) : GcmEncryptorBase(
          keyDeriver ?? new DefaultKeyDeriver(),
          resizer ?? new ArrayResizer())
{
    /// <inheritdoc/>
    public override GcmEncryptedBlock EncryptBlock(byte[] source, byte[] cryptoKey, byte[] counter)
    {
        var outputBuffers = AesGcm.Encrypt(cryptoKey, counter, [], source);
        return new GcmEncryptedBlock(outputBuffers[0], outputBuffers[1]);
    }
}
