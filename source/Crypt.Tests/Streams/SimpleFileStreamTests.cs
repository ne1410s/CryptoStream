using Crypt.Encoding;
using Crypt.Hashing;
using Crypt.Streams;

namespace Crypt.Tests.Streams
{
    /// <summary>
    /// Tests for the <see cref="SimpleFileStream"/>.
    /// </summary>
    public class SimpleFileStreamTests
    {
        [Fact]
        public void Ctor_SpecificMedia_HashExpected()
        {
            // Arrange
            var fi = new FileInfo(Path.Combine("Samples", "sample.avi"));
            var sut = new SimpleFileStream(fi);
            var expectedMd5Hex = "91d326694fdff83d0df74c357f3feb84";

            // Act
            var actualMd5Hex = sut.Hash(HashType.Md5).Encode(Codec.ByteHex);

            // Assert
            actualMd5Hex.Should().Be(expectedMd5Hex);
        }

        [Theory]
        [InlineData(3370848, "251b6820114a93dd0ed52a81ce33e716")]
        public void Read_SpecificPosition_GivesExpected(long position, string expectedMd5Hex)
        {
            // Arrange
            var fi = new FileInfo(Path.Combine("Samples", "sample.avi"));
            var sut = new SimpleFileStream(fi);
            sut.Seek(position);

            // Act
            var actualBlock = sut.Read();
            var actualMd5Hex = actualBlock.Hash(HashType.Md5).Encode(Codec.ByteHex);

            // Assert
            actualMd5Hex.Should().Be(expectedMd5Hex);
        }
    }
}
