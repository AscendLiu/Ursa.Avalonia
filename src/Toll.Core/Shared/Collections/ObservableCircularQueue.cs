using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Toll.Core.Shared.Collections;

/// <summary>
/// 一个支持通知的环形缓队列，当插入达到最大容量时，自动弹出最初的插入项
/// </summary>
/// <typeparam name="T">The type of the list items.</typeparam>
/// <remarks>
/// <para>
/// AvaloniaList is similar to <see cref="System.Collections.ObjectModel.ObservableCollection{T}"/>
/// with a few added features:
/// </para>
/// </remarks>
public sealed class ObservableCircularQueue<T> : IBuff<T>,
    IReadOnlyList<T>,
    INotifyCollectionChanged, 
    INotifyPropertyChanged, 
    IList
{
    private readonly CircularBuffer<T> _inner;
    private NotifyCollectionChangedEventHandler? _collectionChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableCircularQueue{T}"/> class.
    /// </summary>
    public ObservableCircularQueue()
    {
        _inner = new CircularBuffer<T>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableCircularQueue{T}"/>.
    /// </summary>
    /// <param name="capacity">Initial list capacity.</param>
    public ObservableCircularQueue(int capacity)
    {
        _inner = new CircularBuffer<T>(capacity);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableCircularQueue{T}"/> class.
    /// </summary>
    /// <param name="items">The initial items for the collection.</param>
    public ObservableCircularQueue(IEnumerable<T> items)
    {
        _inner = new CircularBuffer<T>(items);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableCircularQueue{T}"/> class.
    /// </summary>
    /// <param name="items">The initial items for the collection.</param>
    public ObservableCircularQueue(params T[] items)
    {
        _inner = new CircularBuffer<T>(items);
    }

    /// <summary>
    /// Raised when a change is made to the collection's items.
    /// </summary>
    public event NotifyCollectionChangedEventHandler? CollectionChanged
    {
        add => _collectionChanged += value;
        remove => _collectionChanged -= value;
    }

    /// <summary>
    /// Raised when a property on the collection changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    public int MaxCapacity { get => _inner.MaxCapacity; set => _inner.MaxCapacity = value; }

    /// <summary>
    /// Gets the number of items in the collection.
    /// </summary>
    public int Count => _inner.Count;

    public bool IsReadOnly => false;

    /// <inheritdoc/>
    bool IList.IsFixedSize => false;

    /// <inheritdoc/>
    bool IList.IsReadOnly => false;

    /// <inheritdoc/>
    int ICollection.Count => _inner.Count;

    /// <inheritdoc/>
    bool ICollection.IsSynchronized => false;

    /// <inheritdoc/>
    object ICollection.SyncRoot => this;


    /// <summary>
    /// Gets the item at the specified index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The item.</returns>
    public T this[int index]
    {
        get => _inner[index];
        set => _inner[index] = value;
    }

    /// <summary>
    /// Gets or sets the item at the specified index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The item.</returns>
    object? IList.this[int index]
    {
        get => _inner[index];
        set => _inner[index] = (T)value!;
    }

    /// <summary>
    /// Gets or sets the total number of elements the internal data structure can hold without resizing.
    /// </summary>
    public int Capacity => _inner.Capacity;

    /// <summary>
    /// Adds an item to the collection.
    /// </summary>
    /// <param name="item">The item.</param>
    public int Add(T item)
    {
        var addIndex = _inner.Add(item);
        if (_inner.RemoveItemBeforeLastAdd != null)
        {
            NotifyRemove(_inner.RemoveItemBeforeLastAdd, 0);
        }
        NotifyAdd(item, addIndex);
        return addIndex;
    }

    /// <summary>
    /// Removes all items from the collection.
    /// </summary>
    public void Clear()
    {
        if (Count > 0)
        {
            if (_collectionChanged != null)
            {
                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, _inner.ToArray(), 0);
                _collectionChanged(this, e);
            }
            _inner.Clear();
            NotifyCountChanged();
        }
    }

    /// <summary>
    /// Tests if the collection contains the specified item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>True if the collection contains the item; otherwise false.</returns>
    public bool Contains(T item) 
    {
        return _inner.Contains(item);
    }

    /// <summary>
    /// Copies the collection's contents to an array.
    /// </summary>
    /// <param name="array">The array.</param>
    /// <param name="arrayIndex">The first index of the array to copy to.</param>
    public void CopyTo(T[] array, int arrayIndex)
    {
        _inner.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Returns an enumerator that enumerates the items in the collection.
    /// </summary>
    /// <returns>An <see cref="IEnumerator{T}"/>.</returns>
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => _inner.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => _inner.GetEnumerator();
    

    /// <inheritdoc/>
    int IList.Add(object? value)
    {
        return Add((T)value!);
    }

    /// <inheritdoc/>
    bool IList.Contains(object? value)
    {
        return Contains((T)value!);
    }

    /// <inheritdoc/>
    void IList.Clear()
    {
        Clear();
    }

    /// <inheritdoc/>
    int IList.IndexOf(object? value)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc/>
    void IList.Insert(int index, object? value)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc/>
    void IList.Remove(object? value)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc/>
    void IList.RemoveAt(int index)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc/>
    void ICollection.CopyTo(Array array, int index)
    {
        if (array == null)
        {
            throw new ArgumentNullException(nameof(array));
        }

        if (array.Rank != 1)
        {
            throw new ArgumentException("Multi-dimensional arrays are not supported.");
        }

        if (array.GetLowerBound(0) != 0)
        {
            throw new ArgumentException("Non-zero lower bounds are not supported.");
        }

        if (index < 0)
        {
            throw new ArgumentException("Invalid index.");
        }

        if (array.Length - index < Count)
        {
            throw new ArgumentException("The target array is too small.");
        }

        if (array is T[] tArray)
        {
            _inner.CopyTo(tArray, index);
        }
        else
        {
            //
            // Catch the obvious case assignment will fail.
            // We can't find all possible problems by doing the check though.
            // For example, if the element type of the Array is derived from T,
            // we can't figure out if we can successfully copy the element beforehand.
            //
            Type targetType = array.GetType().GetElementType()!;
            Type sourceType = typeof(T);
            if (!(targetType.IsAssignableFrom(sourceType) || sourceType.IsAssignableFrom(targetType)))
            {
                throw new ArgumentException("Invalid array type");
            }

            //
            // We can't cast array of value type to object[], so we don't support
            // widening of primitive types here.
            //
            if (array is not object?[] objects)
            {
                throw new ArgumentException("Invalid array type");
            }

            int count = _inner.Count;
            try
            {
                for (int i = 0; i < count; i++)
                {
                    objects[index++] = _inner[i];
                }
            }
            catch (ArrayTypeMismatchException)
            {
                throw new ArgumentException("Invalid array type");
            }
        }
    }

    /// <summary>
    /// Raises the <see cref="CollectionChanged"/> event with an add action.
    /// </summary>
    /// <param name="t">The items that were added.</param>
    /// <param name="index">The starting index.</param>
    private void NotifyAdd(IList t, int index)
    {
        if (_collectionChanged != null)
        {
            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, t, index);
            _collectionChanged(this, e);
        }

        NotifyCountChanged();
    }

    /// <summary>
    /// Raises the <see cref="CollectionChanged"/> event with an add action.
    /// </summary>
    /// <param name="item">The item that was added.</param>
    /// <param name="index">The starting index.</param>
    private void NotifyAdd(T item, int index)
    {
        if (_collectionChanged != null)
        {
            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new[] { item }, index);
            _collectionChanged(this, e);
        }

        NotifyCountChanged();
    }

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event when the <see cref="Count"/> property
    /// changes.
    /// </summary>
    private void NotifyCountChanged()
    {
        PropertyChanged?.Invoke(this, EventArgsCache.CountPropertyChanged);
    }

    /// <summary>
    /// Raises the <see cref="CollectionChanged"/> event with a remove action.
    /// </summary>
    /// <param name="t">The items that were removed.</param>
    /// <param name="index">The starting index.</param>
    private void NotifyRemove(IList t, int index)
    {
        if (_collectionChanged != null)
        {
            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, t, index);
            _collectionChanged(this, e);
        }

        NotifyCountChanged();
    }

    /// <summary>
    /// Raises the <see cref="CollectionChanged"/> event with a remove action.
    /// </summary>
    /// <param name="item">The item that was removed.</param>
    /// <param name="index">The starting index.</param>
    private void NotifyRemove(T item, int index)
    {
        if (_collectionChanged != null)
        {
            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new[] { item }, index);
            _collectionChanged(this, e);
        }

        NotifyCountChanged();
    }
    
}


internal static class EventArgsCache
{
    internal static readonly PropertyChangedEventArgs CountPropertyChanged =
        new PropertyChangedEventArgs(nameof(ObservableCircularQueue<object>.Count));

    internal static readonly NotifyCollectionChangedEventArgs ResetCollectionChanged =
        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
}