namespace Crypto.Tests;

public class ByteExtensionsTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(255)]
    [InlineData(40442)]
    [InlineData(-1)]
    public void CounterIncrement_SmallNumbers(int initial)
    {
        var counter = BitConverter.GetBytes(initial);
        ByteExtensions.Increment(ref counter);
        var expected = initial + 1;
        var actual = BitConverter.ToInt32(counter);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(1, 255, true)]
    [InlineData(24, 255, true)]
    [InlineData(24, 254, false)]
    [InlineData(0, 0, true)]
    public void CounterIncrement_Resize(int initialSize, byte fill, bool expectResize)
    {
        var expectedFinalSize = expectResize ? initialSize + 1 : initialSize;
        var counter = new byte[initialSize];
        Array.Fill(counter, fill);
        ByteExtensions.Increment(ref counter);

        Assert.Equal(expectedFinalSize, counter.Length);
    }

    [Theory]
    [InlineData(0, true, true)]
    [InlineData(1, true, true)]
    [InlineData(1, false, false)]
    [InlineData(24, true, true)]
    [InlineData(9623, false, false)]
    public void CounterIncrement_Endianness(int initialSize, bool bigEndian, bool expectAppend)
    {
        var counter = new byte[initialSize];
        Array.Fill(counter, byte.MaxValue);
        ByteExtensions.Increment(ref counter, bigEndian);
        var expectZeroAt = expectAppend ? counter.Length - 1 : 0;
        var expectOneAt = expectAppend ? 0 : counter.Length - 1;

        if (initialSize != 0)
        {
            Assert.Equal(0, counter[expectZeroAt]);
        }

        Assert.Equal(1, counter[expectOneAt]);
    }
}
