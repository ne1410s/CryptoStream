// <copyright file="CryptoBlockWriteStream.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace CryptoStream.Streams;

using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using CryptoStream.Encoding;
using CryptoStream.IO;
using CryptoStream.Keying;
using CryptoStream.Transform;
using CryptoStream.Utils;

/// <summary>
/// Provides a stream for writing a cryptographic file.
/// </summary>
public class CryptoBlockWriteStream : BlockWriteStream
{
    private readonly byte[] userKey;
    private readonly byte[] cryptoKey;
    private readonly byte[] pepper;
    private readonly IGcmEncryptor encryptor;
    private readonly Dictionary<string, string> metadata;

    /// <summary>
    /// Initializes a new instance of the <see cref="CryptoBlockWriteStream"/> class.
    /// </summary>
    /// <param name="target">The target stream.</param>
    /// <param name="metadata">Any metadata.</param>
    /// <param name="salt">The salt generated in source encryption.</param>
    /// <param name="userKey">The key.</param>
    /// <param name="bufferSize">The buffer size.</param>
    /// <param name="encryptor">Encryptor override (optional).</param>
    public CryptoBlockWriteStream(
        Stream target,
        Dictionary<string, string> metadata,
        byte[] salt,
        byte[] userKey,
        int bufferSize = 32768,
        IGcmEncryptor? encryptor = null)
            : base(target, bufferSize)
    {
        this.metadata = metadata;
        this.userKey = userKey;
        this.encryptor = encryptor ?? new AesGcmEncryptor();
        this.pepper = this.encryptor.GeneratePepper(null!);
        this.cryptoKey = new DefaultKeyDeriver().DeriveCryptoKey(userKey, salt, this.pepper);

        var ext = Path.GetExtension(metadata.NotNull()["filename"]);
        var saltHex = salt.Encode(Codec.ByteHex);
        var secureExt = new FileInfo(saltHex).ToSecureExtension(ext, encryptor);
        this.Name = saltHex + secureExt;
    }

    /// <summary>
    /// Gets a suggested name.
    /// </summary>
    public string Name { get; }

    /// <inheritdoc/>
    public override void WriteFinal()
    {
        var originalLength = this.Length;
        var metaBytes = this.encryptor.GetMetaBytes(this.metadata, originalLength, this.pepper, this.userKey);
        var padSize = StreamBlockUtils.GetPadSize(originalLength, metaBytes.Length);
        var randomBytes = new byte[padSize - (originalLength + metaBytes.Length)];
        using var rng = RandomNumberGenerator.Create();
        rng.GetNonZeroBytes(randomBytes);

        this.Inner.Write(randomBytes, 0, randomBytes.Length);
        this.Inner.Write(metaBytes, 0, metaBytes.Length);
    }

    /// <inheritdoc/>
    protected override byte[] MapBlock(byte[] inputBuffer, long blockNo)
    {
        var counter = blockNo.RaiseBits();
        var encryptedBlock = this.encryptor.EncryptBlock(inputBuffer, this.cryptoKey, counter);
        return encryptedBlock.MessageBuffer;
    }
}
