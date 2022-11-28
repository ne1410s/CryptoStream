// <copyright file="GcmEncryptorBase.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Crypt.Transform
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using Crypt.Hashing;
    using Crypt.Keying;
    using Crypt.Utils;

    /// <inheritdoc cref="IGcmEncryptor"/>
    public abstract class GcmEncryptorBase : IGcmEncryptor
    {
        private readonly ICryptoKeyDeriver keyDeriver;
        private readonly IArrayResizer resizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="GcmEncryptorBase"/> class.
        /// </summary>
        /// <param name="keyDeriver">The key deriver.</param>
        /// <param name="resizer">An array resizer.</param>
        protected GcmEncryptorBase(
            ICryptoKeyDeriver keyDeriver,
            IArrayResizer resizer)
        {
            this.keyDeriver = keyDeriver;
            this.resizer = resizer;
        }

        /// <summary>
        /// Gets the pepper length.
        /// </summary>
        protected int PepperLength => 32;

        /// <inheritdoc/>
        public abstract GcmEncryptedBlock EncryptBlock(byte[] source, byte[] cryptoKey, byte[] counter);

        /// <inheritdoc/>
        public byte[] Encrypt(
            Stream input,
            Stream output,
            byte[] userKey,
            int bufferLength = 32768,
            Stream mac = null)
        {
            var counter = new byte[12];
            var srcBuffer = new byte[bufferLength];
            var salt = this.GenerateSalt(input);
            var pepper = this.GeneratePepper(input);
            var cryptoKey = this.keyDeriver.DeriveCryptoKey(userKey, salt, pepper);

            int readSize;
            while ((readSize = input.Read(srcBuffer, 0, srcBuffer.Length)) != 0)
            {
                ByteExtensions.Increment(ref counter);
                if (readSize < srcBuffer.Length)
                {
                    this.resizer.Resize(ref srcBuffer, readSize);
                }

                var result = this.EncryptBlock(srcBuffer, cryptoKey, counter);
                if (input == output)
                {
                    input.Position -= readSize;
                }

                mac?.Write(result.MacBuffer, 0, result.MacBuffer.Length);
                output.Write(result.MessageBuffer, 0, result.MessageBuffer.Length);
            }

            output.Write(pepper, 0, pepper.Length);
            return salt;
        }

        /// <inheritdoc/>
        public byte[] GeneratePepper(Stream input)
        {
            using var rng = RandomNumberGenerator.Create();
            var pepper = new byte[this.PepperLength];
            rng.GetNonZeroBytes(pepper);
            return pepper;
        }

        /// <inheritdoc/>
        public byte[] GenerateSalt(Stream input)
        {
            var salt = input.Hash(HashType.Sha256);
            input.Position = 0;
            Array.Reverse(salt, 0, 8);
            Array.Reverse(salt, 5, salt.Length - 5);
            Array.Reverse(salt);
            return salt;
        }
    }
}
