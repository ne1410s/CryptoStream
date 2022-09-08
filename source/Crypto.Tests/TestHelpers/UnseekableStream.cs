namespace Crypto.Tests.TestHelpers;

internal class UnseekableStream : MemoryStream
{
    public UnseekableStream(int length = 0)
        : base(Enumerable.Range(0, length).Select(_ => (byte)1).ToArray())
    { }

    public override bool CanSeek => false;

    public override long Seek(long offset, SeekOrigin loc)
        => throw new NotSupportedException();
}
