// <copyright file="HashingExtensions.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Crypt.Hashing;

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
using Crypt.Streams;

/// <summary>
/// Obtains a short byte signatures from streams.
/// </summary>
public static class HashingExtensions
{
    /// <summary>
    /// Obtains a signature from a stream.
    /// </summary>
    /// <param name="input">The stream.</param>
    /// <param name="mode">The hash mode.</param>
    /// <returns>A signature.</returns>
    public static byte[] Hash(this Stream input, HashType mode)
    {
        input.Reset();
        using var algo = ToAlgo(mode);
        return algo.ComputeHash(input);
    }

    /// <summary>
    /// Obtains a signature from a byte array.
    /// </summary>
    /// <param name="input">The byte array.</param>
    /// <param name="mode">The hash mode.</param>
    /// <returns>A signature.</returns>
    public static byte[] Hash(this byte[] input, HashType mode)
    {
        using var algo = ToAlgo(mode);
        return algo.ComputeHash(input);
    }

    /// <summary>
    /// Obtains a signature from a stream (when you're in a hurry). Results
    /// in only the most cursory values and is ludicrously easy to reverse
    /// engineer. Do not use this in any context where security matters.
    /// </summary>
    /// <param name="input">The byte array.</param>
    /// <param name="mode">The hash mode.</param>
    /// <param name="reads">The number of distributed reads.</param>
    /// <param name="chunkSize">The size of each read.</param>
    /// <returns>A signature.</returns>
    public static byte[] HashLite(this Stream input, HashType mode, int reads = 100, int chunkSize = 4096)
    {
        input = input ?? throw new ArgumentNullException(nameof(input));
        using var algo = ToAlgo(mode);
        var seedBytes = System.Text.Encoding.UTF8.GetBytes($"{input.Length}");
        var seed = Hash(seedBytes, mode);
        var dump = new byte[seed.Length];
        algo.TransformBlock(seed, 0, seed.Length, dump, 0);

        var skipSize = (long)(input.Length / (double)reads);
        var chunk = new byte[chunkSize];
        dump = new byte[chunkSize];
        input.Reset();

        int lastRead;
        while ((lastRead = input.Read(chunk, 0, chunkSize)) != 0)
        {
            algo.TransformBlock(chunk, 0, lastRead, dump, 0);
            if (input.CanSeek)
            {
                input.Seek(skipSize, SeekOrigin.Current);
            }
            else
            {
                var toSkip = (int)skipSize;
                var reRead = -1;
                while (toSkip > 0 && reRead != 0)
                {
                    reRead = input.Read(chunk, 0, Math.Min(toSkip, chunkSize));
                    toSkip -= reRead;
                }
            }
        }

        input.Reset(true);
        algo.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
        return algo.Hash;
    }

    [SuppressMessage("Security", "CA5350:Weak algo: MD5", Justification = "Not cryptography")]
    [SuppressMessage("Security", "CA5351:Weak algo: SHA1", Justification = "Not cryptography")]
    private static HashAlgorithm ToAlgo(HashType mode)
        => mode switch
        {
            HashType.Md5 => MD5.Create(),
            HashType.Sha1 => SHA1.Create(),
            HashType.Sha256 => SHA256.Create(),
            HashType.Sha384 => SHA384.Create(),
            HashType.Sha512 => SHA512.Create(),
            _ => throw new ArgumentException($"Bad hash mode: {mode}", nameof(mode)),
        };
}
