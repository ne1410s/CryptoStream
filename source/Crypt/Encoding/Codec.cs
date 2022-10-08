// <copyright file="Codec.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Crypt.Encoding
{
    /// <summary>
    /// Encoding type.
    /// </summary>
    public enum Codec
    {
        /// <summary>
        /// Base64 byte encoding.
        /// </summary>
        ByteBase64 = 1,

        /// <summary>
        /// Hexadecimal byte encoding.
        /// </summary>
        ByteHex = 2,

        /// <summary>
        /// ASCII character encoding.
        /// </summary>
        CharAscii = 3,

        /// <summary>
        /// Unicode character encoding.
        /// </summary>
        CharUnicode = 4,

        /// <summary>
        /// UTF-8 character encoding.
        /// </summary>
        CharUtf8 = 5,
    }
}
