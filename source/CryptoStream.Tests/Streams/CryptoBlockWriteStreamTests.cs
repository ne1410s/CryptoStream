// <copyright file="CryptoBlockWriteStreamTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace CryptoStream.Tests.Streams;

using CryptoStream.Encoding;
using CryptoStream.Hashing;
using CryptoStream.IO;
using CryptoStream.Streams;
using CryptoStream.Tests.TestObjects;
using CryptoStream.Transform;

/// <summary>
/// Tests for the <see cref="CryptoBlockWriteStream"/> class.
/// </summary>
public class CryptoBlockWriteStreamTests
{
    [Fact]
    public void SmallFileE2E_ProducesExpected()
    {
        // Arrange
        var fi1 = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi1.FullName, "hello here is a string");
        var clearBytes = File.ReadAllBytes(fi1.FullName);
        var saltHex = fi1.EncryptInSitu(TestRefs.TestKey);
        var salt = saltHex.Decode(Codec.ByteHex);
        var msEnc = new MemoryStream();
        var metadata = new Dictionary<string, string> { ["filename"] = "test.xyz" };
        using var cbws = new CryptoBlockWriteStream(msEnc, metadata, salt, TestRefs.TestKey);

        // Act
        cbws.Write(clearBytes);
        cbws.WriteFinal();

        // Assert
        var msDec = new MemoryStream();
        var md = new AesGcmDecryptor().Decrypt(msEnc, msDec, TestRefs.TestKey, salt, metadata.Count > 0);
        msDec.ToArray().Should().BeEquivalentTo(clearBytes);
        md.Should().BeEquivalentTo(metadata);
    }

    [Fact]
    public void MediumFileE2E_ProducesExpected()
    {
        // Arrange
        var folder = Guid.NewGuid().ToString();
        try
        {
            Directory.CreateDirectory(folder);
            const string secureName = "2fbdd1cbdb5f317b7e21ebb7ae7c32d166feec3be76b64d470123bf4d2c06ae5.03470a9848";
            var sourceInfo = new FileInfo(Path.Combine(folder, secureName));
            var sourceSalt = sourceInfo.ToSalt();
            File.Copy(Path.Combine("TestObjects", secureName), sourceInfo.FullName);
            var clearInfo = sourceInfo.DecryptHere(TestRefs.TestKey);
            var clearMd5 = clearInfo.Hash(HashType.Md5).Encode(Codec.ByteHex);
            var targetInfo = new FileInfo(Path.Combine(folder, "temp.123"));
            using var fsProc = targetInfo.OpenWrite();
            using var cbws = new CryptoBlockWriteStream(
                fsProc, new() { ["filename"] = "sample.avi" }, sourceSalt, TestRefs.TestKey);

            // Act
            var buffer = new byte[32768];
            int bytesRead;
            using var fsClear = clearInfo.OpenRead();
            while ((bytesRead = fsClear.Read(buffer, 0, buffer.Length)) > 0)
            {
                cbws.Write(buffer, 0, bytesRead);
            }

            cbws.WriteFinal();
            cbws.Reset();
            cbws.Close();
            fsClear.Close();
            Directory.CreateDirectory(Path.Combine(folder, "subdir"));
            targetInfo.MoveTo(Path.Combine(folder, "subdir", cbws.Name));

            var clearInfo2 = targetInfo.DecryptHere(TestRefs.TestKey);
            var clearMd5_2 = clearInfo2.Hash(HashType.Md5).Encode(Codec.ByteHex);

            // Assert
            clearMd5_2.Should().Be(clearMd5);
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }
}
