namespace Crypt.Transform
{
    /// <summary>
    /// A block encrypted for counter-mode.
    /// </summary>
    public class GcmEncryptedBlock
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="GcmEncryptedBlock"/> class.
        /// </summary>
        /// <param name="messageBuffer">The message buffer.</param>
        /// <param name="macBuffer">The mac buffer.</param>
        public GcmEncryptedBlock(byte[] messageBuffer, byte[] macBuffer)
        {
            MessageBuffer = messageBuffer;
            MacBuffer = macBuffer;
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
