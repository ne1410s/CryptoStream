// <copyright file="SimpleFileStreamTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

using Crypt.Encoding;
using Crypt.Hashing;
using Crypt.Streams;
using Crypt.Utils;

namespace Crypt.Tests.Streams
{
    /// <summary>
    /// Tests for the <see cref="SimpleFileStream"/>.
    /// </summary>
    public class SimpleFileStreamTests
    {
        [Fact]
        public void Ctor_WithFile_UriIsPath()
        {
            // Arrange
            var fi = new FileInfo(Path.Combine("TestObjects", "sample.avi"));

            // Act
            var sut = new SimpleFileStream(fi);

            // Assert
            sut.Uri.Should().Be(fi.FullName);
        }

        [Fact]
        public void Ctor_SpecificMedia_HashExpected()
        {
            // Arrange
            var fi = new FileInfo(Path.Combine("TestObjects", "sample.avi"));
            var sut = new SimpleFileStream(fi);
            const string expectedMd5Hex = "91d326694fdff83d0df74c357f3feb84";

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
            var fi = new FileInfo(Path.Combine("TestObjects", "sample.avi"));
            var sut = new SimpleFileStream(fi);
            sut.Seek(position);

            // Act
            var actualBlock = sut.Read();
            var actualMd5Hex = actualBlock.Hash(HashType.Md5).Encode(Codec.ByteHex);

            // Assert
            actualMd5Hex.Should().Be(expectedMd5Hex);
        }

        [Fact]
        public void Read_OversizedBuffer_ResizesBuffer()
        {
            // Arrange
            var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
            File.WriteAllText(fi.FullName, "this is a string that is of some size.");
            const int bufferLength = 1024;
            var sut = new SimpleFileStream(fi, bufferLength);
            sut.Seek(fi.Length - 9);

            // Act
            var block = sut.Read();

            // Assert
            block.Length.Should().Be(9);
        }

        [Fact]
        public void Read_PerfectFitBuffer_NotResized()
        {
            // Arrange
            var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
            File.WriteAllText(fi.FullName, "hello here is a string");
            var bufferLength = fi.Length;
            var mockResizer = new Mock<IArrayResizer>();
            var sut = new SimpleFileStream(fi, (int)bufferLength, mockResizer.Object);

            // Act
            _ = sut.Read();

            // Assert
            mockResizer.Verify(
                m => m.Resize(ref It.Ref<byte[]>.IsAny, It.IsAny<int>()),
                Times.Never);
        }
    }
}
