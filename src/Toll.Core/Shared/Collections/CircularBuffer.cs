using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Toll.Core.Shared.Collections.Generic;
using Toll.Core.Utils;

namespace Toll.Core.Shared.Collections;


/// <summary>
/// 循环缓冲队列，但是不具备主动弹出的功能，只能尾部添加新的元素，当达到容器的最大容纳元素时，再添加则自动去掉头部元素
/// <para>
/// 特性：空间自动增长(最大为：<see cref="MaxCapacity"/>);尾部添加；自动弹出头部元素(当 <see cref="Capacity"/> 等于 <see cref="MaxCapacity"/>)
/// </para>
/// </summary>
[DebuggerTypeProxy(typeof(CircularBuffDebugView<>))]
[DebuggerDisplay("Count = {Count}")]
[Serializable]
[TypeForwardedFrom("System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
public class CircularBuffer<T> : ICollection, IReadOnlyCollection<T>,IBuff<T>
{
    private T[] _arrayBuff;
    
    private int _nextIndex; //下一个待插入元素的位置
    private int _size; // 数组中实际的元素个数
    private int _buffVersion; // 标记当前数组发生改变时的版本
    public static int DefaultMaxCapacity => 100;
    public static int DefaultInitCapacity => 10;
    private int _maxCapacity = DefaultMaxCapacity;

    public CircularBuffer()
    {
        _arrayBuff = new T[DefaultInitCapacity];
    }
    
    /// <summary>
    /// 创建一个指定容量大小的环形缓冲区
    /// </summary>
    /// <param name="capacity"></param>
    public CircularBuffer(int capacity)
    {
        ThrowHelper.ThrowIfNegative(capacity);
        _arrayBuff = new T[capacity];
        if(capacity > _maxCapacity)_maxCapacity = capacity;
    }
    
    // Fills a Queue with the elements of an ICollection.  Uses the enumerator
    // to get each of the elements.
    public CircularBuffer(IEnumerable<T> collection)
    {
        ArgumentNullException.ThrowIfNull(collection);
 
        _arrayBuff = EnumerableHelpers.ToArray(collection, out _size);
        if (_size != _arrayBuff.Length) _nextIndex = _size;
        if(Capacity > DefaultMaxCapacity)_maxCapacity = Capacity;
    }
    
    
    /// <summary>
    /// 只代表容器最大能到达的容量，不代表实际容量
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">值小于o，报异常</exception>
    public int MaxCapacity
    {
        get => _maxCapacity;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            _maxCapacity = value;
            if(Capacity > MaxCapacity) Capacity = MaxCapacity;
        }
    }
    
    public int Capacity
    {
        get => _arrayBuff.Length;
        private set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            if(MaxCapacity < value) MaxCapacity = value;
            SetCapacity(value);
        }
    }
    
    public int Count => _size;
    
    public bool IsReadOnly => false;
    
    bool ICollection.IsSynchronized => false;

    object ICollection.SyncRoot => this;
    
    /// <summary>
    /// 如果插入时，队列已满，则移除的最初插入的项
    /// </summary>
    public T? RemoveItemBeforeLastAdd { get; private set; }
    
    /// <summary>
    /// 判断当前容器是否已充满，继续插入则自动释放最先插入的元素，且容器的容量不会发生改变
    /// </summary>
    public bool IsFull => _size == _arrayBuff.Length;
    
    /// <summary>
    ///  根据理论上元素的下标位置，找到实际元素在数组中的存储下标,索引都从0开始
    /// </summary>
    /// <param name="index">元素展示的索引下标位置，跟其它容器一样，如List</param>
    /// <returns></returns>
    private int ConvertToActualArrayIndex(int index) => IsFull ? (_nextIndex + index) % Capacity : index;

    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= _size) throw new IndexOutOfRangeException("Index was out of range.");
            return _arrayBuff[ConvertToActualArrayIndex(index)];
        }
        set
        {
            if (index < 0 || index >= _size) throw new IndexOutOfRangeException("Index was out of range.");
            _arrayBuff[ConvertToActualArrayIndex(index)] = value;
        }
    }
    
    public void Clear()
    {
        if (_size != 0)
        {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                if(IsFull) Array.Clear(_arrayBuff);
                else Array.Clear(_arrayBuff, 0, _size);
            }

            _size = 0;
        }

        _nextIndex = 0;
        _buffVersion++;
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

        if (array.Length - arrayIndex < _size)
        {
            ThrowHelper.ThrowArgumentException("The target array is too small.");
        }

        int numToCopy = _size;
        if (numToCopy == 0) return;
        if (IsFull)
        {
            Array.Copy(_arrayBuff, _nextIndex, array, arrayIndex, _arrayBuff.Length - _nextIndex);
            Array.Copy(_arrayBuff, 0, array, arrayIndex + _arrayBuff.Length - _nextIndex, _nextIndex);
        }
        else Array.Copy(_arrayBuff, 0, array, arrayIndex, numToCopy);
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

        if (arrayLen - index < _size)
        {
            ThrowHelper.ThrowArgumentException("Invalid length");
        }

        int numToCopy = _size;
        if (numToCopy == 0) return;

        try
        {
            if (IsFull)
            {
                Array.Copy(_arrayBuff, _nextIndex, array, index, _arrayBuff.Length - _nextIndex);
                Array.Copy(_arrayBuff, 0, array, index + _arrayBuff.Length - _nextIndex, _nextIndex);
            }
            else Array.Copy(_arrayBuff, 0, array, index, numToCopy);
        }
        catch (ArrayTypeMismatchException)
        {
            ThrowHelper.ThrowArgumentException("Argument_IncompatibleArrayType");
        }
    }



    /// <summary>
    /// 向尾部插入新元素,返回插入的位置，-1则代表失败
    /// </summary>
    ///  <remarks>
    /// 返回位置的解释：该位置的编号代表该插入项实际在数组中的代表索引，而不是在数组中的数组索引，因为插入项是在数组中循环覆盖的。
    /// 即：当插入时容量满了以后，在插入时，一定是插入在最后面，即返回最大元素的索引，而不是插入项在数组中的存储位置
    /// </remarks>
    /// <param name="item"></param>
    public int Add(T item)
    {
        if (IsFull) Grow();
        if (Capacity == 0) return -1;
        
        int addIndex;
        if (IsFull)
        {
            addIndex = Count - 1;
            RemoveItemBeforeLastAdd = _arrayBuff[_nextIndex];
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                _arrayBuff[_nextIndex] = default!;
            }
        }
        else
        {
            addIndex = _nextIndex;
            RemoveItemBeforeLastAdd = default;
            _size++;
        }
        _arrayBuff[_nextIndex] = item;
        _nextIndex = (_nextIndex + 1) % Capacity; // 循环更新索引
        return addIndex;
    }

    // GetEnumerator returns an IEnumerator over this Queue.  This
    // Enumerator will support removing.
    public Enumerator GetEnumerator() => new Enumerator(this);
    
    IEnumerator<T> IEnumerable<T>.GetEnumerator() =>
        Count == 0 ? SZGenericArrayEnumerator<T>.Empty : GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)this).GetEnumerator();


    // Returns true if the queue contains at least one object equal to item.
    // Equality is determined using EqualityComparer<T>.Default.Equals().
    public bool Contains(T item)
    {
        if (_size == 0) return false;
        return Array.IndexOf(_arrayBuff, item, 0, _size) >= 0;
    }


    /// <summary>
    /// 返回一个按照时间先后顺序插入的元素数组，如果元素为空则返回一个空数组
    /// </summary>
    /// <returns></returns>
    public T[] ToArray()
    {
        if (_size == 0) return Array.Empty<T>();

        var arr = new T[_size];
        if (!IsFull)
        {
            Array.Copy(_arrayBuff, 0, arr, 0, _size);
        }
        else
        {
            Array.Copy(_arrayBuff, _nextIndex, arr, 0, _arrayBuff.Length - _nextIndex);
            Array.Copy(_arrayBuff, 0, arr, _arrayBuff.Length - _nextIndex, _nextIndex);
        }

        return arr;
    }
    
    /// <summary>
    /// 容器实现自动扩大，增长因子：2倍，一次最小增长4，能增长至最大容量<see cref="MaxCapacity"/>
    /// </summary>
    private void Grow()
    {
        if (Capacity >= MaxCapacity) return;
        const int growFactor = 2;
        const int minimumGrow = 4;
 
        var newCapacity = growFactor * _arrayBuff.Length;
 
        // Allow the list to grow to maximum possible capacity (~2G elements) before encountering overflow.
        // Note that this check works even when _items.Length overflowed thanks to the (uint) cast
        if ((uint)newCapacity > Array.MaxLength) newCapacity = Array.MaxLength;
 
        // Ensure minimum growth is respected.
        newCapacity = Math.Max(newCapacity, _arrayBuff.Length + minimumGrow);
        
        if(newCapacity > MaxCapacity) newCapacity = MaxCapacity;
 
        SetCapacity(newCapacity);
    }

    /// <summary>
    /// 设置新的容量值，当新的缓冲区容量小于原缓冲区的Size时，自动去掉最早的部分元素，当新的容量更大时，元素不发生改变
    /// </summary>
    /// <param name="capacity"></param>
    public void SetCapacity(int capacity)
    {
        T[] newArray = new T[capacity];
        if (_size > 0)
        {
            if (!IsFull)
            {
                int copyToNum = capacity > _size ? _size : capacity;
                Array.Copy(_arrayBuff, 0, newArray, 0, copyToNum);
                _nextIndex = copyToNum;
                _size = copyToNum;
            }
            else
            {
                if (capacity > _size)
                {
                    Array.Copy(_arrayBuff, _nextIndex, newArray, 0, _arrayBuff.Length - _nextIndex);
                    Array.Copy(_arrayBuff, 0, newArray, _arrayBuff.Length - _nextIndex, _nextIndex);
                    _nextIndex = _size;
                }
                else
                {
                    var firstCopyNum = _arrayBuff.Length - _nextIndex;
                    if (firstCopyNum > capacity)
                    {
                        Array.Copy(_arrayBuff, _nextIndex, newArray, 0, capacity);
                    }
                    else
                    {
                        Array.Copy(_arrayBuff, _nextIndex, newArray, 0, firstCopyNum);
                        Array.Copy(_arrayBuff, 0, newArray, firstCopyNum, capacity - firstCopyNum);
                    }

                    _size = capacity;
                    _nextIndex = 0;
                }
            }
        }

        _arrayBuff = newArray;
        _buffVersion++;
    }

    private void ThrowForEmptyQueue()
    {
        Debug.Assert(_size == 0);
        ThrowHelper.ThrowInvalidOperationException("QueueFixLength is empty");
    }


    // Implements an enumerator for a Queue.  The enumerator uses the
    // internal version number of the list to ensure that no modifications are
    // made to the list while an enumeration is in progress.
    public struct Enumerator : IEnumerator<T>
    {
        private readonly CircularBuffer<T> _buff;
        private readonly int _version;
        private int _index; // -1 = not started, -2 = ended/disposed
        private T? _currentElement;

        internal Enumerator(CircularBuffer<T> cBuff)
        {
            _buff = cBuff;
            _version = cBuff._buffVersion;
            _index = -1;
            _currentElement = default;
        }

        public void Dispose()
        {
            _index = -2;
            _currentElement = default;
        }

        public bool MoveNext()
        {
            if (_version != _buff._buffVersion)
                ThrowHelper.ThrowInvalidOperationException("InvalidOperation_EnumFailedVersion");

            if (_index == -2) return false;
            _index++;

            if (_index == _buff._size)
            {
                _index = -2;// We've run past the last element
                _currentElement = default;
                return false;
            }
            _currentElement = _buff._arrayBuff[_buff.ConvertToActualArrayIndex(_index)];
            return true;
        }

        public T Current
        {
            get
            {
                if (_index < 0)
                    ThrowEnumerationNotStartedOrEnded();
                return _currentElement!;
            }
        }

        private void ThrowEnumerationNotStartedOrEnded()
        {
            Debug.Assert(_index == -1 || _index == -2);
            throw new InvalidOperationException(_index == -1
                ? "Invalid operation: enumeration has not started yet"
                : "Invalid operation: enumeration has ended");
        }

        object? IEnumerator.Current => Current;

        void IEnumerator.Reset()
        {
            if (_version != _buff._buffVersion)
                ThrowHelper.ThrowInvalidOperationException("InvalidOperation_EnumFailedVersion");
            _index = -1;
            _currentElement = default;
        }
    }
    
}

internal sealed class CircularBuffDebugView<T>(CircularBuffer<T> queue)
{
    private readonly CircularBuffer<T> _queue = queue ?? throw new ArgumentNullException(nameof(queue));

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public T[] Items => _queue.ToArray();
}