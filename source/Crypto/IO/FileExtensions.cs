using System.Globalization;
using System.IO;
using Crypto.Encoding;
using Crypto.Hashing;
using Crypto.Keying;
using Crypto.Transform;

namespace Crypto.IO
{
    /// <summary>
    /// Extensions for <see cref="FileInfo"/>.
    /// </summary>
    public static class FileExtensions
    {
        /// <summary>
        /// Gets a salt.
        /// </summary>
        /// <param name="fi">The file info.</param>
        /// <returns>A salt.</returns>
        public static byte[] ToSalt(this FileInfo fi)
            => fi.FullName.Substring(0, 64).Encode(Codec.ByteHex);

        /// <summary>
        /// Gets a hash.
        /// </summary>
        /// <param name="fi">The file info.</param>
        /// <param name="mode">The hash mode.</param>
        /// <returns>A hash.</returns>
        public static byte[] Hash(this FileInfo fi, HashType mode)
        {
            using (var stream = fi.OpenRead())
            {
                return stream.Hash(mode);
            }
        }

        /// <summary>
        /// Gets a hash (when you're in a hurry). This is not suitable in any
        /// context where security matters; extremely easy to reverse engineer.
        /// </summary>
        /// <param name="fi">The file info.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="reads">The reads.</param>
        /// <param name="chunkSize">The chunk size.</param>
        /// <returns>A weak hash.</returns>
        public static byte[] HashLite(this FileInfo fi, HashType mode, int reads = 100, int chunkSize = 4096)
        {
            using (var stream = fi.OpenRead())
            {
                return stream.HashLite(mode, reads, chunkSize);
            }
        }

        /// <summary>
        /// Decrypts a file to a separate stream.
        /// </summary>
        /// <param name="fi">The file.</param>
        /// <param name="target">The target stream.</param>
        /// <param name="userKey">The user key.</param>
        /// <param name="decryptor">The decryptor.</param>
        /// <param name="keyDeriver">The key deriver.</param>
        /// <param name="bufferLength">The buffer length.</param>
        /// <param name="mac">The mac (optional).</param>
        public static void DecryptTo(
            this FileInfo fi,
            Stream target,
            byte[] userKey,
            IDecryptor decryptor = null,
            IKeyDeriver keyDeriver = null,
            int bufferLength = 32768,
            Stream mac = null)
        {
            decryptor = decryptor ?? new AesGcmDecryptor();
            keyDeriver = keyDeriver ?? new KeyDeriver();
            var salt = fi.ToSalt();

            using (var stream = fi.OpenRead())
            {
                decryptor.Decrypt(stream, target, userKey, salt, keyDeriver, bufferLength, mac);
            }
        }

        /// <summary>
        /// Encrypts a file in it's current location. Caution: the bytes are
        /// progressively overwritten in a non-transactional and irrevocable way.
        /// </summary>
        /// <param name="fi">The file.</param>
        /// <param name="userKey">The user key.</param>
        /// <param name="encryptor">The encryptor.</param>
        /// <param name="keyDeriver">The key deriver.</param>
        /// <param name="bufferLength">The buffer length.</param>
        /// <param name="mac">The mac.</param>
        /// <returns></returns>
        public static string EncryptInSitu(
            this FileInfo fi,
            byte[] userKey,
            IEncryptor encryptor = null,
            IKeyDeriver keyDeriver = null,
            int bufferLength = 32768,
            Stream mac = null)
        {
            encryptor = encryptor ?? new AesGcmEncryptor();
            keyDeriver = keyDeriver ?? new KeyDeriver();

            string saltHex;
            using (var stream = fi.Open(FileMode.Open))
            {
                saltHex = encryptor.Encrypt(stream, stream, userKey, keyDeriver, bufferLength, mac)
                    .Decode(Codec.ByteHex);
            }

            // NOT SURE ABOUT THIS STUFF IN LINUX??
            var target = Path.Combine(fi.DirectoryName, saltHex + fi.Extension)
               .ToLower(CultureInfo.InvariantCulture);

            if (target != fi.FullName)
            {
                File.Delete(target);
                fi.MoveTo(target);
            }

            return saltHex;
        }
    }
}
