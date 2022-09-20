using System.Linq;
using Crypt.Encoding;
using Crypt.Hashing;

namespace Crypt.Keying
{
    /// <inheritdoc cref="IKeyDeriver"/>
    public class DefaultKeyDeriver : IKeyDeriver, ICryptoKeyDeriver
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
                .Select(k => k.Encode(Codec.ByteHex))
                .OrderBy(s => s);

            foreach (var hexHash in hexHashes)
            {
                seed = (hexHash + seed)
                    .Decode(Codec.CharUtf8)
                    .Hash(HashType.Sha1)
                    .Encode(Codec.ByteBase64);
            }

            return seed
                .Decode(Codec.CharUtf8)
                .Hash(HashType.Sha1);
        }
    }
}
