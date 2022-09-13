using System.IO;

namespace Crypto.Transform
{
    /// <summary>
    /// Encrypts a stream of contiguous bytes (end to end).
    /// </summary>
    public interface IGcmEncryptor : IEncryptor
    {
        byte[] GeneratePepper(Stream input);

        byte[] GenerateSalt(Stream input);

        GcmEncryptedBlock EncryptBlock(byte[] source, byte[] cryptoKey, byte[] counter);
    }
}
