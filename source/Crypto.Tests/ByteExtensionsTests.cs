namespace Crypto.Tests;

/// <summary>
/// Tests for the <see cref="ByteExtensions"/> class.
/// </summary>
public class ByteExtensionsTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(255)]
    [InlineData(40442)]
    [InlineData(-1)]
    public void CounterIncrement_SmallNumbers(int initial)
    {
        // Arrange
        var counter = BitConverter.GetBytes(initial);
        var expected = initial + 1;

        // Act
        ByteExtensions.Increment(ref counter);

        // Assert
        var actual = BitConverter.ToInt32(counter);
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(1, 255, true)]
    [InlineData(24, 255, true)]
    [InlineData(24, 254, false)]
    [InlineData(0, 0, true)]
    public void CounterIncrement_Resize(int initialSize, byte fill, bool expectResize)
    {
        // Arrange
        var expectedFinalSize = expectResize ? initialSize + 1 : initialSize;
        var counter = new byte[initialSize];
        Array.Fill(counter, fill);

        // Act
        ByteExtensions.Increment(ref counter);

        // Assert
        counter.Length.Should().Be(expectedFinalSize);
    }

    [Theory]
    [InlineData(0, true, true)]
    [InlineData(1, true, true)]
    [InlineData(1, false, false)]
    [InlineData(24, true, true)]
    [InlineData(9623, false, false)]
    public void CounterIncrement_Endianness(int initialSize, bool bigEndian, bool expectAppend)
    {
        // Arrange
        var counter = new byte[initialSize];
        Array.Fill(counter, byte.MaxValue);

        // Act
        ByteExtensions.Increment(ref counter, bigEndian);

        // Assert
        var expectZeroAt = expectAppend ? counter.Length - 1 : 0;
        var expectOneAt = expectAppend ? 0 : counter.Length - 1;
        if (initialSize != 0)
        {
            counter[expectZeroAt].Should().Be(0);
        }

        counter[expectOneAt].Should().Be(1);
    }

    [Theory]
    [InlineData(500, true, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 244 })]
    [InlineData(500, false, new byte[] { 244, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 })]
    public void Pad12_Endianness(long number, bool bigEndian, byte[] expected)
    {
        // Arrange & Act
        var result = number.Pad12(bigEndian);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }
}
