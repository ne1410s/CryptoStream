using System;
using System.IO;
using System.Security.Cryptography;

namespace Crypt.Hashing
{
    /// <summary>
    /// Obtains a short byte signatures from streams.
    /// </summary>
    public static class HashingExtensions
    {
        /// <summary>
        /// Obtains a signature from a stream.
        /// </summary>
        /// <param name="input">The stream.</param>
        /// <param name="mode">The hash mode.</param>
        /// <returns>A signature.</returns>
        public static byte[] Hash(this Stream input, HashType mode)
        {
            input.Position = 0;
            return ToAlgo(mode).ComputeHash(input);
        }

        /// <summary>
        /// Obtains a signature from a byte array.
        /// </summary>
        /// <param name="input">The byte array.</param>
        /// <param name="mode">The hash mode.</param>
        /// <returns>A signature.</returns>
        public static byte[] Hash(this byte[] input, HashType mode)
            => ToAlgo(mode).ComputeHash(input);

        /// <summary>
        /// Obtains a signature from a stream (when you're in a hurry). Results
        /// in only the most cursory values and is ludicrously easy to reverse
        /// engineer. Do not use this in any context where security matters!!
        /// </summary>
        /// <param name="input">The byte array.</param>
        /// <param name="mode">The hash mode.</param>
        /// <param name="reads">The number of distributed reads.</param>
        /// <param name="chunkSize">The size of each read.</param>
        /// <returns>A signature.</returns>
        public static byte[] HashLite(this Stream input, HashType mode, int reads = 100, int chunkSize = 4096)
        {
            var algo = ToAlgo(mode);
            var seedBytes = System.Text.Encoding.UTF8.GetBytes($"{input.Length}");
            var seed = Hash(seedBytes, mode);
            var dump = new byte[seed.Length];
            algo.TransformBlock(seed, 0, seed.Length, dump, 0);

            var skipSize = (long)(input.Length / (double)reads);
            var chunk = new byte[chunkSize];
            dump = new byte[chunkSize];
            input.Position = 0;

            int lastRead;
            while ((lastRead = input.Read(chunk, 0, chunkSize)) != 0)
            {
                algo.TransformBlock(chunk, 0, lastRead, dump, 0);
                input.Seek(skipSize, SeekOrigin.Current);
            }

            input.Position = 0;
            algo.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            return algo.Hash;
        }

        private static HashAlgorithm ToAlgo(HashType mode)
        {
            switch (mode)
            {
                case HashType.Md5: return MD5.Create();
                case HashType.Sha1: return SHA1.Create();
                case HashType.Sha256: return SHA256.Create();
                case HashType.Sha384: return SHA384.Create();
                case HashType.Sha512: return SHA512.Create();
                default: throw new ArgumentException($"Bad hash mode: {mode}", nameof(mode));
            }
        }
    }
}
