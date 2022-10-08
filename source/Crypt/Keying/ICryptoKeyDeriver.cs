// <copyright file="ICryptoKeyDeriver.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Crypt.Keying
{
    /// <summary>
    /// Derives keys for cryptographic processes.
    /// </summary>
    public interface ICryptoKeyDeriver
    {
        /// <summary>
        /// Derives a cryptographic key for internal processes.
        /// </summary>
        /// <param name="userKey">The originally supplied key.</param>
        /// <param name="salt">The salt.</param>
        /// <param name="pepper">The pepper.</param>
        /// <returns>The derived key.</returns>
        byte[] DeriveCryptoKey(byte[] userKey, byte[] salt, byte[] pepper);
    }
}
