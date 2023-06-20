using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json.Linq;

public class Unit
{
    public string baseFile {get; private set;}

    public Army army {get; set;}

    public string name {get; set;}
    public Texture2D texture {get; private set;}
    public Texture2D maskTexture {get; private set;}

    public BattleStatsData battleStats {get; private set;}
    public EconomyData economy {get; private set;}
    public int productionCost {get; private set;}
    public int purchaseCost {get; private set;}

    public HeroData heroData {get; private set;}

    public bool isHero
    {
        get {
            return heroData != null;
        }
    }
    
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

    public Unit(JObject baseAttributes)
    {
        LoadBaseAttributes(baseAttributes);
    }

    /// Destroys this unit - removes it from its army, and drops all items to the ground, if it was a hero
    public void Destroy()
    {
        army.RemoveUnit(this);
        
        if (isHero) {
            while (heroData.items.Count > 0) {
                heroData.DropItem(heroData.items[0], army.position);
            }
        }
        
        army = null;

        EventManager.OnUnitDestroyed(this);
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

        unitJObject.Add("battleStats", battleStats.ToJObject());
        unitJObject.Add("economy", economy.ToJObject());

        unitJObject.Add("pathfinder", basePathfinder.ToJObject());
        if (isTransitioned) {
            unitJObject.Add("transitionPathfinder", transitionPathfinder.ToJObject());
        }

        if (isHero){
            unitJObject.Add("heroData", heroData.ToJObject());
        }

        unitJObject.Add("productionCost", productionCost);
        unitJObject.Add("purchaseCost", purchaseCost);

        ResourceManager.Minimize(unitJObject);

        return unitJObject;
    }

    /// Creates a new unit from JObject
    public static Unit FromJObject(JObject attributes)
    {
        return new Unit(attributes);
    }

    private void LoadBaseAttributes(JObject baseAttributes)
    {
        ResourceManager.ExpandWithBaseFile(baseAttributes);
        
        baseFile = null;
        if (baseAttributes.ContainsKey("baseFile")) {
            baseFile = (string)baseAttributes.GetValue("baseFile");
        }

        name = (string)baseAttributes.GetValue("name");

        string texturePath = (string)baseAttributes.GetValue("texture");
        texture = ResourceManager.LoadTexture(texturePath);

        if (baseAttributes.ContainsKey("maskTexture")) {
            string maskTexturePath = (string)baseAttributes.GetValue("maskTexture");
            maskTexture = ResourceManager.LoadTexture(maskTexturePath);
        } else {
            maskTexture = Texture2D.whiteTexture;
        }

        battleStats = BattleStatsData.FromJObject((JObject)baseAttributes.GetValue("battleStats"));
        economy = EconomyData.FromJObject((JObject)baseAttributes.GetValue("economy"));

        productionCost = (int)baseAttributes.GetValue("productionCost");
        purchaseCost = (int)baseAttributes.GetValue("purchaseCost");

        basePathfinder = PathfinderData.FromJObject((JObject)baseAttributes.GetValue("pathfinder"));
        transitionPathfinder = null;
        if (baseAttributes.ContainsKey("transitionPathfinder")) {
            transitionPathfinder = PathfinderData.FromJObject((JObject)baseAttributes.GetValue("transitionPathfinder"));
        }

        heroData = null;
        if (baseAttributes.ContainsKey("heroData")) {
            heroData = HeroData.FromJObject((JObject)baseAttributes.GetValue("heroData"));
        }
    }

    public override string ToString()
    {
        return $"Unit(name = {name}, pathfinderData = {pathfinder})";
    }
}
