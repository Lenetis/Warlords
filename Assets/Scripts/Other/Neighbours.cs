using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public struct Neighbours<T> : IEnumerable<T>
{
    public T left;
    public T right;
    public T top;
    public T bottom;

    public T topLeft;
    public T topRight;
    public T bottomLeft;
    public T bottomRight;

    public IEnumerator<T> GetEnumerator()  // (required so we can do "foreach (... in Neighbours)") 
    {
        yield return left;
        yield return right;
        yield return top;
        yield return bottom;

        yield return topLeft;
        yield return topRight;
        yield return bottomLeft;
        yield return bottomRight;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}