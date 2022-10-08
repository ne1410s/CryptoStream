// <copyright file="IDecryptor.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Crypt.Transform
{
    using System.IO;

    /// <summary>
    /// Decrypts a stream of contiguous bytes (end to end).
    /// </summary>
    public interface IDecryptor
    {
        /// <summary>
        /// Decrypts a stream.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <param name="output">The output stream.</param>
        /// <param name="userKey">The originally supplied key.</param>
        /// <param name="salt">The salt generated in encryption.</param>
        /// <param name="bufferLength">The buffer length.</param>
        /// <param name="mac">The authentication stream, if capture is needed.</param>
        void Decrypt(
            Stream input,
            Stream output,
            byte[] userKey,
            byte[] salt,
            int bufferLength = 32768,
            Stream mac = null);
    }
}
