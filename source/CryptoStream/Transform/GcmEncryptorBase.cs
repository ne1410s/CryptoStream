// <copyright file="GcmEncryptorBase.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace CryptoStream.Transform;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using CryptoStream.Encoding;
using CryptoStream.Hashing;
using CryptoStream.Keying;
using CryptoStream.Streams;
using CryptoStream.Utils;

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
    private const int Padding = 4096;
    private const string ReservedPrefix = nameof(GcmEncryptorBase) + "_";

    /// <inheritdoc/>
    public abstract GcmEncryptedBlock EncryptBlock(byte[] source, byte[] cryptoKey, byte[] counter);

    /// <inheritdoc/>
    public byte[] Encrypt(
        Stream source,
        Stream target,
        byte[] userKey,
        Dictionary<string, string> metadata,
        int bufferLength = 32768,
        Stream? mac = null)
    {
        metadata ??= [];
        source = source ?? throw new ArgumentNullException(nameof(source));
        target = target ?? throw new ArgumentNullException(nameof(target));

        var counter = new byte[12];
        var srcBuffer = new byte[bufferLength];
        var salt = this.GenerateSalt(source, userKey);
        var pepper = this.GeneratePepper(source);
        var cryptoKey = keyDeriver.DeriveCryptoKey(userKey, salt, pepper);
        var metaBytes = this.GetMetaBytes(metadata, source.Length, pepper, userKey);

        int readSize;
        while ((readSize = source.Read(srcBuffer, 0, srcBuffer.Length)) != 0)
        {
            ByteExtensions.Increment(ref counter);
            if (readSize < srcBuffer.Length)
            {
                resizer.Resize(ref srcBuffer, readSize);
            }

            var result = this.EncryptBlock(srcBuffer, cryptoKey, counter);
            if (source == target)
            {
                source.Position -= readSize;
            }

            mac?.Write(result.MacBuffer, 0, result.MacBuffer.Length);
            target.Write(result.MessageBuffer, 0, result.MessageBuffer.Length);
        }

        var padSize = StreamBlockUtils.GetPadSize(source.Length, metaBytes.Length);
        var randomBytes = new byte[padSize - (source.Length + metaBytes.Length)];
        using var rng = RandomNumberGenerator.Create();
        rng.GetNonZeroBytes(randomBytes);
        target.Write(randomBytes, 0, randomBytes.Length);
        target.Write(metaBytes, 0, metaBytes.Length);
        return salt;
    }

    /// <inheritdoc/>
    public byte[] GeneratePepper(Stream input)
    {
        using var rng = RandomNumberGenerator.Create();
        var pepper = new byte[32];
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

    private byte[] GetMetaBytes(
        Dictionary<string, string> metadata,
        long originalLength,
        byte[] pepper,
        byte[] userKey)
    {
        var metaCollection = HttpUtility.ParseQueryString(string.Empty);
        foreach (var kvp in metadata)
        {
            metaCollection[kvp.Key] = kvp.Value;
        }

        metaCollection[ReservedPrefix + "length"] = $"{originalLength}";
        metaCollection[ReservedPrefix + "pepper"] = pepper.Encode(Codec.ByteBase64);
        var metaString = metaCollection.ToString();
        metaString = metaString.PadRight(Padding, ' ');
        var metaBytes = metaString.Decode(Codec.CharUtf8);
        if (metaBytes.Length != Padding)
        {
            throw new ArgumentException("Unexpected padding.");
        }

        var blockKey = userKey.Hash(HashType.Sha256);
        return this.EncryptBlock(metaBytes, blockKey, 1L.RaiseBits()).MessageBuffer;
    }
}
