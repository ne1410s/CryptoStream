// <copyright file="TestBlockStream.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace CryptoStream.Tests.TestObjects;

using CryptoStream.Streams;

/// <summary>
/// Test harness for <see cref="BlockStream"/>.
/// </summary>
public class TestBlockStream() : BlockStream(new MemoryStream(), 8)
{
    public override void FlushCache()
        => throw new NotSupportedException("FlushCache");
}
