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
    
    public Unit(JObject attributes)
    {
        GameController gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();

        name = (string)attributes.GetValue("name");

        string texturePath = (string)attributes.GetValue("texture");
        texture = gameController.resourceManager.LoadTexture(texturePath);

        pathfindingTypes = new HashSet<string>();
        foreach (string pathfindingType in attributes.GetValue("pathfindingTypes")) {
            pathfindingTypes.Add(pathfindingType);
        }

        strength = (int)attributes.GetValue("strength");
        move = (int)attributes.GetValue("move");
        upkeep = (int)attributes.GetValue("upkeep");
        productionCost = (int)attributes.GetValue("productionCost");

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
