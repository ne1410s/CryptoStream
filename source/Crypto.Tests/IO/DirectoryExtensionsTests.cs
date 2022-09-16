using Crypto.Encoding;
using Crypto.Hashing;
using Crypto.IO;
using Crypto.Tests.TestObjects;
using Crypto.Transform;

namespace Crypto.Tests.IO;

/// <summary>
/// Tests for <see cref="DirectoryExtensions"/>.
/// </summary>
public class DirectoryExtensionsTests
{
    [Fact]
    public void HashSum_NoContents_ReturnsExpected()
    {
        // Arrange
        var di = new DirectoryInfo(Path.Combine("TestFiles", $"{Guid.NewGuid()}"));
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
        var di = new DirectoryInfo(Path.Combine("TestFiles", $"{Guid.NewGuid()}"));
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
        var di1 = new DirectoryInfo(Path.Combine("TestFiles", $"{Guid.NewGuid()}", "dir"));
        var di2 = new DirectoryInfo(Path.Combine("TestFiles", $"{Guid.NewGuid()}", "dir"));
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
        var di1 = new DirectoryInfo(Path.Combine("TestFiles", $"{Guid.NewGuid()}", "d1"));
        var di2 = new DirectoryInfo(Path.Combine("TestFiles", $"{Guid.NewGuid()}", "d2"));
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
        var di = new DirectoryInfo(Path.Combine("TestFiles", $"{Guid.NewGuid()}"));
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
        var di = new DirectoryInfo(Path.Combine("TestFiles", $"{Guid.NewGuid()}"));
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
        var di = new DirectoryInfo(Path.Combine("TestFiles", $"{Guid.NewGuid()}"));
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
    public void HashSum_ChangeFileTimestampsIncluded_DifferentResult()
    {
        // Arrange
        var di = new DirectoryInfo(Path.Combine("TestFiles", $"{Guid.NewGuid()}"));
        di.Create();

        // Act
        File.WriteAllText(Path.Combine(di.FullName, "file.txt"), "hi!");
        var hashSum1Base64 = di.HashSum(HashType.Sha256, HashSumIncludes.FileTimestamp).Encode(Codec.ByteBase64);
        Thread.Sleep(1100);
        File.WriteAllText(Path.Combine(di.FullName, "file.txt"), "hi!");
        var hashSum2Base64 = di.HashSum(HashType.Sha256, HashSumIncludes.FileTimestamp).Encode(Codec.ByteBase64);

        // Assert
        hashSum1Base64.Should().NotBe(hashSum2Base64);
    }

    [Fact]
    public void EncryptAllInSitu_WithNesting_FindsAllFiles()
    {
        // Arrange
        var mockEncryptor = new Mock<IEncryptor>();
        var di = new DirectoryInfo(Path.Combine("TestFiles", $"{Guid.NewGuid()}"));
        di.Create();
        File.WriteAllText(Path.Combine(di.FullName, "howdy.txt"), "howdy");
        di.CreateSubdirectory("mydir");
        File.WriteAllText(Path.Combine(di.FullName, "mydir", "ho.txt"), "ho");

        // Act
        di.EncryptAllInSitu(TestRefs.TestKey, mockEncryptor.Object);

        // Assert
        mockEncryptor.Verify(
            m => m.Encrypt(It.IsAny<Stream>(), It.IsAny<Stream>(), TestRefs.TestKey, 32768, null),
            Times.Exactly(2));
    }
}
