﻿using Common.UnitTests;
using Toll.Core.Shared.Collections;
using Xunit.Abstractions;

namespace Toll.Core.UnitTests;

public class CircularBuffUnitTests(ITestOutputHelper tempOutput) : OutputConsoleHelper(tempOutput)
{
    private int[] data1 = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17];
    
    [Fact]
    public void CircularBuff_Constructor()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => { _ = new CircularBuffer<int>(-10);});
        var buff = new CircularBuffer<int>();
        Assert.Equal(CircularBuffer<int>.DefaultInitCapacity, buff.Capacity);
        
        buff = new CircularBuffer<int>(0);
        Assert.Equal(0, buff.Capacity);
        
        buff = new CircularBuffer<int>(20);
        Assert.Equal(20, buff.Capacity);
        // Assert.ThrowsAny<ArgumentOutOfRangeException>(() => { _ = new CircularBuffer<int>(10);}); 捕捉任何一个
        // Assert.Equal("EndBit", exception.ParamName);
    }

    [Fact]
    public void CircularBuffer_TestGrow()
    {
        var buff = new CircularBuffer<int>();

        int 需要循环的次数 = 10;
        buff.MaxCapacity = 5;
        int lastCapacity = buff.Capacity;
        for (int index = 0; index < 需要循环的次数; index++)
        {
            // Output.WriteLine($"be capacity:{lastCapacity}");
            buff.Add(index);
            Assert.True(buff.Capacity >= lastCapacity);
            Assert.True(buff.Capacity <= buff.MaxCapacity);
            lastCapacity = buff.Capacity;
            // Output.WriteLine($"en capacity:{lastCapacity}");
        }
    }
    
    [Theory]
    [InlineData(0, 1)]
    [InlineData(10, 4)]
    [InlineData(5, 5)]
    [InlineData(5, 6)]
    public void CircularBuff_Count_Capacity(int capacity,int insertCount)
    {
        var buff = new CircularBuffer<int>(capacity);
        Assert.Equal(capacity, buff.Capacity);
        Assert.True(0 == buff.Count);
        for (int i = 1; i <= insertCount; i++)
        {
            buff.Add(i);
        }

        foreach (var item in buff)//测试迭代器
        {
            Output.WriteLine(item.ToString());
        }
        Output.WriteLine("------ capacity changed.---");
        buff.SetCapacity(4);
        Assert.True(4 == buff.Capacity);
        foreach (var item in buff)//测试迭代器
        {
            Output.WriteLine(item.ToString());
        }
        
        buff.Clear();
        Assert.True(0 == buff.Count);
        Assert.True(4 == buff.Capacity);
    }

    [Fact]
    public void CircularBuff_Contains()
    {
        var buff = new CircularBuffer<string>(10);
        string[] items = { "one", "two", "three", "four", "five", "six" };
        foreach (var item in items)
        {
            buff.Add(item);
        }
        Assert.True(buff.Contains("three"));
        Assert.False(buff.Contains("f3s"));
    }

    [Fact]
    public void CircularBuff_TestIsFull()
    {
        var buff = new CircularBuffer<string>(3);
        Assert.False(buff.IsFull);
        buff.Add("one");
        Assert.False(buff.IsFull);
        buff.Add("one");
        Assert.False(buff.IsFull);
        buff.Add("one");
        Assert.True(buff.IsFull);
        buff.Add("one");
        Assert.True(buff.IsFull);
        buff.Clear();
        Assert.False(buff.IsFull);
    }


    [Fact]
    public void CircularBuff_TestCopyArray()
    {
        var buff = new CircularBuffer<int>(10);
        int[] ints = { 1, 2, 3, 4, 5 };
        foreach (var item in ints)
        {
            buff.Add(item);
        }
        var copy = buff.ToArray();
        Assert.Equal(ints, copy);
        
        var copy2 = new int[10];
        Array.Fill(copy2, 99);
        buff.CopyTo(copy2, 2);
        foreach (var item in copy2)
        {
            Output.WriteLine(item.ToString());
        }

        int[]? copy3 = null;
        Assert.Throws<ArgumentNullException>(() => { buff.CopyTo(copy3, 0); });
        copy3 = new int[2];
        Assert.Throws<ArgumentOutOfRangeException>(() => { buff.CopyTo(copy3, -1); });
        Assert.Throws<ArgumentOutOfRangeException>(() => { buff.CopyTo(copy3, 2); });
        copy3 = new int[6];
        Assert.Throws<ArgumentException>(() => { buff.CopyTo(copy3, 2); });
        
        buff = new CircularBuffer<int>(4);
        int[] ints2 = { 1, 2, 3, 4};
        foreach (var item in ints2)
        {
            buff.Add(item);
        }
        int[]? copyArray = new int[4];
        buff.CopyTo(copyArray, 0); 
        Assert.Equal(ints2, copyArray);
        
        buff.Add(5);
        buff.CopyTo(copyArray, 0); 
        Assert.NotEqual(copyArray, ints2);
        Assert.Equal(copyArray, new int[]{2,3,4,5});
        
        // foreach (var item in copy3)
        // {
        //     Output.WriteLine(item.ToString());
        // }
        
        // var copy3 = new int[4];
        
        // Assert.ThrowsAny<Exception>(() => {buff.CopyTo(copy3, 0); });
    }
    
}

