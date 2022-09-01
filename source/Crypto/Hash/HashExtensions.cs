using System;
using System.IO;
using System.Security.Cryptography;
using Crypto.Codec;

namespace Crypto.Hash
{
    /// <summary>
    /// Extensions for hashing.
    /// </summary>
    public static class HashExtensions
    {
        /// <summary>
        /// Obtains a signature of the input bytes.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="algo">The hash algorithm.</param>
        /// <returns>A byte array.</returns>
        public static byte[] Hash(this byte[] input, HashAlgo algo)
            => algo.ToAlgorithm().ComputeHash(input);

        /// <summary>
        /// Obtains a signature of the input stream. The caller is responsible
        /// for its disposal. Note that after the operation, the stream position
        /// will be at the end.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="algo">The hash algorithm.</param>
        /// <returns>A byte array.</returns>
        public static byte[] Hash(this Stream input, HashAlgo algo)
        {
            input.AssertReadable();
            if (input.Position != 0)
            {
                input.Position = 0;
            }

            var hasher = algo.ToAlgorithm();
            return hasher.ComputeHash(input);
        }

        /// <summary>
        /// Obtains a signature of the input file.
        /// </summary>
        /// <param name="fi">The input.</param>
        /// <param name="algo">The hash algorithm.</param>
        /// <returns>A byte array.</returns>
        public static byte[] Hash(this FileInfo fi, HashAlgo algo)
        {
            using var stream = File.Open(fi.FullName, FileMode.Open, FileAccess.Read);
            return stream.Hash(algo);
        }

        /// <summary>
        /// Hashes a file over evenly distributed file chunks.
        /// </summary>
        /// <remarks>Highly insecure, cryptographically.</remarks>
        /// <param name="fi">The file.</param>
        /// <param name="algo">The hash algorithm.</param>
        /// <param name="reads">The number of distributed reads.</param>
        /// <param name="chunkSize">The chunk size.</param>
        /// <returns>A byte array.</returns>
        public static byte[] LightHash(
            this FileInfo fi,
            HashAlgo algo,
            int reads = 20,
            int chunkSize = 4096)
        {
            using var str = fi.OpenRead();
            return str.LightHash(algo, reads, chunkSize);
        }

        /// <summary>
        /// Performs hashing over evenly-distributed stream chunks. The stream
        /// position is reset to the beginning, but the caller is responsible
        /// for its disposal.
        /// </summary>
        /// <remarks>Highly insecure, cryptographically.</remarks>
        /// <param name="stream">The stream.</param>
        /// <param name="algo">The hash algorithm.</param>
        /// <param name="reads">The number of distributed reads.</param>
        /// <param name="chunkSize">The chunk size.</param>
        /// <returns>A byte array.</returns>
        public static byte[] LightHash(
            this Stream stream,
            HashAlgo algo,
            int reads = 20,
            int chunkSize = 4096)
        {
            HashAlgorithm hasher = algo.ToAlgorithm();
            hasher.AssertReusable();
            stream.AssertReadable(true);

            var seedBytes = $"{stream.Length}".AsBytes(CharCodec.Utf8);
            var seed = seedBytes.Hash(algo);
            var dump = new byte[seed.Length];
            hasher.TransformBlock(seed, 0, seed.Length, dump, 0);

            var skipSize = (long)(stream.Length / (double)reads);
            var chunk = new byte[chunkSize];
            dump = new byte[chunkSize];
            stream.Seek(0, SeekOrigin.Begin);

            int lastRead;
            while ((lastRead = stream.Read(chunk, 0, chunkSize)) > 0)
            {
                hasher.TransformBlock(chunk, 0, lastRead, dump, 0);
                stream.Seek(skipSize, SeekOrigin.Current);
            }

            stream.Seek(0, SeekOrigin.Begin);
            hasher.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            return hasher.Hash;
        }

        private static HashAlgorithm ToAlgorithm(this HashAlgo algo) => algo switch
        {
            HashAlgo.Md5 => MD5.Create(),
            HashAlgo.Sha1 => SHA1.Create(),
            HashAlgo.Sha256 => SHA256.Create(),
            HashAlgo.Sha384 => SHA384.Create(),
            HashAlgo.Sha512 => SHA512.Create(),
            _ => throw new NotSupportedException($"{algo} unsupported"),
        };
    }
}
