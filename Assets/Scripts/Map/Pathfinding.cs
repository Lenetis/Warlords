using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

public class Pathfinding
{
    public int moveCost;
    public HashSet<string> pathfindingTypes;

    public Pathfinding(int moveCost, IEnumerable<string> pathfindingTypes)
    {
        this.moveCost = moveCost;
        this.pathfindingTypes = (HashSet<string>)pathfindingTypes;
    }

    public static Pathfinding FromJObject(JObject attributes)
    {
        int moveCost = (int)attributes.GetValue("moveCost");
        HashSet<string> pathfindingTypes = new HashSet<string>();
        foreach (string pathfindingType in attributes.GetValue("pathfindingTypes")) {
            pathfindingTypes.Add(pathfindingType);
        }

        return new Pathfinding(moveCost, pathfindingTypes);
    }

    public JObject ToJObject()
    {
        JObject pathfindingJObject = new JObject();

        pathfindingJObject.Add("moveCost", moveCost);
        pathfindingJObject.Add("pathfindingTypes", new JArray(pathfindingTypes));
        
        return pathfindingJObject;
    }
}