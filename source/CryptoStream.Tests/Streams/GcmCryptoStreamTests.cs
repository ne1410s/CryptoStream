// <copyright file="GcmCryptoStreamTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace CryptoStream.Tests.Streams;

using CryptoStream.Encoding;
using CryptoStream.Hashing;
using CryptoStream.IO;
using CryptoStream.Streams;
using CryptoStream.Tests.TestObjects;

/// <summary>
/// Tests for the <see cref="GcmCryptoStream"/> class.
/// </summary>
public class GcmCryptoStreamTests
{
    [Fact]
    public void Write_WhenFinalised_ProducesExpected()
    {
        // Arrange
        var testRef = Guid.NewGuid();
        var controlFi = new FileInfo($"{testRef}_write_control.avi");
        var testFiSrc = new FileInfo($"{testRef}_write_test_src.avi");
        var testFiTrg = new FileInfo($"{testRef}_write_test_trg.avi");
        File.Copy("TestObjects/sample.avi", controlFi.FullName);
        File.Copy("TestObjects/sample.avi", testFiSrc.FullName);
        var controlMd5 = controlFi.Hash(HashType.Md5).Encode(Codec.ByteHex);
        var salt = controlFi.EncryptInSitu(TestRefs.TestKey).Decode(Codec.ByteHex);
        controlFi.Delete();
        var sut = testFiTrg.OpenWrite(salt, TestRefs.TestKey);
        var testSrc = testFiSrc.OpenRead();

        // Act
        testSrc.CopyTo(sut, sut.BufferLength);
        sut.FinaliseWrite();
        sut.Close();
        sut.Dispose();
        testSrc.Dispose();
        var finalFi = testFiTrg.CopyTo(sut.Id);
        var clearFi = finalFi.DecryptHere(TestRefs.TestKey);
        var testMd5 = clearFi.Hash(HashType.Md5).Encode(Codec.ByteHex);

        // Assert
        testMd5.Should().Be(controlMd5);
        testFiSrc.Delete();
        testFiTrg.Delete();
        finalFi.Delete();
        clearFi.Delete();
    }
}
