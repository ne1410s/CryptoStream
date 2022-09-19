﻿using System;
using System.IO;
using System.Linq;
using Crypto.Keying;
using Crypto.Utils;

namespace Crypto.Transform
{
    /// <inheritdoc cref="IGcmDecryptor"/>
    public abstract class GcmDecryptorBase : IGcmDecryptor
    {
        private readonly ICryptoKeyDeriver keyDeriver;
        private readonly IArrayResizer resizer;

        /// <summary>
        /// Initialises a new instance of <see cref="GcmDecryptorBase"/>.
        /// </summary>
        /// <param name="keyDeriver">The key deriver.</param>
        /// <param name="resizer">An array resizer.</param>
        protected GcmDecryptorBase(
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
        public abstract byte[] DecryptBlock(
            GcmEncryptedBlock block,
            byte[] cryptoKey,
            byte[] counter,
            bool authenticate);

        /// <inheritdoc/>
        public void Decrypt(
            Stream input,
            Stream output,
            byte[] userKey,
            byte[] salt,
            int bufferLength = 32768,
            Stream mac = null)
        {
            var macBuffer = new byte[16];
            var srcBuffer = new byte[bufferLength];
            var pepper = ReadPepper(input);
            var cryptoKey = keyDeriver.DeriveCryptoKey(userKey, salt, pepper);
            var inputSize = input.Length - PepperLength;
            var totalBlocks = (long)Math.Ceiling(inputSize / (double)bufferLength);
            mac?.Seek(0, SeekOrigin.Begin);

            output.SetLength(0);
            foreach (var b in Enumerable.Range(0, (int)totalBlocks))
            {
                mac?.Read(macBuffer, 0, macBuffer.Length);
                var position = input.Position;
                var maxReadSize = Math.Min(inputSize - position, srcBuffer.Length);
                var readSize = input.Read(srcBuffer, 0, (int)maxReadSize);
                var blockNumber = 1 + (long)Math.Floor((double)position / srcBuffer.Length);
                var counter = blockNumber.RaiseBits();
                if (readSize < srcBuffer.Length)
                {
                    resizer.Resize(ref srcBuffer, readSize);
                }

                var block = new GcmEncryptedBlock(srcBuffer, macBuffer);
                var trgBuffer = DecryptBlock(block, cryptoKey, counter, mac != null);
                output.Write(trgBuffer, 0, trgBuffer.Length);
            }

            output.Position = 0;
        }

        /// <inheritdoc/>
        public byte[] ReadPepper(Stream input)
        {
            var pepper = new byte[PepperLength];
            input.Seek(-PepperLength, SeekOrigin.End);
            input.Read(pepper, 0, pepper.Length);
            input.Position = 0;
            return pepper;
        }
    }
}
