using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface IHeapItem<T> : IComparable<T>
{
    int HeapIndex {get; set;}
}
