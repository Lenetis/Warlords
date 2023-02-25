using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using Newtonsoft.Json.Linq;

public class City : IPlayerMapObject
{
    public Player owner {get; private set;}

    public Position position {get;}
    private List<Position> occupiedPositions;

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
    public int productionProgress; //;)

    private GameObject mapSprite;

    public bool razed {get; private set;}

    public string name {get; set;}
    public string description {get; private set;}

    public City(string jsonPath, Position position, Player owner, string name, string description)
    {
        this.position = position;
        this.owner = owner;
        razed = false;
        producing = false;

        this.name = name;
        this.description = description;

        mapSprite = new GameObject("City");
        mapSprite.transform.position = position;
        mapSprite.AddComponent<SpriteRenderer>();

        string json = File.ReadAllText(jsonPath);
        JObject jObject = JObject.Parse(json);

        occupiedPositions = new List<Position>();
        foreach (JToken jsonPosition in jObject.GetValue("occupiedPositions")) {
            occupiedPositions.Add(new Position((int)jsonPosition[0], (int)jsonPosition[1]));
        }

        buildableUnits = new List<Unit>();
        buildableUnitsPaths = new List<string>();
        foreach (string unitPath in jObject.GetValue("buildableUnits")) {
            buildableUnits.Add(new Unit(unitPath));
            buildableUnitsPaths.Add(unitPath);
        }

        buyableUnits = new List<Unit>();
        buyableUnitsPaths = new List<string>();
        foreach (string unitPath in jObject.GetValue("buyableUnits")) {
            buyableUnits.Add(new Unit(unitPath));
            buyableUnitsPaths.Add(unitPath);
        }

        income = (int)jObject.GetValue("income");
        production = (int)jObject.GetValue("production");

        pathfindingTypes = new HashSet<string>();
        foreach (string pathfindingType in jObject.GetValue("pathfindingTypes")) {
            pathfindingTypes.Add(pathfindingType);
        }

        moveCost = (int)jObject.GetValue("moveCost");

        TileMap tileMap = GameObject.FindGameObjectWithTag("TileMap").GetComponent<TileMap>();
        foreach (Position occupiedPosition in occupiedPositions) {
            Tile occupiedTile = tileMap.GetTile(position + occupiedPosition);
            if (occupiedTile.contents.city != null) {
                throw new System.ArgumentException($"Position {position + occupiedPosition} is already occupied by another city");
            }
            occupiedTile.contents.city = this;
        }

        owner.AddCity(this);

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

        if (razed) {
            owner = null;
            // todo remove city from owner.cities
        }
    }

    public void Raze()
    {
        razed = true;
        buildableUnits.Clear();
        buildableUnitsPaths.Clear();
        producing = false;

        Recalculate();
        owner.RemoveCity(this);
        owner = null;
    }

    public void Capture(Player newOwner)
    {
        producing = false;
        owner.RemoveCity(this);
        newOwner.AddCity(this);
        owner = newOwner;

        Recalculate();
    }

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

                Unit newUnit = new Unit(buildableUnitsPaths[producedUnitIndex]);
                List<Unit> unitList = new List<Unit>();
                unitList.Add(newUnit);
                Army producedArmy = new Army(unitList, position, owner);

                productionProgress = 0;
            }
        }
    }

    public List<Army> GetSupportingArmies()
    {
        List<Army> supportingArmies = new List<Army>();

        TileMap tileMap = GameObject.FindGameObjectWithTag("TileMap").GetComponent<TileMap>();
        foreach (Position occupiedPosition in occupiedPositions) {
            Tile occupiedTile = tileMap.GetTile(position + occupiedPosition);
            if (occupiedTile.contents.armies != null) {
                supportingArmies.AddRange(occupiedTile.contents.armies);
            }
        }
        return supportingArmies;
    }

    public bool OccupiesPosition(Position position) {
        return occupiedPositions.Contains(position - this.position);
    }

    public override string ToString()
    {
        return name;
    }
}
