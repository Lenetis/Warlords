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

    public Position position {get;}
    public List<Position> occupiedPositions {get;}

    public List<Unit> buildableUnits {get;}
    private List<string> buildableUnitsPaths;  // will be used in saving

    public List<Unit> buyableUnits {get;}
    private List<string> buyableUnitsPaths;  // will be used in saving

    public int moveCost {get;}
    public HashSet<string> pathfindingTypes {get;}

    public int income {get;}
    public int production {get;}

    public bool producing {get; set;}
    private int producedUnitIndex;
    public Unit producedUnit
    {
        get {
            return buildableUnits[producedUnitIndex];
        }
        set {
            int unitIndex = buildableUnits.IndexOf(value);
            if (unitIndex == -1) {
                throw new System.ArgumentException("Cannot set production to a non-buildable unit");
            }
            producedUnitIndex = unitIndex;
            productionProgress = 0;
            producing = true;
        }
    }
    public int productionProgress {get; private set;}

    private GameObject mapSprite;  // todo to remove

    public bool razed {get; private set;}

    // todo change this constructor to use way less arguments (pack argument groups into structs maybe?)
    public City(string baseFile, Player owner, string name, string description, Position position, List<Position> occupiedPositions, int moveCost, HashSet<string> pathfindingTypes, int income, int production, bool razed, List<Unit> buildableUnits, List<Unit> buyableUnits, int? producedUnitIndex, int productionProgress)
    {
        this.baseFile = baseFile;

        this.owner = owner;
        this.name = name;
        this.description = description;
        this.position = position;
        this.occupiedPositions = occupiedPositions;
        this.moveCost = moveCost;
        this.pathfindingTypes = pathfindingTypes;
        this.income = income;
        this.production = production;
        this.razed = razed;
        this.buildableUnits = buildableUnits;
        this.buyableUnits = buyableUnits;
        
        if (producedUnitIndex != null) {
            producing = true;
            this.producedUnitIndex = (int)producedUnitIndex;
        } else {
            producing = false;
            this.producedUnitIndex = 0;
        }
        this.productionProgress = productionProgress;

        mapSprite = new GameObject("City");
        mapSprite.transform.position = position;
        mapSprite.AddComponent<SpriteRenderer>();

        Recalculate();
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
        buildableUnitsPaths.Clear();
        producing = false;

        Recalculate();

        gameController.RazeCity(this);
        owner = null;
    }

    /// Changes the owner of this city to newOwner
    public void Capture(Player newOwner)
    {
        producing = false;
        owner = newOwner;

        Recalculate();

        gameController.CaptureCity(this, newOwner);
    }

    /// Destroys the city, completely removing it from the game
    public void Destroy()
    {
        GameObject.Destroy(mapSprite);
        gameController.DestroyCity(this);
    }

    /// Starts turn this city - calculates unit production
    public void StartTurn()
    {
        if (producing) {
            productionProgress += production;
            Debug.Log($"{name} - progress: {productionProgress}/{producedUnit.productionCost}");
            if (productionProgress >= producedUnit.productionCost) {
                Debug.Log("added new Army");

                // todo find a free tile within occupiedPositions
                //      (what if there is none???)
                Position freePosition = position;

                Unit newUnit = Unit.FromJObject(ResourceManager.LoadResource(buildableUnits[producedUnitIndex].baseFile));
                List<Unit> unitList = new List<Unit>();
                unitList.Add(newUnit);
                Army producedArmy = new Army(unitList, position, owner);

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
        
        string baseFile = null;
        if (attributes.ContainsKey("baseFile")) {
            baseFile = (string)attributes.GetValue("baseFile");
        }

        string ownerName = (string)attributes.GetValue("owner");
        Player owner = (ownerName == null) ? null : gameController.GetPlayerByName(ownerName);

        string name = (string)attributes.GetValue("name");
        string description = (string)attributes.GetValue("description");

        Position position = new Position((int)attributes.GetValue("position")[0], (int)attributes.GetValue("position")[1]);

        List<Position> occupiedPositions = new List<Position>();
        foreach (JToken token in attributes.GetValue("occupiedPositions")) {
            occupiedPositions.Add(new Position((int)token[0], (int)token[1]));
        }

        int moveCost = (int)attributes.GetValue("moveCost");

        HashSet<string> pathfindingTypes = new HashSet<string>();
        foreach (string pathfindingType in attributes.GetValue("pathfindingTypes")) {
            pathfindingTypes.Add(pathfindingType);
        }

        int income = (int)attributes.GetValue("income");
        int production = (int)attributes.GetValue("production");

        bool razed = false;
        if (attributes.ContainsKey("razed")) {
            razed = (bool)attributes.GetValue("razed");
        }

        
        List<Unit> buildableUnits = new List<Unit>();
        List<Unit> buyableUnits = new List<Unit>();
        int? producedUnitIndex = null;
        int productionProgress = 0;

        if (!razed) {
            foreach (string unitPath in attributes.GetValue("buildableUnits")) {
                buildableUnits.Add(Unit.FromJObject(ResourceManager.LoadResource(unitPath)));
            }
            
            foreach (string unitPath in attributes.GetValue("buyableUnits")) {
                buyableUnits.Add(Unit.FromJObject(ResourceManager.LoadResource(unitPath)));
            }

            if (attributes.ContainsKey("producedUnitIndex")) {
                producedUnitIndex =(int)attributes.GetValue("producedUnitIndex");
                productionProgress = (int)attributes.GetValue("productionProgress");
            }
        }

        return new City(baseFile, owner, name, description, position, occupiedPositions, moveCost, pathfindingTypes, income, production, razed, buildableUnits, buyableUnits, producedUnitIndex, productionProgress);
    }

    public override string ToString()
    {
        return name;
    }
}
