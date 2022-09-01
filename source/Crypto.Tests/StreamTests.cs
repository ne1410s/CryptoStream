using Crypto.Streams;
using SixLabors.ImageSharp;

namespace Crypto.Tests;

public class StreamTests
{
    private const string PLAIN_SM = "test.png";
    private const string SECURE_SM =
        "9906cd59bb589ac01fa19f009cfbccf783f865b5b22de954b32384d70d0936b8.png";

    private const string PLAIN_MD = "test.jpg";
    private const string SECURE_MD =
        "d0c08571b840a9a617261d1dc4eb3683357baca813ba951dd921558f7ae1141d.jpg";

    [Theory]
    [InlineData(PLAIN_SM)]
    [InlineData(PLAIN_MD)]
    public void Reading_PlainFiles_AsExpected(string fileName)
    {
        var path = Path.Combine("img", fileName);
        var inStream = new ReadFileStream(new FileInfo(path));
        var img = Image.Load(inStream);
        img.Save("out-reading-" + fileName);
    }

    [Theory]
    [InlineData(PLAIN_SM)]
    [InlineData(PLAIN_MD)]
    public void Chunking_PlainFiles_AsExpected(string fileName)
    {
        var path = Path.Combine("img", fileName);
        var inStream = new ChunkingFileStream(new FileInfo(path));
        var img = Image.Load(inStream);
        img.Save("out-chunking-" + fileName);
    }

    [Theory]
    [InlineData(SECURE_SM)]
    [InlineData(SECURE_MD)]
    public void Streaming_SecureFiles_AsExpected(string fileName)
    {
        var path = Path.Combine("img", fileName);
        var inStream = new CryptoFileStream(new FileInfo(path), CryptExtensionsTests.TestKey);
        var img = Image.Load(inStream);
        img.Save("out-streaming-" + fileName);
    }
}
