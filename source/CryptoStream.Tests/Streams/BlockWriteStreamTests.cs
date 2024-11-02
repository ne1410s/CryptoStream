// <copyright file="BlockWriteStreamTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace CryptoStream.Tests.Streams;

using CryptoStream.Encoding;
using CryptoStream.Hashing;
using CryptoStream.IO;
using CryptoStream.Streams;

/// <summary>
/// Tests for the <see cref="BlockWriteStream"/>.
/// </summary>
public class BlockWriteStreamTests
{
    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(1024)]
    public void Write_VaryingBufferLength_GivesSameResult(int bufferLength)
    {
        // Arrange
        const string expectedMd5 = "165f43c3ac341e4defb924b38c9fceb5";
        var sink = new MemoryStream();
        var content = new byte[] { 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 };
        using var sut = new BlockWriteStream(sink, bufferLength);

        // Act
        var written = sut.Write(content);

        // Assert
        written.Should().Be(content.Length);
        content.Hash(HashType.Md5).Encode(Codec.ByteHex).Should().Be(expectedMd5);
    }

    [Fact]
    public void Write_MediumFile_HashesExpected()
    {
        // Arrange
        var folder = Guid.NewGuid().ToString();
        Directory.CreateDirectory(folder);
        var sourceInfo = new FileInfo(Path.Combine(folder, "sample.avi"));
        File.Copy(Path.Combine("TestObjects", "sample.avi"), sourceInfo.FullName);
        using var fs = File.OpenWrite(Path.Combine(folder, "sample2.avi"));
        using var sut = new BlockWriteStream(fs);

        // Act
        var buffer = new byte[32768];
        int bytesRead;
        using var fsClear = sourceInfo.OpenRead();
        while ((bytesRead = fsClear.Read(buffer, 0, buffer.Length)) > 0)
        {
            sut.Write(buffer, 0, bytesRead);
        }

        fsClear.Close();
        fs.Close();

        // Assert
        var sourceInfo2 = new FileInfo(Path.Combine(folder, "sample2.avi"));
        var md5Hex = sourceInfo2.Hash(HashType.Md5).Encode(Codec.ByteHex);
        md5Hex.Should().Be("91d326694fdff83d0df74c357f3feb84");
    }

    [Fact]
    public void WriteFinal_WhenCalled_DoesNotThrow()
    {
        // Arrange
        using var sut = new BlockWriteStream(new MemoryStream(), 123);

        // Act
        var act = sut.WriteFinal;

        // Assert
        act.Should().NotThrow();
        sut.BufferLength.Should().Be(123);
    }
}
