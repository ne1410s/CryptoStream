using System;
using System.IO;
using System.Security.Cryptography;
using Crypto.Hashing;
using Crypto.Keying;

namespace Crypto.Transform
{
    /// <inheritdoc cref="IGcmEncryptor"/>
    public abstract class GcmEncryptorBase : IGcmEncryptor
    {
        private readonly ICryptoKeyDeriver keyDeriver;

        /// <summary>
        /// Initialises a new instance of <see cref="GcmEncryptorBase"/>.
        /// </summary>
        /// <param name="keyDeriver">The key deriver.</param>
        protected GcmEncryptorBase(ICryptoKeyDeriver keyDeriver)
        {
            this.keyDeriver = keyDeriver;
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
            var salt = GenerateSalt(input);
            var pepper = GeneratePepper(input);
            var cryptoKey = keyDeriver.DeriveCryptoKey(userKey, salt, pepper);

            int readSize;
            while ((readSize = input.Read(srcBuffer, 0, srcBuffer.Length)) != 0)
            {
                ByteExtensions.Increment(ref counter);
                var lastRead = readSize < bufferLength;
                if (lastRead)
                {
                    Array.Resize(ref srcBuffer, readSize);
                }

                var result = EncryptBlock(srcBuffer, cryptoKey, counter);
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
            var rng = RandomNumberGenerator.Create();
            var pepper = new byte[PepperLength];
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
