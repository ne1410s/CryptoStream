// <copyright file="TestCrypto.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

using Crypt.Transform;

namespace Crypt.Tests.TestObjects;

public class TestCrypto : IEncryptor, IDecryptor
{
    public void Decrypt(
        Stream input, Stream output, byte[] userKey, byte[] salt, int bufferLength = 32768, Stream? mac = null)
    {
        output.Write(new byte[] { 1, 3, 5 });
    }

    public byte[] Encrypt(Stream input, Stream output, byte[] userKey, int bufferLength = 32768, Stream? mac = null)
    {
        output.Write(new byte[] { 2, 4, 6 });
        return new byte[] { 1, 2, 3 };
    }
}
