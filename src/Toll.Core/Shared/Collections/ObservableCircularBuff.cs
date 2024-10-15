using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Toll.Core.Shared.Collections;

public class ObservableCircularBuff<T> : IDwBuff<T>
{
    private readonly CircularBuffer<T> inner;
    private NotifyCollectionChangedEventHandler? collectionChanged;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableCircularBuff{T}"/>.
    /// </summary>
    /// <param name="capacity">Initial list capacity.</param>
    public ObservableCircularBuff(int capacity = 0)
    {
        inner = new(capacity);
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableCircularBuff{T}"/> class.
    /// </summary>
    /// <param name="items">The initial items for the collection.</param>
    public ObservableCircularBuff(IEnumerable<T> items)
    {
        inner = new(items);
    }
    
    public ObservableCircularBuff(params T[] items)
    {
        inner = new(items);
    }
    
    /// <summary>
    /// Raised when a change is made to the collection's items.
    /// </summary>
    public event NotifyCollectionChangedEventHandler? CollectionChanged
    {
        add => collectionChanged += value;
        remove => collectionChanged -= value;
    }
    
    /// <summary>
    /// Raised when a property on the collection changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets the number of items in the collection.
    /// </summary>
    public int Count => inner.Count;
    

    /// <summary>
    /// Gets or sets the total number of elements the internal data structure can hold without resizing.
    /// </summary>
    public int Capacity
    {
        get => inner.Capacity;
        set => inner.Capacity = value;
    }
    
    public bool IsReadOnly => inner.IsReadOnly;
    
    /// <summary>
    /// Append an item to the collection.
    /// </summary>
    /// <param name="item">The item.</param>
    public T? Append(T item)
    {
        int appendIndex;
        var availableRemove = inner.Append(item);
        if (availableRemove != null)
        {
            NotifyRemove(availableRemove, 0);
        }
        NotifyAdd(item, inner.Count);
        return availableRemove;
    }

    /// <summary>
    /// Removes all items from the collection.
    /// </summary>
    public void Clear()
    {
        if (Count > 0)
        {
            inner.Clear();
            if (collectionChanged != null)
            {
                collectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }

            NotifyCountChanged();
        }
    }
    
    /// <summary>
    /// Tests if the collection contains the specified item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>True if the collection contains the item; otherwise false.</returns>
    public bool Contains(T item) => inner.Contains(item);

    /// <summary>
    /// Copies the collection's contents to an array.
    /// </summary>
    /// <param name="array">The array.</param>
    /// <param name="arrayIndex">The first index of the array to copy to.</param>
    public void CopyTo(T[] array, int arrayIndex)
    {
        inner.CopyTo(array, arrayIndex);
    }
    
    public T this[int index] => inner[index];
    
    /// <summary>
    /// Returns an enumerator that enumerates the items in the collection.
    /// </summary>
    /// <returns>An <see cref="IEnumerator{T}"/>.</returns>
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => inner.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => inner.GetEnumerator();
    
    public CircularBuffer<T>.Enumerator GetEnumerator() => inner.GetEnumerator();

    /// <summary>
    /// Raises the <see cref="CollectionChanged"/> event with an add action.
    /// </summary>
    /// <param name="t">The items that were added.</param>
    /// <param name="index">The starting index.</param>
    private void NotifyAdd(IList t, int index)
    {
        if (collectionChanged != null)
        {
            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, t, index);
            collectionChanged(this, e);
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
        if (collectionChanged != null)
        {
            collectionChanged(this, EventArgsCache.ResetCollectionChanged);
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
        if (collectionChanged != null)
        {
            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, t, index);
            collectionChanged(this, e);
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
        if (collectionChanged != null)
        {
            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new[] { item }, index);
            collectionChanged(this, e);
        }

        NotifyCountChanged();
    }
    
    
    internal static class EventArgsCache
    {
        internal static readonly PropertyChangedEventArgs CountPropertyChanged = new PropertyChangedEventArgs(nameof(ObservableCircularBuff<object>.Count));
        internal static readonly NotifyCollectionChangedEventArgs ResetCollectionChanged = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
    }


}