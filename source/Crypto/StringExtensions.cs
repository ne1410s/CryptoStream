using System.Linq;
using Crypto.Hash;
using Crypto.Codec;

namespace Crypto
{
    /// <summary>
    /// Extensions for strings.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Derives a key based on a seed and a sequence of byte array sources.
        /// </summary>
        /// <param name="seed">A seed.</param>
        /// <param name="sources">A sequence of byte arrays.</param>
        /// <returns>The derived key.</returns>
        public static byte[] DeriveKey(this string seed, params byte[][] sources)
        {
            var hexHashes = sources
                .Select(k => k.AsString(ByteCodec.Hex))
                .OrderBy(s => s);

            foreach (var hexHash in hexHashes)
            {
                seed = (hexHash + seed)
                    .AsBytes(CharCodec.Utf8)
                    .Hash(HashAlgo.Sha1)
                    .AsString(ByteCodec.Base64);
            }

            return seed
                .AsBytes(CharCodec.Utf8)
                .Hash(HashAlgo.Sha1);
        }
    }
}
