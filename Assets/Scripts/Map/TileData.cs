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
    
    public TileData(string jsonPath)
    {
        string json = File.ReadAllText(jsonPath);
        JObject jObject = JObject.Parse(json);

        name = (string)jObject.GetValue("name");

        description = (string)jObject.GetValue("description");

        string texturePath = (string)jObject.GetValue("texture");
        byte[] binaryImageData = File.ReadAllBytes(texturePath);
        texture = new Texture2D(0,0);  // todo for some reason this works, but I *really* don't like this.
        texture.LoadImage(binaryImageData);
        texture.filterMode = FilterMode.Point;
        texture.Apply();

        pathfindingTypes = new HashSet<string>();
        foreach (string pathfindingType in jObject.GetValue("pathfindingTypes")) {
            pathfindingTypes.Add(pathfindingType);
        }

        moveCost = (int)jObject.GetValue("moveCost");
    }

    public override string ToString()
    {
        return $"TileData(name = {name}, description = {description}, pathfindingTypes = [{string.Join(", ", pathfindingTypes)}], moveCost = {moveCost})";
    }
}
