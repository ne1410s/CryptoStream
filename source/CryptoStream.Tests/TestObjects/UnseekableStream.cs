// <copyright file="UnseekableStream.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace CryptoStream.Tests.TestObjects;

public class UnseekableStream(byte[] buffer) : MemoryStream(buffer)
{
    public override bool CanSeek => false;

    public override long Seek(long offset, SeekOrigin loc)
        => throw new NotSupportedException();
}
