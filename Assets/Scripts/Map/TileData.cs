using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using Newtonsoft.Json.Linq;

public class TileData
{
    public string name {get;}
    public string description {get;}
    public Texture2D texture {get;}

    public int moveCost {get;}
    public HashSet<string> pathfindingTypes {get;}
    
    public TileData(JObject attributes)
    {
        name = (string)attributes.GetValue("name");

        description = (string)attributes.GetValue("description");

        string texturePath = (string)attributes.GetValue("texture");
        texture = ResourceManager.LoadTexture(texturePath);

        pathfindingTypes = new HashSet<string>();
        foreach (string pathfindingType in attributes.GetValue("pathfindingTypes")) {
            pathfindingTypes.Add(pathfindingType);
        }

        moveCost = (int)attributes.GetValue("moveCost");
    }

    public override string ToString()
    {
        return $"TileData(name = {name}, description = {description}, pathfindingTypes = [{string.Join(", ", pathfindingTypes)}], moveCost = {moveCost})";
    }
}
