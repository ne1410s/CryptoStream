// <copyright file="GcmEncryptorBase.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Crypt.Transform;

using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Crypt.Hashing;
using Crypt.Keying;
using Crypt.Streams;
using Crypt.Utils;

/// <inheritdoc cref="IGcmEncryptor"/>
/// <summary>
/// Initializes a new instance of the <see cref="GcmEncryptorBase"/> class.
/// </summary>
/// <param name="keyDeriver">The key deriver.</param>
/// <param name="resizer">An array resizer.</param>
public abstract class GcmEncryptorBase(
    ICryptoKeyDeriver keyDeriver,
    IArrayResizer resizer) : IGcmEncryptor
{
    private const int PepperLength = 32;

    /// <inheritdoc/>
    public abstract GcmEncryptedBlock EncryptBlock(byte[] source, byte[] cryptoKey, byte[] counter);

    /// <inheritdoc/>
    public byte[] Encrypt(
        Stream input,
        Stream output,
        byte[] userKey,
        int bufferLength = 32768,
        Stream mac = null)
    {
        input = input ?? throw new ArgumentNullException(nameof(input));
        output = output ?? throw new ArgumentNullException(nameof(output));

        var counter = new byte[12];
        var srcBuffer = new byte[bufferLength];
        var salt = this.GenerateSalt(input, userKey);
        var pepper = this.GeneratePepper(input);
        var cryptoKey = keyDeriver.DeriveCryptoKey(userKey, salt, pepper);

        int readSize;
        while ((readSize = input.Read(srcBuffer, 0, srcBuffer.Length)) != 0)
        {
            ByteExtensions.Increment(ref counter);
            if (readSize < srcBuffer.Length)
            {
                resizer.Resize(ref srcBuffer, readSize);
            }

            var result = this.EncryptBlock(srcBuffer, cryptoKey, counter);
            if (input == output)
            {
                input.Position -= readSize;
            }

            mac?.Write(result.MacBuffer, 0, result.MacBuffer.Length);
            output.Write(result.MessageBuffer, 0, result.MessageBuffer.Length);
        }

        var sizeBlock = this.EncryptBlock(input.Length.RaiseBits(), cryptoKey, 1L.RaiseBits()).MessageBuffer;
        var padSize = StreamBlockUtils.GetPadSize(input.Length, sizeBlock.Length + pepper.Length);
        var padding = new byte[padSize - (input.Length + sizeBlock.Length + pepper.Length)];
        using var rng = RandomNumberGenerator.Create();
        rng.GetNonZeroBytes(padding);
        output.Write(padding, 0, padding.Length);
        output.Write(sizeBlock, 0, sizeBlock.Length);
        output.Write(pepper, 0, pepper.Length);
        return salt;
    }

    /// <inheritdoc/>
    public byte[] GeneratePepper(Stream input)
    {
        using var rng = RandomNumberGenerator.Create();
        var pepper = new byte[PepperLength];
        rng.GetNonZeroBytes(pepper);
        return pepper;
    }

    /// <inheritdoc/>
    public byte[] GenerateSalt(Stream input, byte[] key)
    {
        var salt = input.Hash(HashType.Sha256);
        input.Reset();
        Array.Reverse(salt, 0, 8);
        Array.Reverse(salt, 5, salt.Length - 5);
        Array.Reverse(salt);
        return salt.Concat(key).ToArray().Hash(HashType.Sha256);
    }
}
