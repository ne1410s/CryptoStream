using Crypto.IO;
using Crypto.Tests.TestObjects;

namespace Crypto.Tests.IO;

/// <summary>
/// Tests for <see cref="FileExtensions"/>
/// </summary>
public class FileExtensionsTests
{
    [Fact]
    public void EncryptInSitu_WithFile_UpdatesFileInfoReference()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestFiles", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, "hi");

        // Act
        var salt = fi.EncryptInSitu(TestRefs.TestKey);

        // Assert
        fi.Name.Should().Be(salt + ".txt");
        fi.Exists.Should().BeTrue();
    }
}
