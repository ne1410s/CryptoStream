﻿// <copyright file="CryptoBlockReadStream.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace CryptoStream.Streams;

using System.Collections.ObjectModel;
using System.IO;
using CryptoStream.IO;
using CryptoStream.Keying;
using CryptoStream.Transform;

/// <summary>
/// Provides a stream for reading a cryptographic file.
/// </summary>
public class CryptoBlockReadStream : BlockReadStream
{
    private readonly long originalLength;
    private readonly byte[] cryptoKey;
    private readonly IGcmDecryptor decryptor;

    /// <summary>
    /// Initializes a new instance of the <see cref="CryptoBlockReadStream"/> class.
    /// </summary>
    /// <param name="fi">The source file.</param>
    /// <param name="key">The key.</param>
    /// <param name="bufferSize">The buffer size.</param>
    /// <param name="decryptor">Decryptor override (optional).</param>
    public CryptoBlockReadStream(FileInfo fi, byte[] key, int bufferSize = 32768, IGcmDecryptor? decryptor = null)
        : this(
            new FileStream(fi.NotNull().FullName, FileMode.Open, FileAccess.Read),
            fi.ToSalt(),
            key,
            true,
            bufferSize,
            decryptor)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="CryptoBlockReadStream"/> class.
    /// </summary>
    /// <param name="stream">The source stream.</param>
    /// <param name="salt">The salt.</param>
    /// <param name="userKey">The key.</param>
    /// <param name="expectMetadata">Whether metadata was originally included.</param>
    /// <param name="bufferSize">The buffer size.</param>
    /// <param name="decryptor">Decryptor override (optional).</param>
    public CryptoBlockReadStream(
        Stream stream,
        byte[] salt,
        byte[] userKey,
        bool? expectMetadata,
        int bufferSize = 32768,
        IGcmDecryptor? decryptor = null)
        : base(stream, bufferSize)
    {
        this.decryptor = decryptor ?? new AesGcmDecryptor();
        var pepper = this.decryptor.ReadPepper(
            stream, userKey, expectMetadata, out this.originalLength, out var metadata);
        this.cryptoKey = new DefaultKeyDeriver().DeriveCryptoKey(userKey, salt, pepper);
        this.Metadata = new(metadata ?? []);
    }

    /// <inheritdoc/>
    public override long Length => this.originalLength;

    /// <summary>
    /// Gets metadata.
    /// </summary>
    public ReadOnlyDictionary<string, string> Metadata { get; }

    /// <inheritdoc/>
    protected override byte[] MapBlock(byte[] inputBuffer, long blockNo)
    {
        var counter = blockNo.RaiseBits();
        var encryptedBlock = new GcmEncryptedBlock(inputBuffer, []);
        return this.decryptor.DecryptBlock(encryptedBlock, this.cryptoKey, counter, false);
    }
}
