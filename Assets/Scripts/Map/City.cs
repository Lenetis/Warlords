using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using Newtonsoft.Json.Linq;

public class City : IPlayerMapObject
{
    private GameController gameController;

    public Player owner {get; private set;}

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

    public string name {get; set;}
    public string description {get; private set;}

    public City(JObject attributes, Position position, Player owner, string name, string description)
    {
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();

        this.position = position;
        this.owner = owner;
        razed = false;
        producing = false;

        this.name = name;
        this.description = description;

        mapSprite = new GameObject("City");
        mapSprite.transform.position = position;
        mapSprite.AddComponent<SpriteRenderer>();

        occupiedPositions = new List<Position>();
        foreach (JToken jsonPosition in attributes.GetValue("occupiedPositions")) {
            occupiedPositions.Add(new Position((int)jsonPosition[0], (int)jsonPosition[1]));
        }

        buildableUnits = new List<Unit>();
        buildableUnitsPaths = new List<string>();
        foreach (string unitPath in attributes.GetValue("buildableUnits")) {
            buildableUnits.Add(new Unit(gameController.resourceManager.LoadResource(unitPath)));
            buildableUnitsPaths.Add(unitPath);
        }

        buyableUnits = new List<Unit>();
        buyableUnitsPaths = new List<string>();
        foreach (string unitPath in attributes.GetValue("buyableUnits")) {
            buyableUnits.Add(new Unit(gameController.resourceManager.LoadResource(unitPath)));
            buyableUnitsPaths.Add(unitPath);
        }

        income = (int)attributes.GetValue("income");
        production = (int)attributes.GetValue("production");

        pathfindingTypes = new HashSet<string>();
        foreach (string pathfindingType in attributes.GetValue("pathfindingTypes")) {
            pathfindingTypes.Add(pathfindingType);
        }

        moveCost = (int)attributes.GetValue("moveCost");

        gameController.AddCity(this);

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
        owner = null;

        gameController.RazeCity(this);
    }

    /// Changes the owner of this city to newOwner
    public void Capture(Player newOwner)
    {
        producing = false;
        owner = newOwner;

        Recalculate();

        gameController.CaptureCity(this, newOwner);
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

                Unit newUnit = new Unit(gameController.resourceManager.LoadResource(buildableUnitsPaths[producedUnitIndex]));
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

    public override string ToString()
    {
        return name;
    }
}
