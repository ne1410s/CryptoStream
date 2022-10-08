// <copyright file="IArrayResizer.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Crypt.Utils
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
