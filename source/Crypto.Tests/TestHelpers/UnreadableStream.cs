namespace Crypto.Tests.TestHelpers;

internal class UnreadableStream : MemoryStream
{
    public UnreadableStream(int length = 0)
        : base(Enumerable.Range(0, length).Select(_ => (byte)1).ToArray())
    { }

    public override bool CanRead => false;

    public override int Read(byte[] buffer, int offset, int count)
        => throw new NotSupportedException();
}
