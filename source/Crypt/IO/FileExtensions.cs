using System.IO;
using System.Text.RegularExpressions;
using Crypt.Encoding;
using Crypt.Hashing;
using Crypt.Transform;

namespace Crypt.IO
{
    /// <summary>
    /// Extensions for <see cref="FileInfo"/>.
    /// </summary>
    public static class FileExtensions
    {
        private static readonly Regex SaltRegex = new Regex(
            @"^(?<hex>[a-f0-9]{64})(?<ext>\.[\w-]+){0,1}$",
            RegexOptions.Compiled);

        /// <summary>
        /// Gets a salt.
        /// </summary>
        /// <param name="fi">The file info.</param>
        /// <returns>A salt.</returns>
        public static byte[] ToSalt(this FileInfo fi)
        {
            var match = SaltRegex.Match(fi.Name);
            return match.Success
                ? match.Groups["hex"].Value.Decode(Codec.ByteHex)
                : throw new System.ArgumentException(
                    $"Unable to obtain salt: '{fi.Name}'",
                    "fileName");
        }

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
        /// <param name="bufferLength">The buffer length.</param>
        /// <param name="mac">The mac (optional).</param>
        public static void DecryptTo(
            this FileInfo fi,
            Stream target,
            byte[] userKey,
            IDecryptor decryptor = null,
            int bufferLength = 32768,
            Stream mac = null)
        {
            decryptor = decryptor ?? new AesGcmDecryptor();
            var salt = fi.ToSalt();

            using (var stream = fi.OpenRead())
            {
                decryptor.Decrypt(stream, target, userKey, salt, bufferLength, mac);
            }
        }

        /// <summary>
        /// Encrypts a file in its current location. Caution: the bytes are
        /// progressively overwritten in a non-transactional and irrevocable way.
        /// </summary>
        /// <param name="fi">The file.</param>
        /// <param name="userKey">The user key.</param>
        /// <param name="encryptor">The encryptor.</param>
        /// <param name="bufferLength">The buffer length.</param>
        /// <param name="mac">The mac.</param>
        /// <returns>The salt hex.</returns>
        public static string EncryptInSitu(
            this FileInfo fi,
            byte[] userKey,
            IEncryptor encryptor = null,
            int bufferLength = 32768,
            Stream mac = null)
        {
            encryptor = encryptor ?? new AesGcmEncryptor();

            var saltHex = (string)null;
            using (var stream = fi.Open(FileMode.Open))
            {
                saltHex = encryptor.Encrypt(stream, stream, userKey, bufferLength, mac)
                    .Encode(Codec.ByteHex)
                    .ToLower();
            }

            var target = Path.Combine(fi.DirectoryName, saltHex + fi.Extension);
            if (target != fi.FullName)
            {
                File.Delete(target);
                fi.MoveTo(target);
            }

            return saltHex;
        }
    }
}
