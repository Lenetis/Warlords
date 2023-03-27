using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

public class Unit
{
    public string baseFile {get; private set;}

    public string name {get; set;}
    public Texture2D texture {get;}

    public int strength {get;}
    public int move {get;}
    public int remainingMove {get; set;}
    public HashSet<string> pathfindingTypes {get;}
    public int upkeep {get;}
    public int productionCost{get;}
    
    // todo change this constructor to use fewer arguments
    public Unit(string baseFile, string name, Texture2D texture, int strength, int move, int remainingMove, HashSet<string> pathfindingTypes, int upkeep, int productionCost)
    {
        this.baseFile = baseFile;

        this.name = name;
        this.texture = texture;
        this.strength = strength;
        this.move = move;
        this.remainingMove = remainingMove;
        this.pathfindingTypes = pathfindingTypes;
        this.upkeep = upkeep;
        this.productionCost = productionCost;
    }

    /// Resets remaining movement points of this unit
    public void StartTurn()
    {
        remainingMove = move;
    }

    /// Serializes this unit into a JObject
    public JObject ToJObject()
    {
        JObject unitJObject = new JObject();

        if (baseFile != null) {
            unitJObject.Add("baseFile", baseFile);
        }

        unitJObject.Add("name", name);
        unitJObject.Add("strength", strength);
        unitJObject.Add("move", move);
        unitJObject.Add("remainingMove", remainingMove);
        unitJObject.Add("pathfindingTypes", new JArray(pathfindingTypes));
        unitJObject.Add("upkeep", upkeep);
        unitJObject.Add("productionCost", productionCost);

        return unitJObject;
    }

    /// Creates a new unit from JObject
    public static Unit FromJObject(JObject attributes)
    {
        ResourceManager.ExpandWithBaseFile(attributes);
        string baseFile = null;
        if (attributes.ContainsKey("baseFile")) {
            baseFile = (string)attributes.GetValue("baseFile");
        }

        string name = (string)attributes.GetValue("name");

        string texturePath = (string)attributes.GetValue("texture");
        Texture2D texture = ResourceManager.LoadTexture(texturePath);

        HashSet<string> pathfindingTypes = new HashSet<string>();
        foreach (string pathfindingType in attributes.GetValue("pathfindingTypes")) {
            pathfindingTypes.Add(pathfindingType);
        }

        int strength = (int)attributes.GetValue("strength");
        int move = (int)attributes.GetValue("move");
        int upkeep = (int)attributes.GetValue("upkeep");
        int productionCost = (int)attributes.GetValue("productionCost");

        int remainingMove;
        if (attributes.ContainsKey("remainingMove")) {
            remainingMove = (int)attributes.GetValue("remainingMove");
        } else {
            remainingMove = move;
        }

        return new Unit(baseFile, name, texture, strength, move, remainingMove, pathfindingTypes, upkeep, productionCost);
    }

    public override string ToString()
    {
        return $"Unit(name = {name}, pathfindingTypes = [{string.Join(", ", pathfindingTypes)}], strength = {strength}, move = {move}, upkeep = {upkeep})";
    }
}
