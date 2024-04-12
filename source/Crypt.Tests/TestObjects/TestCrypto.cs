// <copyright file="TestCrypto.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Crypt.Tests.TestObjects;

using Crypt.Transform;

public class TestCrypto : IEncryptor, IDecryptor
{
    public Dictionary<string, string> Decrypt(
        Stream input, Stream output, byte[] userKey, byte[] salt, int bufferLength = 32768, Stream? mac = null)
    {
        output.Write([1, 3, 5]);
        return new();
    }

    public byte[] Encrypt(
        Stream input,
        Stream output,
        byte[] userKey,
        Dictionary<string, string> metadata,
        int bufferLength = 32768,
        Stream? mac = null)
    {
        output.Write([2, 4, 6]);
        return [1, 2, 3];
    }
}
