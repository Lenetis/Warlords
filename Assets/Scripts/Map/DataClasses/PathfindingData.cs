using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json.Linq;

public class PathfindingData
{
    public int moveCost;
    public HashSet<string> pathfindingTypes;
    public PathfindingTransition transition;
    public PathfindingTransition transitionReturn;

    public PathfindingData(int moveCost, HashSet<string> pathfindingTypes, PathfindingTransition transition, PathfindingTransition transitionReturn)
    {
        this.moveCost = moveCost;
        this.pathfindingTypes = pathfindingTypes;
        this.transition = transition;
        this.transitionReturn = transitionReturn;
    }

    public static PathfindingData FromJObject(JObject attributes)
    {
        int moveCost = (int)attributes.GetValue("moveCost");

        HashSet<string> pathfindingTypes = new HashSet<string>();
        foreach (string pathfindingType in attributes.GetValue("pathfindingTypes")) {
            pathfindingTypes.Add(pathfindingType);
        }

        PathfindingTransition transition = null;
        if (attributes.ContainsKey("transition")) {
            transition = PathfindingTransition.FromJObject((JObject)attributes.GetValue("transition"));
        }

        PathfindingTransition transitionReturn = null;
        if (attributes.ContainsKey("transitionReturn")) {
            transitionReturn = PathfindingTransition.FromJObject((JObject)attributes.GetValue("transitionReturn"));
        }

        return new PathfindingData(moveCost, pathfindingTypes, transition, transitionReturn);
    }

    public JObject ToJObject()
    {
        JObject pathfindingJObject = new JObject();

        pathfindingJObject.Add("moveCost", moveCost);
        pathfindingJObject.Add("pathfindingTypes", new JArray(pathfindingTypes));

        if (transition != null) {
            pathfindingJObject.Add("transition", transition.ToJObject());
        }
        if (transitionReturn != null) {
            pathfindingJObject.Add("transitionReturn", transitionReturn.ToJObject());
        }
        
        return pathfindingJObject;
    }
}