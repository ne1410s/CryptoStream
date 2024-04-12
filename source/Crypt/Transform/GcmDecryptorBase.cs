// <copyright file="GcmDecryptorBase.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Crypt.Transform;

using System;
using System.IO;
using System.Linq;
using Crypt.Keying;
using Crypt.Streams;
using Crypt.Utils;

/// <inheritdoc cref="IGcmDecryptor"/>
/// <summary>
/// Initializes a new instance of the <see cref="GcmDecryptorBase"/> class.
/// </summary>
/// <param name="keyDeriver">The key deriver.</param>
/// <param name="resizer">An array resizer.</param>
public abstract class GcmDecryptorBase(
    ICryptoKeyDeriver keyDeriver,
    IArrayResizer resizer) : IGcmDecryptor
{
    private const int PepperLength = 32;
    private const int SizerLength = 12;

    /// <inheritdoc/>
    public abstract byte[] DecryptBlock(
        GcmEncryptedBlock block,
        byte[] cryptoKey,
        byte[] counter,
        bool authenticate);

    /// <inheritdoc/>
    public void Decrypt(
        Stream input,
        Stream output,
        byte[] userKey,
        byte[] salt,
        int bufferLength = 32768,
        Stream mac = null)
    {
        output = output ?? throw new ArgumentNullException(nameof(output));

        var macBuffer = new byte[16];
        var srcBuffer = new byte[bufferLength];
        var pepper = this.ReadPepper(input);
        var cryptoKey = keyDeriver.DeriveCryptoKey(userKey, salt, pepper);
        var inputSize = this.GetOriginalSize(input, cryptoKey);
        var totalBlocks = (long)Math.Ceiling(inputSize / (double)bufferLength);
        mac?.Reset();

        output.SetLength(0);
        foreach (var b in Enumerable.Range(0, (int)totalBlocks))
        {
            mac?.Read(macBuffer, 0, macBuffer.Length);
            var position = input.Position;
            var maxReadSize = Math.Min(inputSize - position, srcBuffer.Length);
            var readSize = input.Read(srcBuffer, 0, (int)maxReadSize);
            var blockNumber = 1 + (long)Math.Floor((double)position / srcBuffer.Length);
            var counter = blockNumber.RaiseBits();
            if (readSize < srcBuffer.Length)
            {
                resizer.Resize(ref srcBuffer, readSize);
            }

            var block = new GcmEncryptedBlock(srcBuffer, macBuffer);
            var trgBuffer = this.DecryptBlock(block, cryptoKey, counter, mac != null);
            output.Write(trgBuffer, 0, trgBuffer.Length);
        }

        output.Reset();
    }

    /// <inheritdoc/>
    public byte[] ReadPepper(Stream input)
    {
        input = input ?? throw new ArgumentNullException(nameof(input));

        var pepper = new byte[PepperLength];
        input.Seek(-PepperLength, SeekOrigin.End);
        input.Read(pepper, 0, pepper.Length);
        input.Reset();
        return pepper;
    }

    private long GetOriginalSize(Stream input, byte[] key)
    {
        var sizerBytes = new byte[SizerLength];
        input.Seek(-(PepperLength + SizerLength), SeekOrigin.End);
        input.Read(sizerBytes, 0, SizerLength);
        input.Reset();
        var churn = this.DecryptBlock(new(sizerBytes, []), key, 1L.RaiseBits(), false);
        var retVal = BitConverter.ToInt64(churn, 0);
        return retVal < 0 || retVal > 20_000_000_000
            ? throw new InvalidOperationException("Unexpected source state.")
            : retVal;
    }
}
