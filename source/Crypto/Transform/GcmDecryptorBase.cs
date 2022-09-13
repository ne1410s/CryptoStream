using System;
using System.IO;
using Crypto.Keying;

namespace Crypto.Transform
{
    /// <inheritdoc cref="IGcmDecryptor"/>
    public abstract class GcmDecryptorBase : IGcmDecryptor
    {
        private readonly ICryptoKeyDeriver keyDeriver;

        /// <summary>
        /// Initialises a new instance of <see cref="GcmDecryptorBase"/>.
        /// </summary>
        /// <param name="keyDeriver">The key deriver.</param>
        protected GcmDecryptorBase(ICryptoKeyDeriver keyDeriver)
        {
            this.keyDeriver = keyDeriver;
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

            output.SetLength(0);
            for (var b = 0; b < totalBlocks; b++)
            {
                mac?.Read(macBuffer, 0, macBuffer.Length);
                var position = input.Position;
                var maxReadSize = Math.Min(inputSize - position, srcBuffer.Length);
                var readSize = input.Read(srcBuffer, 0, (int)maxReadSize);
                var blockNumber = 1 + (long)Math.Floor((double)position / srcBuffer.Length);
                var counter = blockNumber.RaiseBits();
                Array.Resize(ref srcBuffer, readSize);

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
