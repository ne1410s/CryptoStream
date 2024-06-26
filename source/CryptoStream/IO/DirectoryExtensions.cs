﻿// <copyright file="DirectoryExtensions.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace CryptoStream.IO;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CryptoStream.Hashing;
using CryptoStream.Transform;

/// <summary>
/// Extensions for <see cref="DirectoryInfo"/>.
/// </summary>
public static class DirectoryExtensions
{
    private const string Wildcard = "*";

    /// <summary>
    /// Signs a folder structure recursively. This process is not sensitive
    /// to changes in metadata.
    /// </summary>
    /// <param name="di">The directory.</param>
    /// <param name="mode">The hash mode.</param>
    /// <param name="includes">Factors contributing to uniqueness.</param>
    /// <returns>The hash sum.</returns>
    public static byte[] HashSum(
        this DirectoryInfo di, HashType mode, HashSumIncludes includes = HashSumIncludes.FileContents)
    {
        di = di ?? throw new ArgumentNullException(nameof(di));
        var hashSeed = includes.HasFlag(HashSumIncludes.DirectoryRootName) ? di.Name : string.Empty;
        var hash = hashSeed.Hash(mode);

        foreach (var fsi in di.EnumerateFileSystemInfos(Wildcard, SearchOption.AllDirectories))
        {
            var entryBytes = new List<byte>(hash);
            if (fsi is FileInfo fi)
            {
                if (includes.HasFlag(HashSumIncludes.FileContents))
                {
                    entryBytes.AddRange(fi.Hash(mode));
                }

                if (includes.HasFlag(HashSumIncludes.FileTimestamp))
                {
                    entryBytes.AddRange(fi.LastWriteTime.ToString(CultureInfo.InvariantCulture).Hash(mode));
                }
            }

            if (includes.HasFlag(HashSumIncludes.DirectoryStructure))
            {
                entryBytes.AddRange(fsi.Name.Hash(mode));
            }

            hash = entryBytes.ToArray().Hash(mode);
        }

        return hash;
    }

    /// <summary>
    /// Encrypts all unsecured files in-situ, recursively.
    /// </summary>
    /// <param name="di">The directory.</param>
    /// <param name="userKey">The user key.</param>
    /// <param name="recurse">Whether to include sub-directories.</param>
    /// <param name="where">File filter.</param>
    /// <param name="encryptor">The encryptor.</param>
    /// <param name="bufferLength">The buffer length.</param>
    public static void EncryptAllInSitu(
        this DirectoryInfo di,
        byte[] userKey,
        bool recurse = false,
        Func<FileInfo, bool>? where = null,
        IGcmEncryptor? encryptor = null,
        int bufferLength = 32768)
    {
        di = di ?? throw new ArgumentNullException(nameof(di));
        var searchOpt = recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        foreach (var fi in di.EnumerateFiles(Wildcard, searchOpt))
        {
            if (!fi.IsSecure() && (where?.Invoke(fi) ?? true))
            {
                fi.EncryptInSitu(userKey, encryptor, bufferLength);
            }
        }
    }
}
