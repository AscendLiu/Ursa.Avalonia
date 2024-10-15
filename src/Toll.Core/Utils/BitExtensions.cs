using Toll.Core.Common;

namespace Toll.Core.Utils;

/// <summary>
/// 根据bit的范围对变量进行设置或获取
/// </summary>
public static class BitExtensions
{
    /// <summary>
    /// 获得一个uint变量的指定范围的Bit并返回这些比特组成的数，范围从0开始
    /// </summary>
    /// <param name="number"></param>
    /// <param name="startBit"></param>
    /// <param name="endBit"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static uint Bits(this uint number, int startBit, int endBit)
    {
        if (startBit < 0 || endBit >= sizeof(uint) * 8 || startBit > endBit)
        {
            throw new ArgumentOutOfRangeException("Invalid start or end bit range.");
        }

        uint mask = ((1u << (endBit - startBit + 1)) - 1) << startBit;
        return (number & mask) >> startBit;
    }

    /// <summary>
    /// 获得一个uint变量的指定范围的Bit并返回这些比特组成的数，范围从0开始
    /// </summary>
    /// <param name="number"></param>
    /// <param name="bitRange"></param>
    /// <returns></returns>
    public static uint Bits(this uint number, BitRange bitRange) => Bits(number, bitRange.StartBit, bitRange.EndBit);

    /// <summary>
    /// 获得byte的指定比特位:1(true) Or 0(false)
    /// </summary>
    /// <param name="number"></param>
    /// <param name="bitIndex"></param>
    /// <returns></returns>
    public static bool Bit(this byte number, int bitIndex)
    {
        if (bitIndex < 0 || bitIndex > 7)
        {
            throw new ArgumentOutOfRangeException(nameof(bitIndex), "Bit index must be between 0 and 7 for a byte.");
        }
        return ((number >> bitIndex) & 1) == 1;
    }

    public static byte[] BitsArry(this uint number, BitRange bitRange)
    {
        if (bitRange.EndBit >= sizeof(uint) * 8)
        {
            throw new ArgumentOutOfRangeException("Invalid end bit range.");
        }

        var len = bitRange.Length;
        byte[] result = new byte[len];
        for (int index = 0, bitStart = bitRange.StartBit; index < len; index++, bitStart++)
        {
            result[index] = (byte)((number >> bitStart) & 1);
        }

        return result;
    }

    /// <summary>
    /// 用一个uint的变量去覆盖此uint变量的指定范围的比特，
    /// 特别说明：源比特的值始终从0开始，取范围大小的比特，再把这些比特拿去覆盖目标变量的指定范围的比特
    /// </summary>
    /// <param name="dstBits"></param>
    /// <param name="startBit"></param>
    /// <param name="endBit"></param>
    /// <param name="srcBits"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static uint SetBits(this ref uint dstBits, int startBit, int endBit, uint srcBits)
    {
        if (startBit < 0 || endBit >= sizeof(uint) * 8 || startBit > endBit)
        {
            throw new ArgumentOutOfRangeException("Invalid start or end bit range.");
        }

        uint mask = (1u << (endBit - startBit + 1)) - 1;
        srcBits = (srcBits & mask) << startBit;
        dstBits = (dstBits & ~(mask << startBit)) | srcBits;
        return dstBits;
    }
    
    public static byte SetBits(this ref byte dstBits, int index, bool bitValue)
    {
        if (index < 0 || index >= sizeof(byte) * 8)
        {
            throw new ArgumentOutOfRangeException("Invalid index.");
        }
        
        byte mask = (byte)(1 << index);
        if (bitValue)dstBits |= mask;  // Set the bit at the given index to 1
        else dstBits &= (byte)(~mask); // Set the bit at the given index to 0
        
        return dstBits;
    }
    
    public static ushort SetBits(this ref ushort dstBits, int index, bool bitValue)
    {
        if (index < 0 || index >= sizeof(ushort) * 8)
        {
            throw new ArgumentOutOfRangeException("Invalid index.");
        }
        
        ushort mask = (ushort)(1 << index);
        if (bitValue)dstBits |= mask;  // Set the bit at the given index to 1
        else dstBits &= (ushort)(~mask); // Set the bit at the given index to 0
        
        return dstBits;
    }
    
    public static uint SetBits(this ref uint dstBits, int index, bool bitValue)
    {
        if (index < 0 || index >= sizeof(uint) * 8)
        {
            throw new ArgumentOutOfRangeException("Invalid index.");
        }
        
        uint mask = 1u << index;
        if (bitValue)dstBits |= mask;  // Set the bit at the given index to 1
        else dstBits &= ~mask; // Set the bit at the given index to 0
        
        return dstBits;
    }
    
    public static ulong SetBits(this ref ulong dstBits, int index, bool bitValue)
    {
        if (index < 0 || index >= sizeof(ulong) * 8)
        {
            throw new ArgumentOutOfRangeException("Invalid index.");
        }
        
        ulong mask = 1ul << index;
        if (bitValue)dstBits |= mask;  // Set the bit at the given index to 1
        else dstBits &= ~mask; // Set the bit at the given index to 0
        
        return dstBits;
    }

    public static void OnlySetBits(this ref uint dstBits, int startBit, int endBit, uint srcBits)
    {
        if (startBit < 0 || endBit >= sizeof(uint) * 8 || startBit > endBit)
        {
            throw new ArgumentOutOfRangeException("Invalid start or end bit range.");
        }

        uint mask = (1u << (endBit - startBit + 1)) - 1;
        srcBits = (srcBits & mask) << startBit;
        dstBits = (dstBits & ~(mask << startBit)) | srcBits;
    }

    /// <summary>
    /// 用一个uint的变量去覆盖此uint变量的指定范围的比特，
    /// 特别说明：源比特的值始终从0开始，取范围大小的比特，再把这些比特拿去覆盖目标变量的指定范围的比特
    /// </summary>
    /// <param name="dstBits"></param>
    /// <param name="bitRange"></param>
    /// <param name="srcBits"></param>
    /// <returns></returns>
    public static uint SetBits(this ref uint dstBits, BitRange bitRange, uint srcBits) =>
        SetBits(ref dstBits, bitRange.StartBit, bitRange.EndBit, srcBits);

    /// <summary>
    /// 判断两个uint指定的同样范围的比特位是否全部相等，不再此范围内的比特不参与比较，相等则返回true
    /// </summary>
    /// <param name="num1"></param>
    /// <param name="num2"></param>
    /// <param name="bitRange"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static bool EqualBitRange(this uint num1, uint num2, BitRange bitRange)
    {
        if (bitRange.EndBit >= sizeof(uint) * 8)
        {
            throw new ArgumentException("Invalid bit range");
        }

        uint mask = ((1u << bitRange.Length) - 1) << bitRange.StartBit;
        return (num1 & mask) == (num2 & mask);
    }
}