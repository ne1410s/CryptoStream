// <copyright file="IKeyDeriver.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Crypt.Keying
{
    /// <summary>
    /// Derives keys for cryptographic processes.
    /// </summary>
    public interface IKeyDeriver
    {
        /// <summary>
        /// Derives a key based on a seed and an array of hashes. The order of
        /// hashes does not affect the outcome.
        /// </summary>
        /// <param name="seed">A seed.</param>
        /// <param name="hashes">An array of hashes.</param>
        /// <returns>The derived key.</returns>
        byte[] DeriveKey(string seed, params byte[][] hashes);
    }
}
