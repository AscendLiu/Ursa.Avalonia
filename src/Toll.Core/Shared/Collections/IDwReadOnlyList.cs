using System.Collections.Specialized;
using System.ComponentModel;

namespace Toll.Core.Shared.Collections;

/// <summary>
/// A read-only notifying list.
/// </summary>
/// <typeparam name="T">The type of the items in the list.</typeparam>
public interface IDwReadOnlyList<out T> : IReadOnlyList<T>, INotifyCollectionChanged, INotifyPropertyChanged
{
    
}