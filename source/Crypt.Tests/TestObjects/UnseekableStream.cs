// <copyright file="UnseekableStream.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Crypt.Tests.TestObjects;

public class UnseekableStream : MemoryStream
{
    public UnseekableStream(byte[] buffer)
        : base(buffer)
    { }

    public override bool CanSeek => false;

    public override long Seek(long offset, SeekOrigin loc)
        => throw new NotSupportedException();
}
