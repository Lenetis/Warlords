using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingNode : IHeapItem<PathfindingNode>
{
    private static Dictionary<(Position, HashSet<string>), PathfindingNode> nodes = new Dictionary<(Position, HashSet<string>), PathfindingNode>();
    public readonly Position position;
    public readonly HashSet<string> pathfindingTypes;

    public int gCost;
    public int hCost;

    private int heapIndex;

    public int fCost
    {
        get {
            return gCost + hCost;
        }
    }

    public int HeapIndex
    {
        get {
            return heapIndex;
        }
        set {
            heapIndex = value;
        }
    }

    private PathfindingNode(Position position, HashSet<string> pathfindingTypes)
    {
        this.position = position;
        this.pathfindingTypes = pathfindingTypes;
    }

    public static PathfindingNode Get(Position position, HashSet<string> pathfindingTypes)
    {
        if (nodes.ContainsKey((position, pathfindingTypes))) {
            return nodes[(position, pathfindingTypes)];
        }
        else {
            PathfindingNode newNode = new PathfindingNode(position, pathfindingTypes);
            nodes[(position, pathfindingTypes)] = newNode;
            return newNode;
        }
    }

    public int CompareTo(PathfindingNode other)
    {
        int compare = fCost.CompareTo(other.fCost);
        if (compare == 0) {
            compare = hCost.CompareTo(other.hCost);
        }
        return -compare;
    }

    // override object.Equals
    public override bool Equals(object obj)
    {
        //
        // See the full list of guidelines at
        //   http://go.microsoft.com/fwlink/?LinkID=85237
        // and also the guidance for operator== at
        //   http://go.microsoft.com/fwlink/?LinkId=85238
        //
        
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        
        PathfindingNode other = (PathfindingNode)obj;
        return position == other.position && pathfindingTypes.SetEquals(other.pathfindingTypes);
    }
    
    // override object.GetHashCode
    public override int GetHashCode()
    {
        return position.GetHashCode();  // + pathfindingTypes.GetHashCode();
        // we only include the position HashCode, because two different pathfindingTypes HashSets (even with the same contents) will have different HashCodes.
        // required to guarantee that the "same" pathfinding nodes (with equal positions and pathfindingTypes) will appear only once in HashSets, Dictionaries, etc.
        // --- this is a very stupid solution and pathfindingTypes should probably be changed to a custom class instead of HashSet.  //todo
    }

    public override string ToString()
    {
        return $"{position}, {string.Join(", ", pathfindingTypes)}";
    }
}
