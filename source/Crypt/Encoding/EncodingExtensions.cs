// <copyright file="EncodingExtensions.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Crypt.Encoding;

using System;
using System.Globalization;
using SysEncoding = System.Text.Encoding;

/// <summary>
/// Transforms bytes to text and vice versa.
/// </summary>
public static class EncodingExtensions
{
    /// <summary>
    /// Transforms an array of bytes to text.
    /// </summary>
    /// <param name="bytes">A byte array.</param>
    /// <param name="codec">The encoding type.</param>
    /// <returns>The encoded text.</returns>
    public static string Encode(this byte[] bytes, Codec codec)
        => codec switch
        {
            Codec.ByteBase64 => Convert.ToBase64String(bytes),
            Codec.ByteHex => EncodeHex(bytes),
            Codec.CharAscii => SysEncoding.ASCII.GetString(bytes),
            Codec.CharUnicode => SysEncoding.Unicode.GetString(bytes),
            Codec.CharUtf8 => SysEncoding.UTF8.GetString(bytes),
            _ => throw new ArgumentException($"Bad codec: {codec}", nameof(codec)),
        };

    /// <summary>
    /// Transforms text to an array of bytes.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="codec">The encoding type.</param>
    /// <returns>A byte array.</returns>
    public static byte[] Decode(this string text, Codec codec)
        => codec switch
        {
            Codec.ByteBase64 => Convert.FromBase64String(text),
            Codec.ByteHex => DecodeHex(text),
            Codec.CharAscii => SysEncoding.ASCII.GetBytes(text),
            Codec.CharUnicode => SysEncoding.Unicode.GetBytes(text),
            Codec.CharUtf8 => SysEncoding.UTF8.GetBytes(text),
            _ => throw new ArgumentException($"Bad codec: {codec}", nameof(codec)),
        };

    private static byte[] DecodeHex(string hex)
    {
        var bytes = new byte[(hex ?? throw new ArgumentNullException(nameof(hex))).Length / 2];
        for (var i = 0; i < bytes.Length; i++)
        {
            bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        }

        return bytes;
    }

    private static string EncodeHex(byte[] bytes)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var b in bytes ?? throw new ArgumentNullException(nameof(bytes)))
        {
            sb.Append(b.ToString("x2", CultureInfo.InvariantCulture));
        }

        return sb.ToString();
    }
}
