using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Crypto.Hash;
using Crypto.Codec;
using AesGcm = Jose.AesGcm;

namespace Crypto
{
    /// <summary>
    /// Extensions for cryptography.
    /// </summary>
    public static class CryptExtensions
    {
        private const int TagLength = 16;
        private const int PepperLength = 32;

        /// <summary>
        /// Encrypts a stream.
        /// </summary>
        /// <param name="source">The source stream.</param>
        /// <param name="target">The target stream.</param>
        /// <param name="key">The key.</param>
        /// <param name="bufferLength">The buffer length.</param>
        /// <param name="mac">The authentication stream, if capture is required.</param>
        /// <returns>The generated salt.</returns>
        public static string Encrypt(
            this Stream source,
            Stream target,
            byte[] key,
            int bufferLength = 32768,
            Stream mac = null)
        {
            var counter = new byte[12];
            var srcBuffer = new byte[bufferLength];
            var salt = source.Hash(HashAlgo.Sha256);

            source.Position = 0;
            Array.Reverse(salt, 0, 8);
            Array.Reverse(salt, 5, salt.Length - 5);
            Array.Reverse(salt);

            int readSize;
            var pepper = new byte[PepperLength];
            var rng = RandomNumberGenerator.Create();
            rng.GetNonZeroBytes(pepper);
            var keyInternal = key.Derive(salt, pepper);

            while ((readSize = source.Read(srcBuffer, 0, srcBuffer.Length)) != 0)
            {
                ByteExtensions.Increment(ref counter);
                var lastRead = readSize < bufferLength;
                if (lastRead)
                {
                    Array.Resize(ref srcBuffer, readSize);
                }

                var trgBufs = AesGcm.Encrypt(keyInternal, counter, new byte[0], srcBuffer);
                mac?.Write(trgBufs[1], 0, trgBufs[1].Length);
                if (source == target)
                {
                    source.Position -= readSize;
                }

                target.Write(trgBufs[0], 0, trgBufs[0].Length);
            }

            target.Write(pepper, 0, pepper.Length);
            return salt.AsString(ByteCodec.Hex);
        }

        /// <summary>
        /// Reads and immediately writes-over data in the same stream. This does not happen
        /// transactionally and hence has risks, i.e. the source could be rendered uncoverable.
        /// </summary>
        /// <param name="source">The source (and target) stream.</param>
        /// <param name="key">The key.</param>
        /// <param name="bufferLength">The buffer length.</param>
        /// <param name="mac">The authentication stream, if capture is required.</param>
        /// <returns>The generated salt.</returns>
        public static string EncryptInSitu(
            this Stream source,
            byte[] key,
            int bufferLength = 32768,
            Stream mac = null)
        {
            return source.Encrypt(source, key, bufferLength, mac);
        }

        /// <summary>
        /// Encrypts a file in-situ, optionally writing a MAC if stream passed.
        /// </summary>
        /// <param name="fi">The file.</param>
        /// <param name="key">The key.</param>
        /// <param name="bufferLength">Buffer length.</param>
        /// <param name="mac">Optional write stream to capture MAC.</param>
        public static void Encrypt(
            this FileInfo fi,
            byte[] key,
            int bufferLength = 32768,
            Stream mac = null)
        {
            string saltHex;
            using (var source = fi.Open(FileMode.Open))
            {
                saltHex = source.EncryptInSitu(key, bufferLength, mac);
            }

            var target = Path.Combine(fi.DirectoryName, saltHex + fi.Extension)
                .ToLower(CultureInfo.InvariantCulture);

            if (target != fi.FullName)
            {
                File.Delete(target);
                fi.MoveTo(target);
            }
        }

        /// <summary>
        /// Decrypts a file to stream. The target stream is truncated before use,
        /// and gets reset to its starting position on completion.
        /// </summary>
        /// <param name="fi">The file.</param>
        /// <param name="key">The key.</param>
        /// <param name="target">The target stream.</param>
        /// <param name="bufferLength">The buffer length.</param>
        /// <param name="mac">Optional MAC stream. Enables the caller to check
        /// the authenticity of the message if they so wish.</param>
        public static void Decrypt(
            this FileInfo fi,
            byte[] key,
            Stream target,
            int bufferLength = 32768,
            Stream mac = null)
        {
            var macBuffer = GenerateMacBuffer();
            var srcBuffer = new byte[bufferLength];
            var salt = fi.GenerateSalt();

            target.SetLength(0);
            using (var source = fi.OpenRead())
            {
                var pepper = source.GeneratePepper();
                var keyInternal = key.Derive(salt, pepper);
                var totalBlocks = (long)Math.Ceiling(
                    (fi.Length - PepperLength) / (double)bufferLength);

                for (var b = 0; b < totalBlocks; b++)
                {
                    mac?.Read(macBuffer, 0, macBuffer.Length);
                    var trgBuffer = keyInternal.DecryptBlock(
                        source, mac != null, srcBuffer, macBuffer);
                    target.Write(trgBuffer, 0, trgBuffer.Length);
                }
            }

            target.Position = 0;
        }

        /// <summary>
        /// Generates a salt from a file.
        /// </summary>
        /// <param name="fi">The file.</param>
        /// <returns>A salt.</returns>
        public static byte[] GenerateSalt(this FileInfo fi)
            => fi.Name.GenerateSalt();

        /// <summary>
        /// Generates a salt from a file name.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <returns>A salt.</returns>
        public static byte[] GenerateSalt(this string fileName)
            => fileName.Substring(0, 64).AsBytes(ByteCodec.Hex);

        /// <summary>
        /// Generates a pepper from a stream.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>A pepper.</returns>
        public static byte[] GeneratePepper(this Stream source)
        {
            var pepper = new byte[PepperLength];
            source.Seek(-PepperLength, SeekOrigin.End);
            source.Read(pepper, 0, pepper.Length);
            source.Seek(0, SeekOrigin.Begin);
            return pepper;
        }

        /// <summary>
        /// Generates a buffer of suitable length for use in MAC processes.
        /// </summary>
        /// <returns>The mac buffer.</returns>
        public static byte[] GenerateMacBuffer()
        {
            return new byte[TagLength];
        }

        /// <summary>
        /// Produces a key from an input and crypto seasonings.
        /// </summary>
        /// <param name="inputKey">The input key.</param>
        /// <param name="salt">The salt.</param>
        /// <param name="pepper">The pepper.</param>
        /// <returns>The output key.</returns>
        public static byte[] Derive(this byte[] inputKey, byte[] salt, byte[] pepper)
        {
            return pepper
                .Concat(inputKey)
                .Concat(salt.Reverse())
                .ToArray()
                .Hash(HashAlgo.Sha256);
        }

        /// <summary>
        /// Decrypts a block after reading it from the stream current position.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="source">The source stream.</param>
        /// <param name="authenticate">Whether to authenticate.</param>
        /// <param name="srcBuffer">The ciphertext block.</param>
        /// <param name="macBuffer">The mac buffer. If authenticating, this must
        /// be pre-filled with appropriate bytes.</param>
        /// <returns>The bytes read.</returns>
        public static byte[] DecryptBlock(
            this byte[] key,
            Stream source,
            bool authenticate,
            byte[] srcBuffer,
            byte[] macBuffer)
        {
            var length = source.Length - PepperLength;
            var position = source.Position;
            var maxReadSize = Math.Min(length - position, srcBuffer.Length);
            var readSize = source.Read(srcBuffer, 0, (int)maxReadSize);

            var blockNumber = 1 + (long)Math.Floor((double)position / srcBuffer.Length);
            var counter = blockNumber.Pad12();

            Array.Resize(ref srcBuffer, readSize);
            return authenticate
                ? AesGcm.Decrypt(key, counter, new byte[0], srcBuffer, macBuffer)
                : AesGcm.Encrypt(key, counter, new byte[0], srcBuffer)[0];
        }
    }
}
