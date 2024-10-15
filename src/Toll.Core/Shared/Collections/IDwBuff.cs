namespace Toll.Core.Shared.Collections;

/// <summary>
/// A notifying list.
/// </summary>
/// <typeparam name="T">The type of the items in the list.</typeparam>
public interface IDwBuff<T> : IBuff<T>, IDwReadOnlyList<T>
{
    
}