﻿using System.IO;

namespace Crypto.Transform
{
    /// <summary>
    /// Encrypts a stream of contiguous bytes (end to end).
    /// </summary>
    public interface IGcmEncryptor : IEncryptor
    {
        /// <summary>
        /// Generates a pepper from a stream.
        /// </summary>
        /// <param name="input">The stream.</param>
        /// <returns>A pepper.</returns>
        byte[] GeneratePepper(Stream input);

        /// <summary>
        /// Generates a salt from a stream.
        /// </summary>
        /// <param name="input">The stream.</param>
        /// <returns>A salt.</returns>
        byte[] GenerateSalt(Stream input);

        /// <summary>
        /// Encrypts a block.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="cryptoKey">The cryptographic key.</param>
        /// <param name="counter">The counter.</param>
        /// <returns>An encrypted block.</returns>
        GcmEncryptedBlock EncryptBlock(byte[] source, byte[] cryptoKey, byte[] counter);
    }
}