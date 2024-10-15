using Common.UnitTests;
using Toll.Core.Common;
using Xunit.Abstractions;

namespace Toll.Core.UnitTests;


public class BitRangeUnitTests(ITestOutputHelper tempOutput) : OutputConsoleHelper(tempOutput)
{
    
    [Theory]
    [InlineData(-1, 10)]
    [InlineData(10, -10)]
    [InlineData(10, 5)]
    public void BitRange_Constructor_Abnormal(int startBit, int endBit)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => { var _ = new BitRange(startBit, endBit);});
        // Assert.Equal("EndBit", exception.ParamName);
    }

    [Theory]
    [InlineData(10, 10,1)]
    [InlineData(10, 12,3)]
    public void BitRange_Length(int startBit, int endBit,int actualLength)
    {
        var bitRange = new BitRange(startBit, endBit);
        Assert.Equal(bitRange.Length, actualLength);
    }
    
    [Fact]
    public void BitRange_ToString()
    {
        var bitRange = new BitRange(0, 1);
        Output.WriteLine(bitRange.ToString());
    }

    [Fact]
    public void BitRange_OperaOrEquals()
    {
        var bitRange1 = new BitRange(0, 10);
        var bitRange2 = new BitRange(0, 10);
        var bitRange3 = new BitRange(0, 12);
        Assert.Equal(bitRange1, bitRange2);
        Assert.NotEqual(bitRange1,bitRange3);
        
        Assert.True(bitRange1 == bitRange2);
        Assert.False(bitRange1 == bitRange3);
        Assert.True(bitRange1 != bitRange3);
        
        Assert.True(bitRange1.Equals(bitRange2));
        Assert.False(bitRange1.Equals(bitRange3));
        
        Assert.False(bitRange1.Equals(null));
        object? obj = bitRange2;
        Assert.True(bitRange1.Equals(obj));
        obj = bitRange3;
        Assert.False(bitRange1.Equals(obj));
    }
    
}