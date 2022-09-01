namespace Crypto.Hash
{
    /// <summary>
    /// A hashing algorithm.
    /// </summary>
    public enum HashAlgo
    {
        /// <summary>
        /// See <see cref="System.Security.Cryptography.MD5"/>.
        /// </summary>
        Md5,

        /// <summary>
        /// See <see cref="System.Security.Cryptography.SHA1"/>.
        /// </summary>
        Sha1,

        /// <summary>
        /// See <see cref="System.Security.Cryptography.SHA256"/>.
        /// </summary>
        Sha256,

        /// <summary>
        /// See <see cref="System.Security.Cryptography.SHA384"/>.
        /// </summary>
        Sha384,

        /// <summary>
        /// See <see cref="System.Security.Cryptography.SHA512"/>.
        /// </summary>
        Sha512,
    }
}