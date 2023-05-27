using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json.Linq;

public class PathfinderData
{
    public int move;
    public int remainingMove;
    public HashSet<string> pathfindingTypes;

    public PathfinderData(int move, int remainingMove, HashSet<string> pathfindingTypes)
    {
        this.move = move;
        this.remainingMove = remainingMove;
        this.pathfindingTypes = pathfindingTypes;
    }

    public void ResetMove()
    {
        remainingMove = move;
    }

    public static PathfinderData FromJObject(JObject attributes)
    {
        int move = (int)attributes.GetValue("move");
        int remainingMove = move;
        if (attributes.ContainsKey("remainingMove")) {
            remainingMove = (int)attributes.GetValue("remainingMove");
        }

        HashSet<string> pathfindingTypes = new HashSet<string>();
        foreach (string pathfindingType in attributes.GetValue("pathfindingTypes")) {
            pathfindingTypes.Add(pathfindingType);
        }

        return new PathfinderData(move, remainingMove, pathfindingTypes);
    }

    public JObject ToJObject()
    {
        JObject pathfinderJObject = new JObject();

        pathfinderJObject.Add("move", move);
        if (remainingMove != move) {
            pathfinderJObject.Add("remainingMove", remainingMove);
        }
        pathfinderJObject.Add("pathfindingTypes", new JArray(pathfindingTypes));
        
        return pathfinderJObject;
    }
}