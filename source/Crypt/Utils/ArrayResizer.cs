using System;

namespace Crypt.Utils
{
    /// <inheritdoc cref="IArrayResizer"/>
    public class ArrayResizer : IArrayResizer
    {
        /// <inheritdoc/>
        public void Resize(ref byte[] bytes, int newSize)
            => Array.Resize(ref bytes, newSize);
    }
}
