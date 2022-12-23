using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using Newtonsoft.Json.Linq;

public class TileData
{
    public string name;
    public string description;
    public List<string> pathfindingTypes;  // todo change to a set maybe?
    public float moveCost;
    public Texture2D texture;

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
        texture.Apply();

        pathfindingTypes = new List<string>();
        foreach (string pathfindingType in jObject.GetValue("pathfindingTypes")) {
            pathfindingTypes.Add(pathfindingType);
        }

        moveCost = (float)jObject.GetValue("moveCost");   
    }

    public override string ToString()
    {
        return $"TileData(name = {name}, description = {description}, pathfindingTypes = [{string.Join(", ", pathfindingTypes)}], moveCost = {moveCost})";
    }
}
