using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using Newtonsoft.Json.Linq;

public class Unit
{
    public string name {get; set;}
    public Texture2D texture {get;}

    public int strength {get;}
    public int move {get;}
    public int remainingMove {get; set;}
    public HashSet<string> pathfindingTypes {get;}
    public int upkeep {get;}
    public int productionCost{get;}
    
    public Unit(string jsonPath)
    {
        string json = File.ReadAllText(jsonPath);
        JObject jObject = JObject.Parse(json);

        name = (string)jObject.GetValue("name");

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

        strength = (int)jObject.GetValue("strength");
        move = (int)jObject.GetValue("move");
        upkeep = (int)jObject.GetValue("upkeep");
        productionCost = (int)jObject.GetValue("productionCost");

        remainingMove = move;
    }

    public void StartTurn()
    {
        remainingMove = move;
    }

    public override string ToString()
    {
        return $"Unit(name = {name}, pathfindingTypes = [{string.Join(", ", pathfindingTypes)}], strength = {strength}, move = {move}, upkeep = {upkeep})";
    }
}
