using System;
using System.Text;

namespace Crypto.Codec
{
    /// <summary>
    /// Extensions for encoding and decoding string.
    /// </summary>
    public static class CodecExtensions
    {
        /// <summary>
        /// Encodes a string as bytes as per the character codec provided.
        /// </summary>
        /// <param name="input">The string.</param>
        /// <param name="codec">The character codec.</param>
        /// <returns>A byte array.</returns>
        public static byte[] AsBytes(this string input, CharCodec codec) =>
            codec.ToCodec().GetBytes(input);

        /// <summary>
        /// Encodes a string as bytes as per the byte codec provided.
        /// </summary>
        /// <param name="input">The string.</param>
        /// <param name="codec">The byte codec.</param>
        /// <returns>A byte array.</returns>
        public static byte[] AsBytes(this string input, ByteCodec codec)
            => codec.ToBytesFunc()(input);

        /// <summary>
        /// Decodes a byte array as per the character codec provided.
        /// </summary>
        /// <param name="input">The byte array.</param>
        /// <param name="codec">The character codec.</param>
        /// <returns>A string.</returns>
        public static string AsString(this byte[] input, CharCodec codec) =>
            codec.ToCodec().GetString(input);

        /// <summary>
        /// Decodes a byte array as per the byte codec provided.
        /// </summary>
        /// <param name="input">The byte array.</param>
        /// <param name="codec">The byte codec.</param>
        /// <returns>A string.</returns>
        public static string AsString(this byte[] input, ByteCodec codec) =>
            codec.ToStringFunc()(input);

        private static Encoding ToCodec(this CharCodec mode) => mode switch
        {
            CharCodec.Ascii => Encoding.ASCII,
            CharCodec.Unicode => Encoding.Unicode,
            CharCodec.Utf8 => Encoding.UTF8,
            _ => throw new NotSupportedException($"{mode} unsupported"),
        };

        private static Func<string, byte[]> ToBytesFunc(this ByteCodec mode) => mode switch
        {
            ByteCodec.Base64 => str => Convert.FromBase64String(str),
            ByteCodec.Hex => str =>
            {
                var bytes = new byte[str.Length / 2];
                for (var i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = Convert.ToByte(str.Substring(i * 2, 2), 16);
                }

                return bytes;
            },
            _ => throw new NotSupportedException($"{mode} -> bytes unsupported"),
        };

        private static Func<byte[], string> ToStringFunc(this ByteCodec mode) => mode switch
        {
            ByteCodec.Base64 => bytes => Convert.ToBase64String(bytes),
            ByteCodec.Hex => bytes =>
            {
                var sb = new StringBuilder();
                foreach (var b in bytes)
                {
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            },
            _ => throw new NotSupportedException($"{mode} -> string unsupported"),
        };
    }
}
