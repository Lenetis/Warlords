using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json.Linq;

public class PathfindingTransition
{
    public HashSet<string> from {get;}
    public HashSet<string> to {get;}
    public int? move {get;}
    public PathfindingTransition(HashSet<string> from, HashSet<string> to, int? move)
    {
        this.from = from;
        this.to = to;
        this.move = move;
    }

    public static PathfindingTransition FromJObject(JObject attributes)
    {
        HashSet<string> fromPathfindingTypes = new HashSet<string>();
        HashSet<string> toPathfindingTypes = null;
        int? move = null;

        foreach (string pathfindingType in attributes.GetValue("from")) {
            fromPathfindingTypes.Add(pathfindingType);
        }
        
        if (attributes.ContainsKey("to")) {
            toPathfindingTypes = new HashSet<string>();
            foreach (string pathfindingType in attributes.GetValue("to")) {
                toPathfindingTypes.Add(pathfindingType);
            }
        }

        if (attributes.ContainsKey("move")) {
            move = (int)attributes.GetValue("move");
        }

        return new PathfindingTransition(fromPathfindingTypes, toPathfindingTypes, move);
    }

    public JObject ToJObject()
    {
        JObject transitionJObject = new JObject();

        transitionJObject.Add("from", new JArray(from));
        if (to != null) {
            transitionJObject.Add("to", new JArray(to));
        }
        if (move != null) {
            transitionJObject.Add("move", move);
        }
        
        return transitionJObject;
    }
}