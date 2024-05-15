// <copyright file="GcmEncryptedBlock.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace CryptoStream.Transform;

/// <summary>
/// A block encrypted for counter-mode.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GcmEncryptedBlock"/> class.
/// </remarks>
/// <param name="messageBuffer">The message buffer.</param>
/// <param name="macBuffer">The mac buffer.</param>
public class GcmEncryptedBlock(byte[] messageBuffer, byte[] macBuffer)
{
    /// <summary>
    /// Gets the mac buffer.
    /// </summary>
    public byte[] MacBuffer { get; } = macBuffer;

    /// <summary>
    /// Gets the message buffer.
    /// </summary>
    public byte[] MessageBuffer { get; } = messageBuffer;
}
