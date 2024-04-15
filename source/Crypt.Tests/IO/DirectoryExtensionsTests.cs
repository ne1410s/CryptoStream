// <copyright file="DirectoryExtensionsTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Crypt.Tests.IO;

using Crypt.Encoding;
using Crypt.Hashing;
using Crypt.IO;
using Crypt.Tests.TestObjects;
using Crypt.Transform;

/// <summary>
/// Tests for <see cref="DirectoryExtensions"/>.
/// </summary>
public class DirectoryExtensionsTests
{
    [Fact]
    public void HashSum_NullDir_ThrowsException()
    {
        // Arrange
        var di = (DirectoryInfo)null!;

        // Act
        var act = () => di.HashSum(HashType.Sha256);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void HashSum_NoContents_ReturnsExpected()
    {
        // Arrange
        var di = new DirectoryInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}"));
        di.Create();

        // Act
        var hashSumBase64 = di.HashSum(HashType.Sha256).Encode(Codec.ByteBase64);

        // Assert
        hashSumBase64.Should().Be("47DEQpj8HBSa+/TImW+5JCeuQeRkm5NMpJWZG3hSuFU=");
    }

    [Theory]
    [InlineData("empty-sub-directory")]
    [InlineData("dir1")]
    public void HashSum_FileContentsOnlyWithVaryingStructure_DoesNotAffectResult(string folderName)
    {
        // Arrange
        var di = new DirectoryInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}"));
        di.Create();
        di.CreateSubdirectory(folderName);
        const string expected = "Xfbg4nYTWdMKgnUFjimfzAOBU0VF9Vz0PkGYP11MlFY=";

        // Act
        var hashSumBase64 = di.HashSum(HashType.Sha256, HashSumIncludes.FileContents).Encode(Codec.ByteBase64);

        // Assert
        hashSumBase64.Should().Be(expected);
    }

    [Fact]
    public void HashSum_IncludeRootWithSameNames_SameResult()
    {
        // Arrange
        var di1 = new DirectoryInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}", "dir"));
        var di2 = new DirectoryInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}", "dir"));
        di1.Create();
        di2.Create();

        // Act
        var hashSum1Base64 = di1.HashSum(HashType.Sha256, HashSumIncludes.DirectoryRootName).Encode(Codec.ByteBase64);
        var hashSum2Base64 = di2.HashSum(HashType.Sha256, HashSumIncludes.DirectoryRootName).Encode(Codec.ByteBase64);

        // Assert
        hashSum1Base64.Should().Be(hashSum2Base64);
    }

    [Fact]
    public void HashSum_IncludeRootWithDifferentNames_DifferentResult()
    {
        // Arrange
        var di1 = new DirectoryInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}", "d1"));
        var di2 = new DirectoryInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}", "d2"));
        di1.Create();
        di2.Create();

        // Act
        var hashSum1Base64 = di1.HashSum(HashType.Sha256, HashSumIncludes.DirectoryRootName).Encode(Codec.ByteBase64);
        var hashSum2Base64 = di2.HashSum(HashType.Sha256, HashSumIncludes.DirectoryRootName).Encode(Codec.ByteBase64);

        // Assert
        hashSum1Base64.Should().NotBe(hashSum2Base64);
    }

    [Theory]
    [InlineData("empty-sub-directory", "vBqLvFKGI3XOzud23bMKR67pFVHvCH2n9eLYF48m3+c=")]
    [InlineData("dir1", "G4RfKa487nE0W7iL8vefAL9Z4bpMy6EXcRO4PbUdE44=")]
    public void HashSum_IncludeStrutureWithVaryingStructure_AffectsResult(string folderName, string expected)
    {
        // Arrange
        var di = new DirectoryInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}"));
        di.Create();
        di.CreateSubdirectory(folderName);

        // Act
        var hashSumBase64 = di.HashSum(HashType.Sha256, HashSumIncludes.DirectoryStructure).Encode(Codec.ByteBase64);

        // Assert
        hashSumBase64.Should().Be(expected);
    }

    [Theory]
    [InlineData("hi", "kvP4YR5mIDmnoL4HoH+nq5boOZSfOeDK2VCDIHN3dTg=")]
    [InlineData("ho", "M2d/K5W1bwlekSs9vP22bAzPoVcpqa7sEvYnY6jDFxo=")]
    public void HashSum_ChangingContentsOfNestedFile_AffectsResult(string fileText, string expected)
    {
        // Arrange
        var di = new DirectoryInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}"));
        di.Create();
        di.CreateSubdirectory("mydir");
        File.WriteAllText(Path.Combine(di.FullName, "mydir", "file.txt"), fileText);

        // Act
        var hashSumBase64 = di.HashSum(HashType.Sha256).Encode(Codec.ByteBase64);

        // Assert
        hashSumBase64.Should().Be(expected);
    }

    [Fact]
    public void HashSum_ChangeFileTimestampButNotIncluded_SameResult()
    {
        // Arrange
        var di = new DirectoryInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}"));
        const string expected = "W6p5HeRHCryGjNCIXxTD+7tEVH/EwU5NpI8q9oCkW74=";
        di.Create();

        // Act
        File.WriteAllText(Path.Combine(di.FullName, "file.txt"), "hi!");
        var hashSum1Base64 = di.HashSum(HashType.Sha256).Encode(Codec.ByteBase64);
        File.WriteAllText(Path.Combine(di.FullName, "file.txt"), "hi!");
        var hashSum2Base64 = di.HashSum(HashType.Sha256).Encode(Codec.ByteBase64);

        // Assert
        hashSum1Base64.Should().Be(hashSum2Base64).And.Be(expected);
    }

    [Fact]
    public async Task HashSum_ChangeFileTimestampsIncluded_DifferentResult()
    {
        // Arrange
        var di = new DirectoryInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}"));
        di.Create();

        // Act
        await File.WriteAllTextAsync(Path.Combine(di.FullName, "file.txt"), "hi!");
        var hashSum1Base64 = di.HashSum(HashType.Sha256, HashSumIncludes.FileTimestamp).Encode(Codec.ByteBase64);
        await Task.Delay(1100);
        await File.WriteAllTextAsync(Path.Combine(di.FullName, "file.txt"), "hi!");
        var hashSum2Base64 = di.HashSum(HashType.Sha256, HashSumIncludes.FileTimestamp).Encode(Codec.ByteBase64);

        // Assert
        hashSum1Base64.Should().NotBe(hashSum2Base64);
    }

    [Theory]
    [InlineData(false, 1)]
    [InlineData(true, 2)]
    public void EncryptAllInSitu_VaryingRecurseFlag_FindsExpectedFiles(bool recurse, int expectedCount)
    {
        // Arrange
        var mockEncryptor = GetMockEncryptor();
        var di = new DirectoryInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}"));
        di.Create();
        File.WriteAllText(Path.Combine(di.FullName, "howdy.txt"), "howdy");
        di.CreateSubdirectory("mydir");
        File.WriteAllText(Path.Combine(di.FullName, "mydir", "ho.txt"), "ho");

        // Act
        di.EncryptAllInSitu(TestRefs.TestKey, recurse, encryptor: mockEncryptor.Object);

        // Assert
        mockEncryptor.Verify(
            m => m.Encrypt(
                It.IsAny<Stream>(),
                It.IsAny<Stream>(),
                TestRefs.TestKey,
                It.IsAny<Dictionary<string, string>>(),
                32768,
                null),
            Times.Exactly(expectedCount));
    }

    [Fact]
    public void EncryptAllInSitu_NullDir_ThrowsException()
    {
        // Arrange
        var mockEncryptor = new Mock<IEncryptor>();
        var di = (DirectoryInfo)null!;

        // Act
        var act = () => di.EncryptAllInSitu(TestRefs.TestKey, encryptor: mockEncryptor.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void EncryptAllInSitu_AlreadySecure_NotProcessed()
    {
        // Arrange
        var folder = Guid.NewGuid().ToString();
        Directory.CreateDirectory(folder);
        var mockEncryptor = GetMockEncryptor();
        const string secureName = "2fbdd1cbdb5f317b7e21ebb7ae7c32d166feec3be76b64d470123bf4d2c06ae5.avi";
        File.Copy(Path.Combine("TestObjects", "pixel.png"), Path.Combine(folder, "pixel.png"));
        File.Copy(Path.Combine("TestObjects", secureName), Path.Combine(folder, secureName));

        // Act
        new DirectoryInfo(folder).EncryptAllInSitu(TestRefs.TestKey, encryptor: mockEncryptor.Object);

        // Assert
        mockEncryptor.Verify(
            m => m.Encrypt(
                It.IsAny<Stream>(),
                It.IsAny<Stream>(),
                It.IsAny<byte[]>(),
                It.IsAny<Dictionary<string, string>>(),
                It.IsAny<int>(),
                It.IsAny<Stream>()),
            Times.Once());
    }

    [Fact]
    public void EncryptAllInSitu_WithPredicate_FindsExpectedFiles()
    {
        // Arrange
        var folder = Guid.NewGuid().ToString();
        Directory.CreateDirectory(folder);
        var mockEncryptor = GetMockEncryptor();
        File.Copy(Path.Combine("TestObjects", "pixel.png"), Path.Combine(folder, "pixel.png"));
        File.Copy(Path.Combine("TestObjects", "pixel.png"), Path.Combine(folder, "otherfile.png"));
        static bool Filter(FileInfo fi) => fi.Name.StartsWith("pixel", StringComparison.Ordinal);

        // Act
        new DirectoryInfo(folder).EncryptAllInSitu(TestRefs.TestKey, where: Filter, encryptor: mockEncryptor.Object);

        // Assert
        mockEncryptor.Verify(
            m => m.Encrypt(
                It.IsAny<Stream>(),
                It.IsAny<Stream>(),
                It.IsAny<byte[]>(),
                It.IsAny<Dictionary<string, string>>(),
                It.IsAny<int>(),
                It.IsAny<Stream>()),
            Times.Once());
    }

    private static Mock<IEncryptor> GetMockEncryptor()
    {
        var mockEncryptor = new Mock<IEncryptor>();
        mockEncryptor
            .Setup(m => m.Encrypt(
                It.IsAny<Stream>(),
                It.IsAny<Stream>(),
                It.IsAny<byte[]>(),
                It.IsAny<Dictionary<string, string>>(),
                It.IsAny<int>(),
                It.IsAny<Stream>()))
            .Returns(Guid.NewGuid().ToByteArray());
        return mockEncryptor;
    }
}
