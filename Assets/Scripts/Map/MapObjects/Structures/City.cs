using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using Newtonsoft.Json.Linq;

public class City : MultitileStructure, IOwnableMapObject
{
    public string baseFile {get; private set;}

    private Player _owner;
    public Player owner
    {
        get {
            if (razed) {
                return null;
                // a razed city technically doesn't need to have an owner, but we still need a reference to the owner's city textures
            }
            return _owner;
        }
        private set {
            _owner = value;
        }
    }

    
    public string name {get; set;}
    public string description {get; private set;}

    public List<Unit> buildableUnits {get; private set;}

    public List<Unit> buyableUnits {get; private set;}

    public EconomyData economy {get; private set;}
    public BattleStatsData battleStats {get; private set;}

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

    public bool razed {get; private set;}


    public City(JObject baseAttributes, Player owner, string name, string description, Position position) : base(position, LoadOccupiedPositions(baseAttributes))
    {
        LoadBaseAttributes(baseAttributes);

        this.owner = owner;
        this.name = name;
        this.description = description;
    }

    /// Creates the mapSprite GameObject representing this city
    protected override void CreateSprite()
    {
        mapSprite = new GameObject($"City({name})");
        mapSprite.transform.position = position;
        SpriteRenderer spriteRenderer = mapSprite.AddComponent<SpriteRenderer>();
        spriteRenderer.material = new Material(Shader.Find("Shader Graphs/ColorMaskShader"));
    }

    /// Updates the sprite of mapSprite GameObject. (E.g. when the owner changes or when the city is razed)
    public override void UpdateSprite()
    {
        Texture2D texture;
        Texture2D maskTexture;
        if (!razed) {
            texture = _owner.cityTexture;
            maskTexture = _owner.cityMaskTexture;
        } else {
            texture = _owner.razedCityTexture;
            maskTexture = _owner.razedCityMaskTexture;
        }
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0), TileMap.tileSize);
        SpriteRenderer spriteRenderer = mapSprite.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingOrder = 10;
        spriteRenderer.color = _owner.color;
        spriteRenderer.material.SetTexture("_MaskTexture", maskTexture);
    }

    /// Sets the city to an inactive state (unable to produce any units) and removes it from its owner cities
    public void Raze()
    {
        _owner.RemoveCity(this);

        razed = true;
        buildableUnits.Clear();
        producing = false;

        UpdateSprite();

        EventManager.OnCityRazed(this);
    }

    /// Changes the owner of this city to newOwner and gives newOwner a random amount of gold
    /// Removes {destroyedUnitsNumber} units from buildable units and gives newOwner half their purchase cost in gold
    public void Capture(Player newOwner, int destroyedUnitsNumber = 0)
    {
        producing = false;

        owner.RemoveCity(this);
        owner = newOwner;
        newOwner.AddCity(this);

        UpdateSprite();

        List<Unit> destroyedUnits = new List<Unit>();
        int unitGold = 0;

        if (buildableUnits.Count > 1) {
            for (int i = 0; i < destroyedUnitsNumber; i += 1) {
                int destroyedUnitIndex = Random.Range(0, buildableUnits.Count);
                destroyedUnits.Add(buildableUnits[destroyedUnitIndex]);

                unitGold += buildableUnits[destroyedUnitIndex].purchaseCost / 2;

                buildableUnits.Remove(buildableUnits[destroyedUnitIndex]);

                if (buildableUnits.Count == 1) {
                    break;
                }
            }
        }
        else if (buildableUnits.Count == 1 && destroyedUnitsNumber > 0) {
            destroyedUnits.Add(buildableUnits[0]);

            unitGold += buildableUnits[0].purchaseCost / 2;

            buildableUnits.Remove(buildableUnits[0]);
        }
        CityCapturedEventData eventData;
        eventData.lootedGold = Random.Range(Constants.cityCaptureMinGold, Constants.cityCaptureMaxGold + 1);
        eventData.unitGold = unitGold;
        eventData.destroyedUnits = destroyedUnits;

        newOwner.gold += eventData.lootedGold;
        newOwner.gold += eventData.unitGold;

        Debug.Log($"Your armies loot {eventData.lootedGold} gold!");
        if (destroyedUnits.Count > 0) {
            if (destroyedUnitsNumber == 1) {  // todo this isn't pretty
                Debug.Log($"The city of {name} is pillaged for {unitGold}. Ability to produce {destroyedUnits.Count} has been lost and only {buildableUnits.Count} units remain!");
            } else if (destroyedUnitsNumber == 4) {
                Debug.Log($"The city of {name} is sacked for {unitGold}. Ability to produce {destroyedUnits.Count} has been lost and only {buildableUnits.Count} units remain!");
            }
        }

        EventManager.OnCityCaptured(this, eventData);
    }

    /// Adds this city to the tileMap and to its owner cities, and creates the city sprite
    public override void AddToGame()
    {
        base.AddToGame();

        owner?.AddCity(this);
        EventManager.OnCityCreated(this);

        CreateSprite();
        UpdateSprite();
    }

    /// Destroys the city, completely removing it from the game
    public override void Destroy()
    {
        base.Destroy();

        GameObject.Destroy(mapSprite);
        
        if (_owner != null) {
            _owner.RemoveCity(this);
        }

        EventManager.OnCityDestroyed(this);
    }

    /// Adds the given unit to the list of buildable units.
    /// If (replaceIndex < buildableUnits.Count), replaces the unit at replaceIndex. Otherwise adds new unit to the list.
    /// Raises System.ArgumentException if the unit is already buildable or if it doesn't exist in the buyableUnits list.
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
                EventManager.OnUnitBought(this, unit);
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
            productionProgress = Mathf.Min(productionProgress + production, producedUnit.productionCost);
            Debug.Log($"{name} - progress: {productionProgress}/{producedUnit.productionCost}");
            if (productionProgress == producedUnit.productionCost) {
                Position? freePosition = GetFreePosition(1);

                if (freePosition == null) {
                    Debug.LogWarning($"Could not find a free position for produced unit in {this}");
                    // do nothing but keep the productionProgress, so the unit can be spawned on the next turn
                }
                else {
                    Debug.Log("created new Army");
                    Unit newUnit = Unit.FromJObject(ResourceManager.LoadResource(buildableUnits[producedUnitIndex].baseFile));
                    List<Unit> unitList = new List<Unit>();
                    unitList.Add(newUnit);
                    Army producedArmy = new Army(unitList, freePosition.Value, owner);
                    producedArmy.AddToGame();

                    productionProgress = 0;
                }
            }
        }
    }

    /// Returns the first found position in occupiedPositions of tile that has room for unitCount units, or null if no such position exists
    public Position? GetFreePosition(int unitCount)
    {
        foreach (Position occupiedPosition in occupiedPositions) {
            if (gameController.tileMap.GetTile(occupiedPosition).unitCount + unitCount <= Constants.maxUnitsPerTile) {
                return occupiedPosition;
            }
        }
        return null;
    }

    /// Returns a list of all armies that will support this city if it is attacked (all armies within the city's occupied positions)
    public List<Army> GetSupportingArmies()
    {
        List<Army> supportingArmies = new List<Army>();

        TileMap tileMap = GameObject.FindGameObjectWithTag("TileMap").GetComponent<TileMap>();
        foreach (Position occupiedPosition in occupiedPositions) {
            Tile occupiedTile = tileMap.GetTile(occupiedPosition);
            if (occupiedTile.armies != null) {
                supportingArmies.AddRange(occupiedTile.armies);
            }
        }
        return supportingArmies;
    }

    /// Serializes this city into a JObject
    public JObject ToJObject()
    {
        JObject cityJObject = new JObject();

        if (baseFile != null) {
            cityJObject.Add("baseFile", baseFile);
        }

        cityJObject.Add("owner", _owner?.name);
        cityJObject.Add("name", name);
        cityJObject.Add("description", description);
        cityJObject.Add("position", new JArray(position.x, position.y));
        cityJObject.Add("occupiedPositions", new JArray(occupiedPositions.Select(occupiedPosition => new JArray(occupiedPosition.x - position.x, occupiedPosition.y - position.y))));

        cityJObject.Add("pathfinding", pathfinding.ToJObject());

        cityJObject.Add("economy", economy.ToJObject());

        cityJObject.Add("battleStats", battleStats.ToJObject());

        cityJObject.Add("production", production);

        if (tileTypes != null && tileTypes.Count != 0) {
            cityJObject.Add("tileTypes", new JArray(tileTypes));
        }

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
        ResourceManager.Minimize(cityJObject);

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
                newCity.producedUnitIndex = (int)attributes.GetValue("producedUnitIndex");
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

        pathfinding = PathfindingData.FromJObject((JObject)baseAttributes.GetValue("pathfinding"));
        
        economy = EconomyData.FromJObject((JObject)baseAttributes.GetValue("economy"));

        battleStats = BattleStatsData.FromJObject((JObject)baseAttributes.GetValue("battleStats"));

        production = (int)baseAttributes.GetValue("production");

        razed = false;
        if (baseAttributes.ContainsKey("razed")) {
            razed = (bool)baseAttributes.GetValue("razed");
        }
        
        buildableUnits = new List<Unit>();
        buyableUnits = new List<Unit>();

        if (baseAttributes.ContainsKey("tileTypes")) {
            foreach (string tileType in baseAttributes.GetValue("tileTypes")) {
                tileTypes.Add(tileType);
            }
        }

        if (!razed) {
            foreach (string unitPath in baseAttributes.GetValue("buildableUnits")) {
                buildableUnits.Add(Unit.FromJObject(ResourceManager.LoadResource(unitPath)));
            }
            
            foreach (string unitPath in baseAttributes.GetValue("buyableUnits")) {
                buyableUnits.Add(Unit.FromJObject(ResourceManager.LoadResource(unitPath)));
            }
        }
    }

    /// Returns a HashSet of occupied positions loaded from the attributes JObject.
    /// Required for calling the base constructor (MultitileStructure)
    private static HashSet<Position> LoadOccupiedPositions(JObject attributes)
    {
        ResourceManager.ExpandWithBaseFile(attributes);
        HashSet<Position> occupiedPositions = new HashSet<Position>();
        foreach (JToken token in attributes.GetValue("occupiedPositions")) {
            occupiedPositions.Add(new Position((int)token[0], (int)token[1]));
        }
        return occupiedPositions;
    }

    public override string ToString()
    {
        return name + $" at {position} {pathfinding.ToJObject()}";
    }
}
