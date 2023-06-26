using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json.Linq;

public class PathfinderData
{
    public int move {get; private set;}
    public int usedMove {get; set;}
    public int remainingMove
    {
        get {return Mathf.Max(0, move - usedMove);}
    }
    public HashSet<string> pathfindingTypes {get; set;}

    public PathfinderData(int move, int usedMove, HashSet<string> pathfindingTypes)
    {
        this.move = move;
        this.usedMove = usedMove;
        this.pathfindingTypes = pathfindingTypes;
    }

    public void AddPathfinder(PathfinderData otherPathfinder)
    {
        move += otherPathfinder.move;
        usedMove += otherPathfinder.usedMove;
        pathfindingTypes.UnionWith(otherPathfinder.pathfindingTypes);
    }

    public void RemovePathfinder(PathfinderData otherPathfinder)
    {
        move -= otherPathfinder.move;
        usedMove -= otherPathfinder.usedMove;
        pathfindingTypes.ExceptWith(otherPathfinder.pathfindingTypes);
    }

    public void ResetMove()
    {
        usedMove = 0;
    }

    public static PathfinderData FromJObject(JObject attributes)
    {
        int move = (int)attributes.GetValue("move");
        int usedMove = 0;
        if (attributes.ContainsKey("usedMove")) {
            usedMove = (int)attributes.GetValue("usedMove");
        }

        HashSet<string> pathfindingTypes = new HashSet<string>();
        foreach (string pathfindingType in attributes.GetValue("pathfindingTypes")) {
            pathfindingTypes.Add(pathfindingType);
        }

        return new PathfinderData(move, usedMove, pathfindingTypes);
    }

    public JObject ToJObject()
    {
        JObject pathfinderJObject = new JObject();

        pathfinderJObject.Add("move", move);
        if (usedMove != 0) {
            pathfinderJObject.Add("usedMove", usedMove);
        }
        pathfinderJObject.Add("pathfindingTypes", new JArray(pathfindingTypes));
        
        return pathfinderJObject;
    }
}