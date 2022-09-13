using System.Linq;
using Crypto.Encoding;
using Crypto.Hashing;

namespace Crypto.Keying
{
    /// <inheritdoc cref="IKeyDeriver"/>
    public class KeyDeriver : IKeyDeriver
    {
        /// <inheritdoc/>
        public byte[] DeriveCryptoKey(byte[] userKey, byte[] salt, byte[] pepper)
            => pepper.Concat(userKey).Concat(salt.Reverse())
                .ToArray()
                .Hash(HashType.Sha256);

        /// <inheritdoc/>
        public byte[] DeriveKey(string seed, params byte[][] hashes)
        {
            var hexHashes = hashes
                .Select(k => k.Decode(Codec.ByteHex))
                .OrderBy(s => s);

            foreach (var hexHash in hexHashes)
            {
                seed = (hexHash + seed)
                    .Encode(Codec.CharUtf8)
                    .Hash(HashType.Sha1)
                    .Decode(Codec.ByteBase64);
            }

            return seed
                .Encode(Codec.CharUtf8)
                .Hash(HashType.Sha1);
        }
    }
}
