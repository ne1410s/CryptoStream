using System.IO;

namespace Crypto.Transform
{
    /// <summary>
    /// Encrypts a stream of contiguous bytes (end to end).
    /// </summary>
    public interface IEncryptor
    {
        /// <summary>
        /// Encrypts a stream.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <param name="output">The output stream.</param>
        /// <param name="userKey">The originally supplied key.</param>
        /// <param name="bufferLength">The buffer length.</param>
        /// <param name="mac">The authentication stream, if capture is needed.</param>
        /// <returns>The salt.</returns>
        byte[] Encrypt(
            Stream input,
            Stream output,
            byte[] userKey,
            int bufferLength = 32768,
            Stream mac = null);
    }
}
