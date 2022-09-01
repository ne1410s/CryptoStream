using System;
using System.Linq;

namespace Crypto
{
    /// <summary>
    /// Cryptographic extensions relating to bytes.
    /// </summary>
    public static class ByteExtensions
    {
        private static readonly byte[] SingleByte = new byte[] { 1 };
        private static readonly byte[] FourZeroes = { 0, 0, 0, 0 };

        /// <summary>
        /// Increments a counter. Useful for > 64-bit operations.
        /// </summary>
        /// <param name="counter">The counter.</param>
        /// <param name="bigEndian">A value here forces big or little endianness
        /// accordingly - else that of the cpu architecture is used.</param>
        /// <remarks>Big endian means the most significant bit is first, little
        /// endian it is last. e.g. 16|8|4|2|1 is big endian.</remarks>
        public static void Increment(ref byte[] counter, bool? bigEndian = null)
        {
            bigEndian = bigEndian ?? !BitConverter.IsLittleEndian;
            var start = bigEndian.Value ? counter.Length - 1 : 0;
            var terminate = bigEndian.Value ? -1 : counter.Length;
            var increment = bigEndian.Value ? -1 : 1;

            for (var n = start; n != terminate; n += increment)
            {
                if (counter[n] < byte.MaxValue)
                {
                    counter[n]++;
                    return;
                }
                else
                {
                    counter[n] = 0;
                }
            }

            var left = bigEndian.Value ? SingleByte : counter;
            var right = bigEndian.Value ? counter : SingleByte;

            counter = left.Concat(right).ToArray();
        }

        /// <summary>
        /// Gets a 64-bit integer expressed as a padded array of twelve bytes.
        /// </summary>
        /// <param name="number">The 64-bit integer.</param>
        /// <returns>A twelve-bytes array.</returns>
        public static byte[] Pad12(this long number)
        {
            var eightBytes = BitConverter.GetBytes(number);
            return BitConverter.IsLittleEndian
                ? eightBytes.Concat(FourZeroes).ToArray()
                : FourZeroes.Concat(eightBytes).ToArray();
        }
    }
}
