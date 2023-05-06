using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using Newtonsoft.Json.Linq;

public class City : IPlayerMapObject
{
    public string baseFile {get; private set;}

    private static GameController gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();

    public Player owner {get; private set;}

    
    public string name {get; set;}
    public string description {get; private set;}

    public Position position {get; private set;}
    public List<Position> occupiedPositions {get; private set;}

    public List<Unit> buildableUnits {get; private set;}

    public List<Unit> buyableUnits {get; private set;}

    public int moveCost {get; private set;}
    public HashSet<string> pathfindingTypes {get; private set;}

    public int income {get; private set;}
    public int production {get; private set;}

    public bool producing {get; set;}
    private int producedUnitIndex;
    public Unit producedUnit
    {
        get {
            if (producing == false) {
                return null;
            }
            return buildableUnits[producedUnitIndex];
        }
        set {
            if (value == null) {
                producing = false;
            }
            else {
                int unitIndex = buildableUnits.IndexOf(value);
                if (unitIndex == -1) {
                    throw new System.ArgumentException("Cannot set production to a non-buildable unit");
                }
                producedUnitIndex = unitIndex;
                producing = true;
            }
            productionProgress = 0;
        }
    }
    public int productionProgress {get; private set;}

    private GameObject mapSprite;  // todo to remove

    public bool razed {get; private set;}


    public City(JObject baseAttributes, Player owner, string name, string description, Position position)
    {
        LoadBaseAttributes(baseAttributes);

        this.owner = owner;
        this.name = name;
        this.description = description;
        this.position = position;

        mapSprite = new GameObject($"City({name})");
        mapSprite.transform.position = position;
        mapSprite.AddComponent<SpriteRenderer>();

        Recalculate();

        AddToGame();
    }

    // todo change name to something more descriptive 
    // (function is called everytime the city owner changes, to change the city mapSprite)
    private void Recalculate()
    {
        Texture2D texture;
        if (!razed) {
            texture = owner.cityTexture;
        } else {
            texture = owner.razedCityTexture;
        }
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0), 32);
        SpriteRenderer spriteRenderer = mapSprite.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingOrder = 10;
        spriteRenderer.color = owner.color;  // todo change the recoloring to something more fancy
    }

    /// Sets the city to an inactive state (unable to produce any units) and removes it from its owner cities
    public void Raze()
    {
        razed = true;
        buildableUnits.Clear();
        producing = false;

        Recalculate();

        owner.RemoveCity(this);
        owner = null;

        EventManager.OnCityRazed(this);
    }

    /// Changes the owner of this city to newOwner
    public void Capture(Player newOwner)
    {
        producing = false;
        owner = newOwner;

        Recalculate();

        owner.RemoveCity(this);
        newOwner.AddCity(this);

        EventManager.OnCityCaptured(this);
    }

    /// Adds this city to the tileMap and to its owner cities
    private void AddToGame()
    {
        foreach (Position occupiedPosition in occupiedPositions) {
            Tile occupiedTile = gameController.tileMap.GetTile(position + occupiedPosition);
            occupiedTile.AddCity(this);
        }
        owner.AddCity(this);

        EventManager.OnCityCreated(this);
    }

    /// Destroys the city, completely removing it from the game
    public void Destroy()
    {
        GameObject.Destroy(mapSprite);
        
        if (owner != null) {
            owner.RemoveCity(this);
        }
        foreach (Position occupiedPosition in occupiedPositions) {
            gameController.tileMap.GetTile(position + occupiedPosition).RemoveCity();
        }

        EventManager.OnCityDestroyed(this);
    }

    /// TODO OPIS
    public void BuyUnit(Unit unit, int replaceIndex)
    {
        if (buyableUnits.Contains(unit)) {
            if (!buildableUnits.Any(buildableUnit => buildableUnit.baseFile == unit.baseFile)) {
                owner.gold -= unit.purchaseCost;
                if (replaceIndex >= buildableUnits.Count) {
                    buildableUnits.Add(unit);
                }
                else {
                    buildableUnits[replaceIndex] = unit;
                    if (producedUnitIndex == replaceIndex) {
                        producing = false;
                    }
                }
            }
            else {
                throw new System.ArgumentException("Cannot buy a unit that has already been bought");
            }
        }
        else {
            throw new System.ArgumentException("Cannot buy a non-buyable unit");
        }
    }

    /// Starts turn this city - calculates unit production
    public void StartTurn()
    {
        if (producing) {
            productionProgress += production;
            Debug.Log($"{name} - progress: {productionProgress}/{producedUnit.productionCost}");
            if (productionProgress >= producedUnit.productionCost) {
                Debug.Log("created new Army");

                // todo find a free tile within occupiedPositions
                //      (what if there is none???)
                Position freePosition = position;

                Unit newUnit = Unit.FromJObject(ResourceManager.LoadResource(buildableUnits[producedUnitIndex].baseFile));
                List<Unit> unitList = new List<Unit>();
                unitList.Add(newUnit);
                new Army(unitList, position, owner);

                productionProgress = 0;
            }
        }
    }

    /// Returns a list of all armies that will support this city if it is attacked (all armies within the city's occupied positions)
    public List<Army> GetSupportingArmies()
    {
        List<Army> supportingArmies = new List<Army>();

        TileMap tileMap = GameObject.FindGameObjectWithTag("TileMap").GetComponent<TileMap>();
        foreach (Position occupiedPosition in occupiedPositions) {
            Tile occupiedTile = tileMap.GetTile(position + occupiedPosition);
            if (occupiedTile.armies != null) {
                supportingArmies.AddRange(occupiedTile.armies);
            }
        }
        return supportingArmies;
    }

    /// Returns true if the position is occupied by this city and false otherwise
    public bool OccupiesPosition(Position position) {
        return occupiedPositions.Contains(position - this.position);
    }

    /// Serializes this city into a JObject
    public JObject ToJObject()
    {
        JObject cityJObject = new JObject();

        if (baseFile != null) {
            cityJObject.Add("baseFile", baseFile);
        }

        cityJObject.Add("owner", owner?.name);
        cityJObject.Add("name", name);
        cityJObject.Add("description", description);
        cityJObject.Add("position", new JArray(position.x, position.y));
        cityJObject.Add("occupiedPositions", new JArray(occupiedPositions.Select(position => new JArray(position.x, position.y))));

        cityJObject.Add("moveCost", moveCost);
        cityJObject.Add("pathfindingTypes", new JArray(pathfindingTypes));

        cityJObject.Add("income", income);
        cityJObject.Add("production", production);

        if (razed) {
            // cities are by default not razed, so no need to save it in that case
            cityJObject.Add("razed", true);
        } else {
            // cities can buy/produce units only when they are not razed
            cityJObject.Add("buildableUnits", new JArray(buildableUnits.Select(unit => unit.ToJObject().GetValue("baseFile"))));
            cityJObject.Add("buyableUnits", new JArray(buyableUnits.Select(unit => unit.ToJObject().GetValue("baseFile"))));
            if (producing) {
                // save the produced unit only if the city is producing something
                cityJObject.Add("producedUnitIndex", producedUnitIndex);
                cityJObject.Add("productionProgress", productionProgress);
            }
        }

        return cityJObject;
    }

    /// Creates a new city from JObject
    public static City FromJObject(JObject attributes)
    {
        ResourceManager.ExpandWithBaseFile(attributes);

        string ownerName = (string)attributes.GetValue("owner");
        Player owner = (ownerName == null) ? null : gameController.GetPlayerByName(ownerName);

        string name = (string)attributes.GetValue("name");
        string description = (string)attributes.GetValue("description");

        Position position = new Position((int)attributes.GetValue("position")[0], (int)attributes.GetValue("position")[1]);

        City newCity = new City(attributes, owner, name, description, position);

        if (!newCity.razed) {
            if (attributes.ContainsKey("producedUnitIndex")) {
                newCity.producedUnitIndex =(int)attributes.GetValue("producedUnitIndex");
                newCity.productionProgress = (int)attributes.GetValue("productionProgress");
            }
        }

        return newCity;
    }

    /// Loads the base attributes (the ones that are guaranteed to be in every city JObject)
    private void LoadBaseAttributes(JObject baseAttributes)
    {
        ResourceManager.ExpandWithBaseFile(baseAttributes);
        
        baseFile = null;
        if (baseAttributes.ContainsKey("baseFile")) {
            baseFile = (string)baseAttributes.GetValue("baseFile");
        }

        occupiedPositions = new List<Position>();
        foreach (JToken token in baseAttributes.GetValue("occupiedPositions")) {
            occupiedPositions.Add(new Position((int)token[0], (int)token[1]));
        }
        moveCost = (int)baseAttributes.GetValue("moveCost");

        pathfindingTypes = new HashSet<string>();
        foreach (string pathfindingType in baseAttributes.GetValue("pathfindingTypes")) {
            pathfindingTypes.Add(pathfindingType);
        }

        income = (int)baseAttributes.GetValue("income");
        production = (int)baseAttributes.GetValue("production");

        razed = false;
        if (baseAttributes.ContainsKey("razed")) {
            razed = (bool)baseAttributes.GetValue("razed");
        }
        
        buildableUnits = new List<Unit>();
        buyableUnits = new List<Unit>();

        if (!razed) {
            foreach (string unitPath in baseAttributes.GetValue("buildableUnits")) {
                buildableUnits.Add(Unit.FromJObject(ResourceManager.LoadResource(unitPath)));
            }
            
            foreach (string unitPath in baseAttributes.GetValue("buyableUnits")) {
                buyableUnits.Add(Unit.FromJObject(ResourceManager.LoadResource(unitPath)));
            }
        }
    }

    public override string ToString()
    {
        return name;
    }
}
