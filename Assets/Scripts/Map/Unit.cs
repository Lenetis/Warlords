using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json.Linq;

public class Unit
{
    public string baseFile {get; private set;}

    public string name {get; set;}
    public Texture2D texture {get;}
    public Texture2D maskTexture {get;}

    public int strength {get;}
    public int upkeep {get;}
    public int productionCost {get;}
    public int purchaseCost {get;}

    public PathfinderData pathfinder
    {
        get {
            if (transitionPathfinder != null) {
                return transitionPathfinder;
            }
            return basePathfinder;
        }
    }
    public bool isTransitioned
    {
        get {
            return transitionPathfinder != null;
        }
    }
    public PathfinderData basePathfinder {get; private set;}
    public PathfinderData transitionPathfinder {get; private set;}
    
    // todo change this constructor to use fewer arguments
    public Unit(string baseFile, string name, Texture2D texture, Texture2D maskTexture, int strength, int upkeep, int productionCost, int purchaseCost, PathfinderData basePathfinder, PathfinderData transitionPathfinder)
    {
        this.baseFile = baseFile;

        this.name = name;
        this.texture = texture;
        this.maskTexture = maskTexture;
        this.strength = strength;
        this.upkeep = upkeep;
        this.productionCost = productionCost;
        this.purchaseCost = purchaseCost;
        this.basePathfinder = basePathfinder;
        this.transitionPathfinder = transitionPathfinder;
    }

    /// Resets remaining movement points of this unit
    public void StartTurn()
    {
        pathfinder.ResetMove();
    }

    /// Transitions this unit's pathfinder to another move type (e.g. when entering water from a port)
    public void Transition(PathfindingTransition transition)
    {
        // todo check if the transition's "from" pathfindingTypes match with the unit's
        int newMove = transition.move == null ? pathfinder.move : (int)transition.move;
        transitionPathfinder = new PathfinderData(newMove, 0, transition.to);
    }

    /// Returns this unit's pathfinder from another move type back to the base pathfinder (e.g. when returning to land from water from water)
    public void TransitionReturn()
    {
        transitionPathfinder = null;
        pathfinder.remainingMove = 0;
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

        unitJObject.Add("pathfinder", basePathfinder.ToJObject());
        if (isTransitioned) {
            unitJObject.Add("transitionPathfinder", transitionPathfinder.ToJObject());
        }

        unitJObject.Add("upkeep", upkeep);
        unitJObject.Add("productionCost", productionCost);
        unitJObject.Add("purchaseCost", purchaseCost);

        ResourceManager.Minimize(unitJObject);

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

        Texture2D maskTexture;
        if (attributes.ContainsKey("maskTexture")) {
            string maskTexturePath = (string)attributes.GetValue("maskTexture");
            maskTexture = ResourceManager.LoadTexture(maskTexturePath);
        } else {
            maskTexture = Texture2D.whiteTexture;
        }

        int strength = (int)attributes.GetValue("strength");
        int upkeep = (int)attributes.GetValue("upkeep");
        int productionCost = (int)attributes.GetValue("productionCost");
        int purchaseCost = (int)attributes.GetValue("purchaseCost");

        PathfinderData basePathfinder = PathfinderData.FromJObject((JObject)attributes.GetValue("pathfinder"));
        PathfinderData transitionPathfinder = null;
        if (attributes.ContainsKey("transitionPathfinder")) {
            transitionPathfinder = PathfinderData.FromJObject((JObject)attributes.GetValue("transitionPathfinder"));
        }

        return new Unit(baseFile, name, texture, maskTexture, strength, upkeep, productionCost, purchaseCost, basePathfinder, transitionPathfinder);
    }

    public override string ToString()
    {
        return $"Unit(name = {name}, strength = {strength}, upkeep = {upkeep}, pathfinderData = {pathfinder})";
    }
}
