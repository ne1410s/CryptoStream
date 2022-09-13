using System.IO;

namespace Crypto.Transform
{
    /// <summary>
    /// Decrypts a stream of contiguous bytes (end to end).
    /// </summary>
    public interface IGcmDecryptor : IDecryptor
    {
        /// <summary>
        /// Reads the pepper from a stream.
        /// </summary>
        /// <param name="input">The stream.</param>
        /// <returns>The pepper.</returns>
        byte[] ReadPepper(Stream input);

        /// <summary>
        /// Decrypts a block.
        /// </summary>
        /// <param name="block">The encrypted block.</param>
        /// <param name="cryptoKey">The cryptographic key.</param>
        /// <param name="counter">The counter.</param>
        /// <param name="authenticate">Whether to authenticate with mac.</param>
        /// <returns>A block of decrypted bytes.</returns>
        byte[] DecryptBlock(
            GcmEncryptedBlock block,
            byte[] cryptoKey,
            byte[] counter,
            bool authenticate);
    }
}
