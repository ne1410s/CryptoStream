namespace Crypto.Utils
{
    /// <summary>
    /// Resizes arrays.
    /// </summary>
    public interface IArrayResizer
    {
        /// <summary>
        /// Resize a byte array.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="newSize">The new size.</param>
        void Resize(ref byte[] bytes, int newSize);
    }
}
