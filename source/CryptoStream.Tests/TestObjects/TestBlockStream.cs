// <copyright file="TestBlockStream.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

using CryptoStream.Streams;

namespace CryptoStream.Tests.TestObjects;

/// <summary>
/// Test harness for <see cref="BlockStream"/>
/// </summary>
public class TestBlockStream() : BlockStream(new MemoryStream(), 8)
{
    public override void FlushCache()
        => throw new NotSupportedException("FlushCache");
}
