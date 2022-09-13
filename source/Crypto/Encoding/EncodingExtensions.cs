using System;
using SysEncoding = System.Text.Encoding;

namespace Crypto.Encoding
{
    /// <summary>
    /// Transforms bytes to text and vice versa.
    /// </summary>
    public static class EncodingExtensions
    {
        /// <summary>
        /// Transforms an array of bytes to text.
        /// </summary>
        /// <param name="bytes">The encoded bytes.</param>
        /// <param name="codec">The encoding type.</param>
        /// <returns>Decoded text.</returns>
        public static string Decode(this byte[] bytes, Codec codec)
        {
            switch (codec)
            {
                case Codec.ByteBase64: return Convert.ToBase64String(bytes);
                case Codec.ByteHex: return DecodeHex(bytes);
                case Codec.CharAscii: return SysEncoding.ASCII.GetString(bytes);
                case Codec.CharUnicode: return SysEncoding.Unicode.GetString(bytes);
                case Codec.CharUtf8: return SysEncoding.UTF8.GetString(bytes);
                default: throw new ArgumentException($"Bad codec: {codec}", nameof(codec));
            }
        }

        /// <summary>
        /// Transforms text to an array of bytes.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="codec">The encoding type.</param>
        /// <returns>An encoded byte array.</returns>
        public static byte[] Encode(this string text, Codec codec)
        {
            switch (codec)
            {
                case Codec.ByteBase64: return Convert.FromBase64String(text);
                case Codec.ByteHex: return EncodeHex(text);
                case Codec.CharAscii: return SysEncoding.ASCII.GetBytes(text);
                case Codec.CharUnicode: return SysEncoding.Unicode.GetBytes(text);
                case Codec.CharUtf8: return SysEncoding.UTF8.GetBytes(text);
                default: throw new ArgumentException($"Bad codec: {codec}", nameof(codec));
            }
        }

        private static byte[] EncodeHex(string hex)
        {
            var bytes = new byte[hex.Length / 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }

            return bytes;
        }

        private static string DecodeHex(byte[] bytes)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var b in bytes)
            {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }
    }
}
