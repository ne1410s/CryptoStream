// <copyright file="DefaultKeyDeriver.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Crypt.Keying;

using System.Linq;
using Crypt.Encoding;
using Crypt.Hashing;

/// <inheritdoc cref="IKeyDeriver"/>
public class DefaultKeyDeriver : IKeyDeriver, ICryptoKeyDeriver
{
    /// <inheritdoc/>
    public byte[] DeriveCryptoKey(byte[] userKey, byte[] salt, byte[] pepper)
        => pepper.Concat(userKey).Concat(salt.Reverse())
            .ToArray()
            .Hash(HashType.Sha256);

    /// <inheritdoc/>
    public byte[] DeriveKey(string seed, params byte[][] hashes)
    {
        var hexHashes = hashes
            .Select(k => k.Encode(Codec.ByteHex))
            .OrderBy(s => s);

        foreach (var hexHash in hexHashes)
        {
            seed = (hexHash + seed)
                .Decode(Codec.CharUtf8)
                .Hash(HashType.Sha1)
                .Encode(Codec.ByteBase64);
        }

        return seed
            .Decode(Codec.CharUtf8)
            .Hash(HashType.Sha1);
    }
}
