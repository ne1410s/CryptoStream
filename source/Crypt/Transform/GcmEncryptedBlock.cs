// <copyright file="GcmEncryptedBlock.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Crypt.Transform
{
    /// <summary>
    /// A block encrypted for counter-mode.
    /// </summary>
    public class GcmEncryptedBlock
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GcmEncryptedBlock"/> class.
        /// </summary>
        /// <param name="messageBuffer">The message buffer.</param>
        /// <param name="macBuffer">The mac buffer.</param>
        public GcmEncryptedBlock(byte[] messageBuffer, byte[] macBuffer)
        {
            this.MessageBuffer = messageBuffer;
            this.MacBuffer = macBuffer;
        }

        /// <summary>
        /// Gets the mac buffer.
        /// </summary>
        public byte[] MacBuffer { get; }

        /// <summary>
        /// Gets the message buffer.
        /// </summary>
        public byte[] MessageBuffer { get; }
    }
}
