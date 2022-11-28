// <copyright file="AesGcmDecryptor.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Crypt.Transform
{
    using System;
    using Crypt.Keying;
    using Crypt.Utils;
    using Jose;

    /// <summary>
    /// Performs block decryption using AES-GCM.
    /// </summary>
    public class AesGcmDecryptor : GcmDecryptorBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AesGcmDecryptor"/> class.
        /// </summary>
        /// <param name="keyDeriver">Derives the crypto key.</param>
        /// <param name="resizer">Resizes arrays.</param>
        public AesGcmDecryptor(
            ICryptoKeyDeriver keyDeriver = null,
            IArrayResizer resizer = null)
            : base(
                  keyDeriver ?? new DefaultKeyDeriver(),
                  resizer ?? new ArrayResizer())
        { }

        /// <inheritdoc/>
        public override byte[] DecryptBlock(
            GcmEncryptedBlock block,
            byte[] cryptoKey,
            byte[] counter,
            bool authenticate) => authenticate
                ? AesGcm.Decrypt(cryptoKey, counter, Array.Empty<byte>(), block.MessageBuffer, block.MacBuffer)
                : AesGcm.Encrypt(cryptoKey, counter, Array.Empty<byte>(), block.MessageBuffer)[0];
    }
}
