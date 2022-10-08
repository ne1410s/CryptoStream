// <copyright file="HashType.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Crypt.Hashing
{
    /// <summary>
    /// Hash type.
    /// </summary>
    public enum HashType
    {
        /// <summary>
        /// MD5 hash.
        /// </summary>
        Md5 = 1,

        /// <summary>
        /// SHA1 hash.
        /// </summary>
        Sha1 = 2,

        /// <summary>
        /// SHA256 hash.
        /// </summary>
        Sha256 = 3,

        /// <summary>
        /// SHA384 hash.
        /// </summary>
        Sha384 = 4,

        /// <summary>
        /// SHA512 hash.
        /// </summary>
        Sha512 = 5,
    }
}
