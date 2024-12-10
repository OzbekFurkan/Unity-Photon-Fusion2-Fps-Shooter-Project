using System.Collections;
using System.Collections.Generic;

public class StateStack<T>
{
    private HashSet<T> hashSet = new HashSet<T>();
    private List<T> list = new List<T>();

    ///<summary>Add an element</summary>
    public bool Add(T item)
    {
        if (hashSet.Add(item)) // Add to HashSet (ensures uniqueness)
        {
            list.Add(item); // Add to List
            return true;
        }
        return false; // Item already exists
    }

    ///<summary>Remove an element</summary>
    public bool Remove(T item)
    {
        if (hashSet.Remove(item)) // Remove from HashSet
        {
            list.Remove(item); // Remove from List
            return true;
        }
        return false; // Item not found
    }

    ///<summary>Get the last element</summary>
    public T GetLast()
    {
        if (list.Count > 0)
        {
            return list[list.Count - 1];
        }
        throw new System.InvalidOperationException("No elements in the structure.");
    }

    ///<summary>Remove the last element</summary>
    public T RemoveLast()
    {
        if (list.Count > 0)
        {
            T lastItem = list[list.Count - 1];
            list.RemoveAt(list.Count - 1); // Remove from List
            hashSet.Remove(lastItem); // Remove from HashSet
            return lastItem;
        }
        throw new System.InvalidOperationException("No elements to remove.");
    }

    ///<summary>Check if the structure contains an element</summary>
    public bool Contains(T item)
    {
        return hashSet.Contains(item);
    }

    ///<summary>Clear all elements</summary>
    public void Clear()
    {
        hashSet.Clear();
        list.Clear();
    }

    ///<summary>Get the number of elements</summary>
    public int Count => list.Count;
}
