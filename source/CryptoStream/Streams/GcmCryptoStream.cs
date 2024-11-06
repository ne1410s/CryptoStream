// <copyright file="GcmCryptoStream.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace CryptoStream.Streams;

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using CryptoStream.Encoding;
using CryptoStream.IO;
using CryptoStream.Keying;
using CryptoStream.Transform;
using CryptoStream.Utils;

/// <summary>
/// A block stream that transforms buffer reads and writes according to gcm.
/// </summary>
public class GcmCryptoStream : BlockStream
{
    private readonly long? length;
    private readonly AesGcmDecryptor decryptor = new();
    private readonly AesGcmEncryptor encryptor = new();
    private readonly byte[] userKey;
    private readonly byte[] cryptoKey;
    private readonly byte[] pepper;

    /// <summary>
    /// Initializes a new instance of the <see cref="GcmCryptoStream"/> class.
    /// Use this constructor for <b>read</b> operations.
    /// </summary>
    /// <param name="stream">The underlying stream.</param>
    /// <param name="salt">The salt.</param>
    /// <param name="key">The key.</param>
    /// <param name="bufferLength">The buffer length.</param>
    public GcmCryptoStream(Stream stream, byte[] salt, byte[] key, int bufferLength = 32768)
        : base(stream, bufferLength)
    {
        this.userKey = key;
        this.pepper = this.decryptor.ReadPepper(stream, key, true, out var originalLength, out var metadata);
        this.length = originalLength;
        this.cryptoKey = new DefaultKeyDeriver().DeriveCryptoKey(key, salt, this.pepper);
        this.Metadata = metadata;
        this.Id = salt.Encode(Codec.ByteHex);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GcmCryptoStream"/> class.
    /// Use this constructor for <b>write</b> operations.
    /// </summary>
    /// <param name="stream">The underlying stream.</param>
    /// <param name="salt">The salt.</param>
    /// <param name="key">The key.</param>
    /// <param name="ext">The target extension.</param>
    /// <param name="bufferLength">The buffer length.</param>
    public GcmCryptoStream(
        Stream stream, byte[] salt, byte[] key, string ext, int bufferLength = 32768)
            : base(stream, bufferLength)
    {
        this.userKey = key;
        this.pepper = this.encryptor.GeneratePepper(null!);
        this.cryptoKey = new DefaultKeyDeriver().DeriveCryptoKey(key, salt, this.pepper);
        this.Metadata = new() { ["filename"] = "_" + ext };
        var saltHex = salt.Encode(Codec.ByteHex);
        var secureExt = new FileInfo(saltHex).ToSecureExtension(ext, this.encryptor);
        this.Id = saltHex + secureExt;
    }

    /// <summary>
    /// Gets metadata.
    /// </summary>
    public Dictionary<string, string> Metadata { get; }

    /// <inheritdoc/>
    public override long Length => this.length ?? this.Inner.Length;

    /// <inheritdoc/>
    public override void FinaliseWrite()
    {
        base.FinaliseWrite();

        var originalLength = this.Length;
        var metaBytes = this.encryptor.GetMetaBytes(this.Metadata, originalLength, this.pepper, this.userKey);
        var padSize = StreamBlockUtils.GetPadSize(originalLength, metaBytes.Length);
        var randomBytes = new byte[padSize - (originalLength + metaBytes.Length)];
        using var rng = RandomNumberGenerator.Create();
        rng.GetNonZeroBytes(randomBytes);

        this.Inner.Write(randomBytes, 0, randomBytes.Length);
        this.Inner.Write(metaBytes, 0, metaBytes.Length);
    }

    /// <inheritdoc/>
    protected override void TransformBufferForRead(long blockNo)
    {
        var counter = blockNo.RaiseBits();
        var encryptedBlock = new GcmEncryptedBlock(this.BlockBuffer, []);
        var result = this.decryptor.DecryptBlock(encryptedBlock, this.cryptoKey, counter, false);
        Array.Copy(result, this.BlockBuffer, this.BufferLength);
    }

    /// <inheritdoc/>
    protected override void TransformBufferForWrite(long blockNo)
    {
        var counter = blockNo.RaiseBits();
        var encryptedBlock = this.encryptor.EncryptBlock(this.BlockBuffer, this.cryptoKey, counter);
        Array.Copy(encryptedBlock.MessageBuffer, this.BlockBuffer, this.BufferLength);
    }
}
