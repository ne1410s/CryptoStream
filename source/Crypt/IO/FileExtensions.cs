// <copyright file="FileExtensions.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Crypt.IO;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Crypt.Encoding;
using Crypt.Hashing;
using Crypt.Transform;

/// <summary>
/// Extensions for <see cref="FileInfo"/>.
/// </summary>
public static class FileExtensions
{
    private const string AlreadySecureMessage = "File already appears to be secure. "
        + "If you wish to secure it, please rename it first";

    private static readonly Regex SaltRegex = new(
        @"\b(?<hex>[a-f0-9]{64})(?<ext>\.[\w-]+){0,1}$",
        RegexOptions.Compiled);

    private static readonly Regex PlainExtRegex = new(
        @"^\.[a-zA-Z0-9]{1,5}$",
        RegexOptions.Compiled);

    private static readonly Regex SecureExtRegex = new(
        @"^\.[a-f0-9]{10}$",
        RegexOptions.Compiled);

    /// <summary>
    /// Gets whether the file appears to be secure.
    /// </summary>
    /// <param name="fi">The file info.</param>
    /// <returns>Whether the file appears secure.</returns>
    public static bool IsSecure(this FileInfo fi) => SaltRegex.IsMatch(fi?.Name);

    /// <summary>
    /// Gets a salt.
    /// </summary>
    /// <param name="fi">The file info.</param>
    /// <returns>A salt.</returns>
    /// <exception cref="ArgumentException">File suitability.</exception>
    public static byte[] ToSalt(this FileInfo fi)
    {
        var match = SaltRegex.Match((fi ?? throw new ArgumentNullException(nameof(fi))).Name);
        return match.Success
            ? match.Groups["hex"].Value.Decode(Codec.ByteHex)
            : throw new ArgumentException(
                $"Unable to obtain salt: '{fi!.Name}'",
                nameof(fi));
    }

    /// <summary>
    /// Gets a hash.
    /// </summary>
    /// <param name="fi">The file info.</param>
    /// <param name="mode">The hash mode.</param>
    /// <returns>A hash.</returns>
    public static byte[] Hash(this FileInfo fi, HashType mode)
    {
        using var stream = (fi ?? throw new ArgumentNullException(nameof(fi))).OpenRead();
        return stream.Hash(mode);
    }

    /// <summary>
    /// Gets a hash (when you're in a hurry). This is not suitable in any
    /// context where security matters; extremely easy to reverse engineer.
    /// </summary>
    /// <param name="fi">The file info.</param>
    /// <param name="mode">The mode.</param>
    /// <param name="reads">The reads.</param>
    /// <param name="chunkSize">The chunk size.</param>
    /// <returns>A weak hash.</returns>
    public static byte[] HashLite(this FileInfo fi, HashType mode, int reads = 100, int chunkSize = 4096)
    {
        using var stream = (fi ?? throw new ArgumentNullException(nameof(fi))).OpenRead();
        return stream.HashLite(mode, reads, chunkSize);
    }

    /// <summary>
    /// Decrypts a file to a separate stream.
    /// </summary>
    /// <param name="fi">The file.</param>
    /// <param name="target">The target stream.</param>
    /// <param name="userKey">The user key.</param>
    /// <param name="decryptor">The decryptor.</param>
    /// <param name="bufferLength">The buffer length.</param>
    /// <param name="mac">The mac (optional).</param>
    /// <returns>Original metadata.</returns>
    public static Dictionary<string, string> DecryptTo(
        this FileInfo fi,
        Stream target,
        byte[] userKey,
        IDecryptor? decryptor = null,
        int bufferLength = 32768,
        Stream? mac = null)
    {
        decryptor ??= new AesGcmDecryptor();
        var salt = fi.ToSalt();

        using var stream = fi.OpenRead();
        return decryptor.Decrypt(stream, target, userKey, salt, bufferLength, mac);
    }

    /// <summary>
    /// Decrypts a file by making a copy under the source directory.
    /// </summary>
    /// <param name="fi">The file.</param>
    /// <param name="userKey">The user key.</param>
    /// <param name="decryptor">The decryptor.</param>
    /// <param name="bufferLength">The buffer length.</param>
    /// <param name="mac">The mac (optional).</param>
    /// <returns>Newly-generated file information.</returns>
    public static FileInfo DecryptHere(
        this FileInfo fi,
        byte[] userKey,
        IDecryptor? decryptor = null,
        int bufferLength = 32768,
        Stream? mac = null)
    {
        var randomHex = Guid.NewGuid().ToString().Substring(0, 8);
        var tempFile = new FileInfo($"{fi}.{randomHex}.dec");
        string extension;
        using (var target = tempFile.OpenWrite())
        {
            var metadata = fi.DecryptTo(target, userKey, decryptor, bufferLength, mac);
            extension = new FileInfo(metadata["filename"]).Extension;
        }

        var fileName = $"{tempFile.Name.Substring(0, 12)}.{randomHex}{extension}";
        tempFile.MoveTo(Path.Combine(tempFile.DirectoryName, fileName));
        return tempFile;
    }

    /// <summary>
    /// Encrypts a file in its current location. Caution: the bytes are
    /// progressively overwritten in a non-transactional and irrevocable way.
    /// </summary>
    /// <param name="fi">The file.</param>
    /// <param name="userKey">The user key.</param>
    /// <param name="encryptor">The encryptor.</param>
    /// <param name="bufferLength">The buffer length.</param>
    /// <param name="mac">The mac.</param>
    /// <returns>The salt hex.</returns>
    public static string EncryptInSitu(
        this FileInfo fi,
        byte[] userKey,
        IGcmEncryptor? encryptor = null,
        int bufferLength = 32768,
        Stream? mac = null)
    {
        encryptor ??= new AesGcmEncryptor();

        var isSecure = fi?.IsSecure() ?? throw new ArgumentNullException(nameof(fi));
        if (isSecure)
        {
            throw new ArgumentException(AlreadySecureMessage, nameof(fi));
        }

        string saltHex;
        var metadata = new Dictionary<string, string>() { ["filename"] = fi.Name };
        using (var stream = fi.Open(FileMode.Open))
        {
            saltHex = encryptor.Encrypt(stream, stream, userKey, metadata, bufferLength, mac)
                .Encode(Codec.ByteHex)
                .ToLowerInvariant();
        }

        var secureExt = new FileInfo(saltHex).ToSecureExtension(fi.Extension, encryptor);
        var target = Path.Combine(fi.DirectoryName, saltHex + secureExt);
        if (target != fi.FullName)
        {
            File.Delete(target);
            fi.MoveTo(target);
        }

        return saltHex;
    }

    /// <summary>
    /// Gets a secure extension.
    /// </summary>
    /// <param name="secure">A secure file.</param>
    /// <param name="plainExtension">The original extension.</param>
    /// <param name="encryptor">The encryptor.</param>
    /// <returns>A secure extension to use.</returns>
    /// <exception cref="ArgumentException">Bad name format.</exception>
    public static string ToSecureExtension(this FileInfo secure, string plainExtension, IGcmEncryptor? encryptor = null)
    {
        encryptor ??= new AesGcmEncryptor();
        plainExtension = plainExtension ?? throw new ArgumentNullException(nameof(plainExtension));

        if (!PlainExtRegex.IsMatch(plainExtension))
        {
            throw new ArgumentException("Unable to parse file data.", nameof(plainExtension));
        }

        var salt = secure.ToSalt();
        var working = plainExtension.TrimStart('.').PadLeft(5, ' ').Decode(Codec.CharUtf8);
        var buffer = encryptor.EncryptBlock(working, salt, 1L.RaiseBits()).MessageBuffer;
        return '.' + buffer.Encode(Codec.ByteHex);
    }

    /// <summary>
    /// Gets a plain extension.
    /// </summary>
    /// <param name="secure">A secure file.</param>
    /// <param name="decryptor">The encryptor.</param>
    /// <returns>A plain extension.</returns>
    /// <exception cref="ArgumentException">File suitability.</exception>
    public static string ToPlainExtension(this FileInfo secure, IGcmDecryptor? decryptor = null)
    {
        decryptor ??= new AesGcmDecryptor();
        var salt = secure.ToSalt();
        var secureExtension = secure.Extension;
        if (!SecureExtRegex.IsMatch(secureExtension))
        {
            throw new ArgumentException("Unable to parse file data.", nameof(secure));
        }

        var buffer = secure.Extension.TrimStart('.').Decode(Codec.ByteHex);
        var working = decryptor.DecryptBlock(new(buffer, []), salt, 1L.RaiseBits(), false);
        return '.' + working.Encode(Codec.CharUtf8).Trim();
    }
}
