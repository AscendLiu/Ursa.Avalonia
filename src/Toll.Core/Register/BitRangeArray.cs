using System.Collections;
using Toll.Core.Utils;

namespace Toll.Core.Register;


public class BitRangeArray
{
    private BitArray bits;

    public int Length
    {
        get => bits.Length;
        set => bits.Length = value;
    }
    public int Count => bits.Length;
    
    public object SyncRoot => this;
 
    public bool IsSynchronized => bits.IsSynchronized;
 
    public bool IsReadOnly => bits.IsReadOnly;

    public BitRangeArray(int length,bool defaultValue = false)
    {
        bits = new BitArray(length,defaultValue);
    }

    public BitRangeArray(byte[] bytes)
    {
        bits = new BitArray(bytes);
    }

    public BitRangeArray(bool[] values)
    {
        bits = new BitArray(values);
    }
    
    public BitRangeArray(int[] values)
    {
        bits = new BitArray(values);
    }    
    
    public BitRangeArray(BitRangeArray bitRangeArray)
    {
        bits = new BitArray(bitRangeArray.bits);
    }
    
    public bool this[int index]
    {
        get => Get(index);
        set => Set(index, value);
    }
    
    public bool Get(int index) => bits.Get(index);

    /// <summary>
    /// 返回指定范围Bit组成的值，注意：支持返回的最大值是ulong.MaxValue
    /// </summary>
    /// <param name="startIndex"></param>
    /// <param name="endIndex"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public ulong Get(int startIndex, int endIndex)
    {
        if (startIndex < 0 || endIndex < startIndex || endIndex >= Length || (endIndex - startIndex + 1) > sizeof(ulong) * 8)
        {
            throw new ArgumentOutOfRangeException("endIndex must be greater than startIndex And startIndex >= 0 And endIndex < Length");
        }
        
        ulong resultValue = 0;
        for (int index = startIndex,resultIndex = 0; index <= endIndex && resultIndex < sizeof(ulong) * 8; index++,resultIndex++)
        {
            resultValue.SetBits(resultIndex, Get(index));
        }
        return resultValue;
    }

    /// <summary>
    /// 返回指定值类型和指定范围Bit组成的值，注意：范围内的bit总数不能超过T类型的最大bit数
    /// </summary>
    /// <param name="startIndex"></param>
    /// <param name="endIndex"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public T Get<T>(int startIndex, int endIndex) where T : struct
    {
        if (startIndex < 0 || endIndex < startIndex || endIndex >= Length)
        {
            throw new ArgumentOutOfRangeException("endIndex must be greater than startIndex And startIndex >= 0 And endIndex < Length");
        }
        T resultValue = default;
        if (!typeof(T).IsValueType)
        {
            throw new ArgumentException("T must be a value type");
        }
        
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
    
    public void Set(int index, bool value) => bits.Set(index, value);

    public void SetAll(bool value) => bits.SetAll(value);

    /// <summary>
    /// 返回一直指向当前实例和bitValue进行按位与计算结果的引用
    /// Exceptions: ArgumentException if value == null or value.Length != this.Length.
    /// </summary>
    /// <param name="bitValue"></param>
    /// <returns></returns>
    public BitRangeArray And(BitRangeArray bitValue)
    {
        bits.And(bitValue.bits);
        return this;
    }

    /// <summary>
    /// 返回一直指向当前实例和bitValue进行按位或计算结果的引用
    /// Exceptions: ArgumentException if value == null or value.Length != this.Length.
    /// </summary>
    /// <param name="bitValue"></param>
    /// <returns></returns>
    public BitRangeArray Or(BitRangeArray bitValue)
    {
        bits.Or(bitValue.bits);
        return this;
    }

    /// <summary>
    /// 返回一直指向当前实例和bitValue进行按位异或计算结果的引用
    /// Exceptions: ArgumentException if value == null or value.Length != this.Length.
    /// </summary>
    /// <param name="bitValue"></param>
    /// <returns></returns>
    public BitRangeArray Xor(BitRangeArray bitValue)
    {
        bits.Xor(bitValue.bits);
        return this;
    }
    
    /// <summary>
    /// 所有bit反转，（0 => 1;1 => 0）,自身更新并返回
    /// </summary>
    /// <returns></returns>
    public BitRangeArray Not()
    {
        bits.Not();
        return this;
    }

    public object Clone() => new BitRangeArray(this);

    public void CopyTo(Array array, int index) => bits.CopyTo(array, index);

}