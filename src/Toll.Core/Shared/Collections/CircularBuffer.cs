using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Toll.Core.Shared.Collections.Generic;
using Toll.Core.Utils;

namespace Toll.Core.Shared.Collections;


/// <summary>
/// 具有固定大小的队列，一旦队列满后，插入新的元素会导致头部自动移除
/// </summary>
[DebuggerTypeProxy(typeof(QueueDebugView<>))]
[DebuggerDisplay("Count = {Count}")]
[Serializable]
[TypeForwardedFrom("System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
public class CircularBuffer<T> : ICollection, IReadOnlyCollection<T>,IBuff<T>
{
    private T[] arrayBuff;
    
    /// <summary>
    /// 下一个待插入元素的位置
    /// </summary>
    private int nextIndex;
    private int size; // 数组中实际的元素个数
    private int buffVersion;

    /// <summary>
    /// 创建一个指定容量大小的环形缓冲区
    /// </summary>
    /// <param name="capacity"></param>
    public CircularBuffer(int capacity = 0)
    {
        ThrowHelper.ThrowIfNegative(capacity);
        arrayBuff = new T[capacity];
    }
    
    // Fills a Queue with the elements of an ICollection.  Uses the enumerator
    // to get each of the elements.
    public CircularBuffer(IEnumerable<T> collection)
    {
        ArgumentNullException.ThrowIfNull(collection);
 
        arrayBuff = EnumerableHelpers.ToArray(collection, out size);
        if (size != arrayBuff.Length) nextIndex = size;
    }
    
    /// <summary>
    /// 下一个待插入的位置
    /// </summary>
    public int NextIndex => nextIndex;

    /// <summary>
    /// 已经容纳的元素个数
    /// </summary>
    public int Count => size;

    /// <summary>
    /// 可以容纳的元素总数
    /// </summary>
    public int Capacity
    {
        get => arrayBuff.Length;
        set => SetCapacity(value);
    }

    /// <inheritdoc cref="ICollection{T}"/>
    bool ICollection.IsSynchronized => false;

    object ICollection.SyncRoot => this;

    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= size) throw new IndexOutOfRangeException("Index was out of range.");
            if (IsFull)
            {
                int actualIndex = nextIndex + index;
                if (actualIndex >= arrayBuff.Length) actualIndex -=  arrayBuff.Length;
                return arrayBuff[actualIndex];
            }
            return arrayBuff[nextIndex];
        }
    }

    /// <summary>
    /// 清空所有元素
    /// </summary>
    public void Clear()
    {
        if (size != 0)
        {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                Array.Clear(arrayBuff);
            }

            size = 0;
        }

        nextIndex = 0;
        buffVersion++;
    }


    /// <summary>
    /// 把缓冲区的所有数据拷贝到指定数组的指定起始位置，必须保证数组有足够的空间去存储缓冲区的数据
    /// </summary>
    /// <param name="array"></param>
    /// <param name="arrayIndex"></param>
    public void CopyTo(T[] array, int arrayIndex)
    {
        ArgumentNullException.ThrowIfNull(array);

        if (arrayIndex < 0 || arrayIndex >= array.Length)
        {
            ThrowHelper.ThrowIfArgumentOutOfRange(arrayIndex, "Index must be greater than 0 and less than Length");
        }

        if (array.Length - arrayIndex < size)
        {
            ThrowHelper.ThrowArgumentException("Invalid length");
        }

        int numToCopy = size;
        if (numToCopy == 0) return;
        if (IsFull)
        {
            Array.Copy(arrayBuff, nextIndex, array, arrayIndex, arrayBuff.Length - nextIndex);
            Array.Copy(arrayBuff, 0, array, arrayIndex + arrayBuff.Length - nextIndex, nextIndex);
        }
        else Array.Copy(arrayBuff, 0, array, arrayIndex, numToCopy);
    }

    /// <summary>
    /// 把缓冲区的所有数据拷贝到指定数组的指定起始位置，必须保证数组有足够的空间去存储缓冲区的数据
    /// </summary>
    /// <param name="array"></param>
    /// <param name="index"></param>
    void ICollection.CopyTo(Array array, int index)
    {
        ThrowHelper.ThrowIfNull(array);

        if (array.Rank != 1)
        {
            ThrowHelper.ThrowArgumentException("the rank of array must equal to one.", nameof(array));
        }

        if (array.GetLowerBound(0) != 0)
        {
            ThrowHelper.ThrowArgumentException("the rank of array elements must be 0");
        }

        int arrayLen = array.Length;
        if (index < 0 || index > arrayLen)
        {
            ThrowHelper.ThrowArgumentException("the index of the array Must Be Less Or Equal the length of array",
                nameof(index));
        }

        if (arrayLen - index < size)
        {
            ThrowHelper.ThrowArgumentException("Invalid length");
        }

        int numToCopy = size;
        if (numToCopy == 0) return;

        try
        {
            if (IsFull)
            {
                Array.Copy(arrayBuff, nextIndex, array, index, arrayBuff.Length - nextIndex);
                Array.Copy(arrayBuff, 0, array, index + arrayBuff.Length - nextIndex, nextIndex);
            }
            else Array.Copy(arrayBuff, 0, array, index, numToCopy);
        }
        catch (ArrayTypeMismatchException)
        {
            ThrowHelper.ThrowArgumentException("Argument_IncompatibleArrayType");
        }
    }

    public bool IsReadOnly => false;

    /// <summary>
    /// 向尾部插入新元素
    /// </summary>
    /// <param name="item"></param>
    public T? Append(T item)
    {
        if (Capacity == 0) return default;
        if (IsFull)
        {
            T removed = arrayBuff[nextIndex];
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                arrayBuff[nextIndex] = default!;
            }
            arrayBuff[nextIndex] = item;
            nextIndex = (nextIndex + 1) % Capacity; // 循环更新索引
            return removed;
        }
        else
        {
            arrayBuff[nextIndex] = item;
            nextIndex = (nextIndex + 1) % Capacity; // 循环更新索引
            size++;
        }
        return default;
    }

    /// <summary>
    /// 判断当前容器是否已充满，继续插入则自动释放最先插入的元素，且容器的容量不会发生改变
    /// </summary>
    public bool IsFull => size == arrayBuff.Length;

    // GetEnumerator returns an IEnumerator over this Queue.  This
    // Enumerator will support removing.
    public Enumerator GetEnumerator() => new Enumerator(this);

    /// <internalonly/>
    IEnumerator<T> IEnumerable<T>.GetEnumerator() =>
        Count == 0 ? SZGenericArrayEnumerator<T>.Empty : GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)this).GetEnumerator();


    // Returns true if the queue contains at least one object equal to item.
    // Equality is determined using EqualityComparer<T>.Default.Equals().
    public bool Contains(T item)
    {
        if (size == 0) return false;

        if (!IsFull)
        {
            return Array.IndexOf(arrayBuff, item, 0, size) >= 0;
        }

        return Array.IndexOf(arrayBuff, item, 0, size) >= 0;
    }


    /// <summary>
    /// 返回一个按照时间先后顺序插入的元素数组，如果元素为空则返回一个空数组
    /// </summary>
    /// <returns></returns>
    public T[] ToArray()
    {
        if (size == 0)
        {
            return Array.Empty<T>();
        }

        T[] arr = new T[size];

        if (!IsFull)
        {
            Array.Copy(arrayBuff, 0, arr, 0, size);
        }
        else
        {
            Array.Copy(arrayBuff, nextIndex, arr, 0, arrayBuff.Length - nextIndex);
            Array.Copy(arrayBuff, 0, arr, arrayBuff.Length - nextIndex, nextIndex);
        }

        return arr;
    }

    /// <summary>
    /// 设置新的容量值，当新的缓冲区容量小于原缓冲区的Size时，自动去掉最早的部分元素，当新的容量更大时，元素不发生改变
    /// </summary>
    /// <param name="capacity"></param>
    public void SetCapacity(int capacity)
    {
        T[] newArray = new T[capacity];
        if (size > 0)
        {
            if (!IsFull)
            {
                int copyToNum = capacity > size ? size : capacity;
                Array.Copy(arrayBuff, 0, newArray, 0, copyToNum);
                nextIndex = copyToNum;
                size = copyToNum;
            }
            else
            {
                if (capacity > size)
                {
                    Array.Copy(arrayBuff, nextIndex, newArray, 0, arrayBuff.Length - nextIndex);
                    Array.Copy(arrayBuff, 0, newArray, arrayBuff.Length - nextIndex, nextIndex);
                    nextIndex = size;
                }
                else
                {
                    var firstCopyNum = arrayBuff.Length - nextIndex;
                    if (firstCopyNum > capacity)
                    {
                        Array.Copy(arrayBuff, nextIndex, newArray, 0, capacity);
                    }
                    else
                    {
                        Array.Copy(arrayBuff, nextIndex, newArray, 0, firstCopyNum);
                        Array.Copy(arrayBuff, 0, newArray, firstCopyNum, capacity - firstCopyNum);
                    }

                    size = capacity;
                    nextIndex = 0;
                }
            }
        }

        arrayBuff = newArray;
        buffVersion++;
    }

    private void ThrowForEmptyQueue()
    {
        Debug.Assert(size == 0);
        ThrowHelper.ThrowInvalidOperationException("QueueFixLength is empty");
    }


    // Implements an enumerator for a Queue.  The enumerator uses the
    // internal version number of the list to ensure that no modifications are
    // made to the list while an enumeration is in progress.
    public struct Enumerator : IEnumerator<T>
    {
        private readonly CircularBuffer<T> buff;
        private readonly int version;
        private int index; // -1 = not started, -2 = ended/disposed
        private T? currentElement;

        internal Enumerator(CircularBuffer<T> cBuff)
        {
            buff = cBuff;
            version = cBuff.buffVersion;
            index = -1;
            currentElement = default;
        }

        public void Dispose()
        {
            index = -2;
            currentElement = default;
        }

        public bool MoveNext()
        {
            if (version != buff.buffVersion)
                ThrowHelper.ThrowInvalidOperationException("InvalidOperation_EnumFailedVersion");

            if (index == -2) return false;
            index++;

            if (index == buff.size)
            {
                // We've run past the last element
                index = -2;
                currentElement = default;
                return false;
            }

            // Cache some fields in locals to decrease code size
            T[] array = buff.arrayBuff;
            uint capacity = (uint)array.Length;

            uint arrayIndex; // this is the actual index into the queue's backing array
            if (buff.IsFull)
            {
                arrayIndex = (uint)(buff.nextIndex + index);
                if (arrayIndex >= capacity) arrayIndex -= capacity;// 从头开始循环
            }
            else arrayIndex = (uint)index;

            currentElement = array[arrayIndex];
            return true;
        }

        public T Current
        {
            get
            {
                if (index < 0)
                    ThrowEnumerationNotStartedOrEnded();
                return currentElement!;
            }
        }

        private void ThrowEnumerationNotStartedOrEnded()
        {
            Debug.Assert(index == -1 || index == -2);
            throw new InvalidOperationException(index == -1
                ? "Invalid operation: enumeration has not started yet"
                : "Invalid operation: enumeration has ended");
        }

        object? IEnumerator.Current => Current;

        void IEnumerator.Reset()
        {
            if (version != buff.buffVersion)
                ThrowHelper.ThrowInvalidOperationException("InvalidOperation_EnumFailedVersion");
            index = -1;
            currentElement = default;
        }
    }
}

internal sealed class QueueDebugView<T>(CircularBuffer<T> queue)
{
    private readonly CircularBuffer<T> queue = queue ?? throw new ArgumentNullException(nameof(queue));

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public T[] Items => queue.ToArray();
}