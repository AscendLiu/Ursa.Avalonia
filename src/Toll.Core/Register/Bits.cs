using System.Text;
using Toll.Core.Common;
using Toll.Core.Utils;

namespace Toll.Core.Register;

/// <summary>
/// 使用bool[]数组代表每一个比特位
/// </summary>
public class Bits
{
    private bool[] bits;

    public int Count => bits.Length;
    public int Length
    {
        get => bits.Length;
        set
        {
            if(value < 0) throw new ArgumentOutOfRangeException(nameof(Length), value, "Length must be greater than zero.");
            Array.Resize(ref bits, value);
        }
    }

    /// <summary>
    /// 创建一个比特位为空的寄存器
    /// </summary>
    public Bits():this(0)
    {
        Int32 f = 12;
        var fd =Int32.MaxValue;
    }
    
    /// <summary>
    /// 使用指定比特长度和默认比特构造一个寄存器
    /// </summary>
    /// <param name="bitLength"></param>
    /// <param name="defaultValue"></param>
    public Bits(int bitLength,bool defaultValue = false)
    {
        bits = new bool[bitLength];
        for (int i = 0; i < bitLength; i++)
        {
            bits[i] = defaultValue;
        }
    }
    
    /// <summary>
    /// 使用bool代表Bit状态的数组初始化所有的比特
    /// </summary>
    /// <param name="boolBits"></param>
    public Bits(bool[] boolBits)
    {
        if(boolBits is null) throw new ArgumentNullException(nameof(boolBits));
        
        bits = new bool[boolBits.Length];
        Array.Copy(boolBits, bits, bits.Length);
    }
    
    
    /// <summary>
    /// 根据byte保存的bit信息进行初始化，bytes[0]表示位0-7，字节[1]表示位8-15，以此类推，每个字节的低位bit代表低位
    /// 初始化的Bit长度由外部指定，指定长度比初始化数组小，则超出的初始化Bit被抛弃，若初始化Bit不足指定的Bit长度则，没有初始化的Bit默认保持为Bit = 0
    /// </summary>
    /// <param name="bytes">用于初始化Bit信息的字节数组</param>
    /// <param name="bitLength">指定初始化的Bit长度，可以使用bytes.Length * 8进行指定</param>
    public Bits(byte[] bytes,int bitLength)
    {
        if(bytes is null) throw new ArgumentNullException(nameof(bytes));
        bits = new bool[bitLength];
        int initBitLength = bytes.Length * 8;
        for (int index = 0; index < bitLength && index < initBitLength; index++)
        {
            int initByteIndex = index / 8;
            int initBitIndex = index % 8;
            bits[index] = ((0b1 << initBitIndex) & bytes[initByteIndex]) != 0;
        }
    }
    
    /// <summary>
    /// 根据ushort保存的bit信息进行初始化，ushort[0]表示位0-15，字节[1]表示位16-31，以此类推，每个字节的低位bit代表低位
    /// 初始化的Bit长度由外部指定，指定长度比初始化数组小，则超出的初始化Bit被抛弃，若初始化Bit不足指定的Bit长度则，没有初始化的Bit默认保持为Bit = 0
    /// </summary>
    /// <param name="ushorts">用于初始化Bit信息的ushort数组</param>
    /// <param name="bitLength">指定初始化的Bit长度，可以使用 ushorts.Length * 16 进行指定</param>
    public Bits(ushort[] ushorts,int bitLength)
    {
        if(ushorts is null) throw new ArgumentNullException(nameof(ushorts));
        bits = new bool[bitLength];
        int initBitLength = ushorts.Length * 16;
        for (int index = 0; index < bitLength && index < initBitLength; index++)
        {
            int initUShortIndex = index / 16;
            int initBitIndex = index % 16;
            bits[index] = ((1u << initBitIndex) & ushorts[initUShortIndex]) != 0;
        }
    }
    
    /// <summary>
    /// 根据uint保存的bit信息进行初始化，uint[0]表示位0-31，字节[1]表示位32-63，以此类推，每个uint的低位bit代表低位
    /// </summary>
    /// <param name="uints">用于初始化Bit信息的uint数组</param>
    /// <param name="bitLength">指定初始化的Bit长度，可以使用uints.Length * 32进行指定</param>
    public Bits(uint[] uints,int bitLength)
    {
        if(uints is null) throw new ArgumentNullException(nameof(uints));
        bits = new bool[bitLength];
        int initBitLength = uints.Length * 32;
        for (int index = 0; index < bitLength && index < initBitLength; index++)
        {
            int initUintIndex = index / 32;
            int initBitIndex = index % 32;
            bits[index] = ((1u << initBitIndex) & uints[initUintIndex]) != 0;
        }
    }

    public Bits(Bits otherBits)
    {
        if(otherBits is null) throw new ArgumentNullException(nameof(otherBits));
        bits = new bool[otherBits.Length];
        Array.Copy(otherBits.bits, bits, bits.Length);
    }
    
    public bool this[int index]
    {
        get => Get(index);
        set => Set(index, value);
    }

    public T Get<T>(BitRange range) where T : struct
    {
        T resultValue = default;
        if (!typeof(T).IsValueType)
        {
            throw new ArgumentException("T must be a value type");
        }
        int startIndex = range.StartBit, endIndex = range.EndBit;
        if (resultValue is byte valueByte)
        {
            for (int index = startIndex,resultIndex = 0; index <= endIndex && resultIndex < sizeof(byte) * 8; index++,resultIndex++)
            {
                valueByte.SetBits(resultIndex, Get(index));
            }
            resultValue = (T)Convert.ChangeType(valueByte, typeof(T));
        }else if (resultValue is ushort valueShort)
        {
            for (int index = startIndex,resultIndex = 0; index <= endIndex && resultIndex < sizeof(ushort) * 8; index++,resultIndex++)
            {
                valueShort.SetBits(resultIndex, Get(index));
            }
            resultValue = (T)Convert.ChangeType(valueShort, typeof(T));
        }else if (resultValue is uint valueInt)
        {
            for (int index = startIndex,resultIndex = 0; index <= endIndex && resultIndex < sizeof(uint) * 8; index++,resultIndex++)
            {
                valueInt.SetBits(resultIndex, Get(index));
            }
            resultValue = (T)Convert.ChangeType(valueInt, typeof(T));
        }else if (resultValue is ulong valueLong)
        {
            for (int index = startIndex,resultIndex = 0; index <= endIndex && resultIndex < sizeof(ulong) * 8; index++,resultIndex++)
            {
                valueLong.SetBits(resultIndex, Get(index));
            }
            resultValue = (T)Convert.ChangeType(valueLong, typeof(T));
        }
        
        return resultValue;
    }
    
    
    public bool Get(int index)
    {
        if (index >= Length || index < 0)
            ThrowArgumentOutOfRangeException(index);

        return bits[index];
    }
    
    public void Set(int index, bool value)
    {
        if (index >= Length || index < 0)
            ThrowArgumentOutOfRangeException(index);
        
        bits[index] = value;
    }

    public void Set(byte value)
    {
        int bitIndexInLength = sizeof(byte) * 8;
        for (int bitIndex = 0; bitIndex < Length && bitIndex < bitIndexInLength; bitIndex++)
        {
            bits[bitIndex] = value.Bit(bitIndex);
        }
    }

    /// <summary>
    /// 全部比特位设置为指定值
    /// </summary>
    /// <param name="value">所有bit设置为true或者false</param>
    public void SetAll(bool value)
    {
        for (int index = 0; index < Length; index++)
        {
            bits[index] = value;
        }
    }
    
    /// <summary>
    /// 所有Bit按照相同位置进行与运算 ,并返回自身的引用
    /// </summary>
    /// <param name="value">另一个计算对象</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">两者Bit长度不同或value为null ，则报异常</exception>
    public Bits And(Bits value)
    {
        if(value is null) throw new ArgumentNullException(nameof(value));
        if(value.Length != Length)
            throw new ArgumentException("Array lengths must be the same.");
        for (int index = 0; index < Length; index++)
        {
            bits[index] &= value[index];
        }

        return this;
    }
    
    /// <summary>
    /// 所有Bit按照相同位置进行或运算 ,并返回自身的引用，如两者Bit长度不同则报异常
    /// </summary>
    /// <param name="value">另一个计算对象</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">两者Bit长度不同或value为null ，则报异常</exception>
    public Bits Or(Bits value)
    {
        if(value is null) throw new ArgumentNullException(nameof(value));
        if(value.Length != Length)
            throw new ArgumentException("Array lengths must be the same.");
        for (int index = 0; index < Length; index++)
        {
            bits[index] |= value[index];
        }

        return this;
    }
    
    /// <summary>
    /// 所有Bit按照相同位置进行异或运算 ,并返回自身的引用，如两者Bit长度不同则报异常
    /// </summary>
    /// <param name="value">另一个计算对象</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">两者Bit长度不同或value为null ，则报异常</exception>
    public Bits Xor(Bits value)
    {
        if(value is null) throw new ArgumentNullException(nameof(value));
        if(value.Length != Length)
            throw new ArgumentException("Array lengths must be the same.");
        for (int index = 0; index < Length; index++)
        {
            bits[index] ^= value[index];
        }

        return this;
    }
    
    /// <summary>
    /// 对所有Bit位进行置反
    /// </summary>
    /// <returns>自身的引用</returns>
    public Bits Not()
    {
        for (int i = 0; i < Length; i++)
        {
            bits[i] = !bits[i];
        }
        return this;
    }

    /// <summary>
    /// 检查当前所有的Bit是否全部置true，即全部Bit为1
    /// </summary>
    /// <returns>全部比特位为ture,则返回true</returns>
    public bool HasAllSet()
    {
        foreach (var bit in bits)
        {
            if (bit == false) return false;
        }
        return true;
    }

    /// <summary>
    /// 检查当前所有的Bit是否存在Bit等于1，有则返回true
    /// </summary>
    /// <returns>true：存在bit为1</returns>
    public bool HasAnySet()
    {
        foreach (var bit in bits)
        {
            if (bit) return true;
        }
        return false;
    }

    public override string ToString()
    {
        if(Length == 0) return string.Empty;
        StringBuilder sb = new StringBuilder(Length);
        for (int index = 0; index < Length; index++)
        {
            sb.Append(bits[index] ? '1' : '0');
            if((index) % 4 == 3)sb.Append(' ');
        }
        if(sb[Length - 1] == ' ') sb.Remove(sb.Length - 1, 1);
        return sb.ToString();
    }
    

    private static void ThrowArgumentOutOfRangeException(int index)
    {
        throw new ArgumentOutOfRangeException(nameof(index), index, @"Index was out of range. Must be non-negative and less than the size of the collection.");
    }

    public byte ToByte()
    {
        byte result = 0;
        int bitIndexLength = sizeof(byte) * 8;
        for (int index = 0; index < bitIndexLength && index < Length; index++)
        {
            result.SetBits(index, bits[index]);
        }
        return result;
    }

    public ushort ToUShort()
    {
        ushort result = 0;
        int bitIndexLength = sizeof(ushort) * 8;
        for (int index = 0; index < bitIndexLength && index < Length; index++)
        {
            result.SetBits(index, bits[index]);
        }
        return result;
    }
    
    public uint ToUInt()
    {
        uint result = 0;
        int bitIndexLength = sizeof(uint) * 8;
        for (int index = 0; index < bitIndexLength && index < Length; index++)
        {
            result.SetBits(index, bits[index]);
        }
        return result;
    }
    
    public ulong ToULong()
    {
        ulong result = 0;
        int bitIndexLength = sizeof(ulong) * 8;
        for (int index = 0; index < bitIndexLength && index < Length; index++)
        {
            result.SetBits(index, bits[index]);
        }
        return result;
    }
    
}