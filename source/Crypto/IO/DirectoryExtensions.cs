using System.IO;
using System.Linq;
using Crypto.Encoding;
using Crypto.Hashing;
using Crypto.Transform;

namespace Crypto.IO
{
    /// <summary>
    /// Extensions for <see cref="DirectoryInfo"/>.
    /// </summary>
    public static class DirectoryExtensions
    {
        /// <summary>
        /// Signs a folder structure recursively. This process is not sensitive
        /// to changes in metadata.
        /// </summary>
        /// <param name="di">The directory.</param>
        /// <param name="mode">The hash mode.</param>
        /// <returns>The hash sum.</returns>
        public static byte[] HashSum(this DirectoryInfo di, HashType mode)
        {
            var hash = "".Hash(mode);
            foreach (var fsi in di.EnumerateFileSystemInfos("*", SearchOption.AllDirectories))
            {
                var item = fsi is FileInfo fi
                    ? fi.Hash(mode)
                    : fsi.Name.Encode(Codec.CharUtf8).Hash(mode);
                hash = hash.Concat(item).ToArray().Hash(mode);
            }

            return hash;
        }

        /// <summary>
        /// Encrypts all files in-situ, recursively.
        /// </summary>
        /// <param name="di">The directory.</param>
        /// <param name="userKey">The user key.</param>
        /// <param name="encryptor">The encryptor.</param>
        /// <param name="bufferLength">The buffer length.</param>
        public static void EncryptAllInSitu(
            this DirectoryInfo di,
            byte[] userKey,
            IEncryptor encryptor = null,
            int bufferLength = 32768)
        {
            foreach (var fi in di.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                fi.EncryptInSitu(userKey, encryptor, bufferLength);
            }
        }
    }
}
