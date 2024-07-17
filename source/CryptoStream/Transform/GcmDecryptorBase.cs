// <copyright file="GcmDecryptorBase.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace CryptoStream.Transform;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using CryptoStream.Encoding;
using CryptoStream.Hashing;
using CryptoStream.Keying;
using CryptoStream.Streams;
using CryptoStream.Utils;

/// <inheritdoc cref="IGcmDecryptor"/>
/// <summary>
/// Initializes a new instance of the <see cref="GcmDecryptorBase"/> class.
/// </summary>
/// <param name="keyDeriver">The key deriver.</param>
/// <param name="resizer">An array resizer.</param>
public abstract class GcmDecryptorBase(
    ICryptoKeyDeriver keyDeriver,
    IArrayResizer resizer) : IGcmDecryptor
{
    private const int MetaPadding = 4096;
    private const int NoMetaPadding = 128;
    private const string ReservedPrefix = nameof(GcmEncryptorBase) + "_";

    /// <inheritdoc/>
    public abstract byte[] DecryptBlock(
        GcmEncryptedBlock block,
        byte[] cryptoKey,
        byte[] counter,
        bool authenticate);

    /// <inheritdoc/>
    public Dictionary<string, string> Decrypt(
        Stream source,
        Stream target,
        byte[] userKey,
        byte[] salt,
        bool? expectMetadata,
        int bufferLength = 32768,
        Stream? mac = null)
    {
        source = source ?? throw new ArgumentNullException(nameof(source));
        target = target ?? throw new ArgumentNullException(nameof(target));

        var macBuffer = new byte[16];
        var srcBuffer = new byte[bufferLength];
        var pepper = this.ReadPepper(source, userKey, expectMetadata, out var originalLength, out var metadata);

        var cryptoKey = keyDeriver.DeriveCryptoKey(userKey, salt, pepper);
        var totalBlocks = (long)Math.Ceiling(originalLength / (double)bufferLength);
        mac?.Reset();

        target.SetLength(0);
        foreach (var b in Enumerable.Range(0, (int)totalBlocks))
        {
            mac?.Read(macBuffer, 0, macBuffer.Length);
            var position = source.Position;
            var maxReadSize = Math.Min(originalLength - position, srcBuffer.Length);
            var readSize = source.Read(srcBuffer, 0, (int)maxReadSize);
            var blockNumber = 1 + (long)Math.Floor((double)position / srcBuffer.Length);
            var counter = blockNumber.RaiseBits();
            if (readSize < srcBuffer.Length)
            {
                resizer.Resize(ref srcBuffer, readSize);
            }

            var block = new GcmEncryptedBlock(srcBuffer, macBuffer);
            var trgBuffer = this.DecryptBlock(block, cryptoKey, counter, mac != null);
            target.Write(trgBuffer, 0, trgBuffer.Length);
        }

        target.Reset();
        return metadata;
    }

    /// <inheritdoc/>
    public byte[] ReadPepper(
        Stream input,
        byte[] userKey,
        bool? expectMetadata,
        out long originalLength,
        out Dictionary<string, string> metadata)
    {
        input = input ?? throw new ArgumentNullException(nameof(input));

        var useMetaPadding = expectMetadata ?? input.Length > MetaPadding;
        var paddingToUse = useMetaPadding ? MetaPadding : NoMetaPadding;
        var metaBytes = new byte[paddingToUse];
        input.Seek(-paddingToUse, SeekOrigin.End);
        input.Read(metaBytes, 0, metaBytes.Length);
        input.Reset();

        var blockKey = userKey.Hash(HashType.Sha256);
        var churn = this.DecryptBlock(new(metaBytes, []), blockKey, 1L.RaiseBits(), false);
        var metaString = churn.Encode(Codec.CharUtf8).Trim();
        var collection = HttpUtility.ParseQueryString(metaString);
        var lengthText = collection[ReservedPrefix + "length"] ?? throw new InvalidDataException();
        originalLength = long.Parse(lengthText);
        metadata = collection.AllKeys
            .Where(key => !key.StartsWith(ReservedPrefix, StringComparison.OrdinalIgnoreCase))
            .ToDictionary(key => key, key => collection[key]);
        return collection[ReservedPrefix + "pepper"].Decode(Codec.ByteBase64);
    }
}
