// <copyright file="ByteExtensions.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace CryptoStream;

using System;
using System.Linq;

/// <summary>
/// Extensions for bytes and byte arrays.
/// </summary>
public static class ByteExtensions
{
    private static readonly byte[] SingleByte = [1];
    private static readonly byte[] FourZeroes = [0, 0, 0, 0];

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
        counter = counter ?? throw new ArgumentNullException(nameof(counter));
        bigEndian ??= !BitConverter.IsLittleEndian;
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

        counter = [.. left, .. right];
    }

    /// <summary>
    /// Raises an 8 byte (64-bit) integer to a 12 byte (92-bit) byte array.
    /// </summary>
    /// <param name="number">The 64-bit integer.</param>
    /// <param name="bigEndian">A value here forces big or little endianness
    /// accordingly - else that of the cpu architecture is used.</param>
    /// <remarks>Big endian means the most significant bit is first, little
    /// endian it is last. e.g. 16|8|4|2|1 is big endian.</remarks>
    /// <returns>A twelve-bytes array.</returns>
    public static byte[] RaiseBits(this long number, bool? bigEndian = null)
    {
        bigEndian ??= !BitConverter.IsLittleEndian;
        var eightBytes = BitConverter.GetBytes(number);
        if (bigEndian == BitConverter.IsLittleEndian)
        {
            eightBytes = eightBytes.Reverse().ToArray();
        }

        return bigEndian.Value
            ? [.. FourZeroes, .. eightBytes]
            : [.. eightBytes, .. FourZeroes];
    }
}
