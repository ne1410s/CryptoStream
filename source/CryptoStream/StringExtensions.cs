// <copyright file="StringExtensions.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace CryptoStream;

using System.IO;
using CryptoStream.Encoding;
using CryptoStream.Hashing;
using CryptoStream.Transform;

/// <summary>
/// Extensions for strings.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Creates a hash of the supplied string.
    /// </summary>
    /// <param name="str">The string to hash.</param>
    /// <param name="mode">The hash mode.</param>
    /// <returns>A hash.</returns>
    public static byte[] Hash(this string str, HashType mode)
        => str.Decode(Codec.CharUtf8).Hash(mode);

    /// <summary>
    /// Encrypts a string to base 64 text.
    /// </summary>
    /// <param name="str">The source string.</param>
    /// <param name="password">The password.</param>
    /// <param name="saltBase64">The salt as base 64.</param>
    /// <param name="encryptor">The encryptor.</param>
    /// <param name="bufferLength">The buffer length.</param>
    /// <returns>The encrypted base 64.</returns>
    public static string Encrypt(
        this string str,
        string password,
        out string saltBase64,
        IEncryptor? encryptor = null,
        int bufferLength = 32768)
    {
        encryptor ??= new AesGcmEncryptor();
        var userKey = password.Decode(Codec.CharUtf8);

        using var srcStream = new MemoryStream(str.Decode(Codec.CharUtf8));
        using var trgStream = new MemoryStream();
        saltBase64 = encryptor.Encrypt(srcStream, trgStream, userKey, [], bufferLength).Encode(Codec.ByteBase64);
        return trgStream.ToArray().Encode(Codec.ByteBase64);
    }

    /// <summary>
    /// Decrypts a base 64 encrypted string.
    /// </summary>
    /// <param name="strBase64">The encrypted base 64 string.</param>
    /// <param name="password">The password.</param>
    /// <param name="saltBase64">The salt base 64.</param>
    /// <param name="decryptor">The decryptor.</param>
    /// <param name="bufferLength">The buffer length.</param>
    /// <returns>The decrypted string.</returns>
    public static string Decrypt(
        this string strBase64,
        string password,
        string saltBase64,
        IDecryptor? decryptor = null,
        int bufferLength = 32768)
    {
        decryptor ??= new AesGcmDecryptor();
        var userKey = password.Decode(Codec.CharUtf8);
        var salt = saltBase64.Decode(Codec.ByteBase64);

        using var srcStream = new MemoryStream(strBase64.Decode(Codec.ByteBase64));
        using var trgStream = new MemoryStream();
        decryptor.Decrypt(srcStream, trgStream, userKey, salt, false, bufferLength);
        return trgStream.ToArray().Encode(Codec.CharUtf8);
    }
}
