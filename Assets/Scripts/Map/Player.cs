using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using Newtonsoft.Json.Linq;

public class Player
{
    public string baseFile {get; private set;}
    
    private static GameController gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
    
    public string name {get;}
    public Color color {get;}

    public bool active {get; private set;}

    public Texture2D cityTexture {get;}
    public Texture2D cityMaskTexture {get;}

    public Texture2D razedCityTexture {get;}
    public Texture2D razedCityMaskTexture {get;}

    private List<string> heroNames;
    private List<Unit> heroUnits;
    private List<Unit> heroAllies;

    public List<Army> armies {get;}
    public List<City> cities {get;}
    public List<Unit> heroes {get;}

    private int _gold;
    public int gold
    {
        get {
            return _gold;
        }
        set {
            if (value < 0) {
                throw new System.ArgumentException("The amount of gold cannot be less than 0");
            } else {
                _gold = value;
            }
        }
    }

    public EconomyData economy
    {
        get {
            int totalUpkeep = 0;
            int totalIncome = 0;
            foreach (Army army in armies) {
                totalUpkeep += army.economy.upkeep;
                totalIncome += army.economy.income;
            }
            foreach (City city in cities) {
                totalUpkeep += city.economy.upkeep;
                totalIncome += city.economy.income;
            }
            return new EconomyData(totalIncome, totalUpkeep);
        }
    }

    // todo change this constructor to use less arguments!
    public Player(string baseFile, string name, Color color, bool active, int gold, Texture2D cityTexture, Texture2D razedCityTexture, Texture2D cityMaskTexture, Texture2D razedCityMaskTexture, List<string> heroNames, List<Unit> heroUnits, List<Unit> heroAllies)
    {
        this.baseFile = baseFile;

        this.name = name;
        this.color = color;

        this.active = active;

        this.gold = gold;

        this.cityTexture = cityTexture;
        this.cityMaskTexture = cityMaskTexture;
        this.razedCityTexture = razedCityTexture;
        this.razedCityMaskTexture = razedCityMaskTexture;

        this.heroNames = heroNames;
        this.heroUnits = heroUnits;
        this.heroAllies = heroAllies;

        armies = new List<Army>();
        cities = new List<City>();
        heroes = new List<Unit>();
    }

    /// Adds army to this player's army list
    public void AddArmy(Army army)
    {
        armies.Add(army);
        foreach (Unit hero in army.heroes) {
            if (!heroes.Contains(hero)) {
                heroes.Add(hero);
            }
        }
    }

    /// Removes army from this player's army list
    public void RemoveArmy(Army army)
    {
        armies.Remove(army);
        foreach (Unit hero in army.heroes) {
            heroes.Remove(hero);
        }
    }

    /// Adds city to this player's city list
    public void AddCity(City city)
    {
        cities.Add(city);
    }

    /// Removes city from this player's city list
    public void RemoveCity(City city)
    {
        cities.Remove(city);
    }

    /// Starts turn - starts turn of all armies and updates gold amount according to army upkeep and city income and, if it's >= 0, starts turn of all cities
    public void StartTurn()
    {
        if (!active) {
            return;
        }

        if (cities.Count == 0) {
            active = false;

            while (armies.Count > 0) {
                armies[0].Destroy();
            }

            Debug.Log($"{name}, {Constants.defeatText[Random.Range(0, Constants.defeatText.Length)]}");

            EventManager.OnPlayerDefeated(this);

            return;
        }

        _gold -= economy.upkeep;
        _gold += economy.income;

        foreach (Army army in armies) {
            army.StartTurn();
        }

        if (_gold >= 0) {
            foreach (City city in cities) {
                city.StartTurn();
            }
        } else {
            _gold = 0;
            // todo add a variable to tell UI if this 0 means just exactly 0, or "city production stopped"
        }

        if (gameController.turn % 3 == 0) {  // todo this 3 should be loaded from a file, or a constant, or something
            TrySpawnHero();
        }
    }

    /// Chooses a random city, hero and cost, and checks if the player can afford it. If yes - raises OnHeroSpawn event
    private void TrySpawnHero()
    {
        City city = cities[Random.Range(0, cities.Count)];
        if (city.GetFreePosition(1) == null) {
            return;
        }

        int heroUnitIndex = Random.Range(0, heroUnits.Count);
        Unit heroUnit = Unit.FromJObject(ResourceManager.LoadResource(heroUnits[heroUnitIndex].baseFile));

        int heroCost = 0;
        int alliesCount = 0;
        if (gameController.turn != 0) {
            int additionalCost = Random.Range(-400, 400);  // todo this 400 should be loaded from a file, or a constant, or something
            heroCost = heroUnit.purchaseCost + additionalCost;

            alliesCount = Random.Range(0, 3 + 1);  // todo this should be loaded from a file/constant/something
        }

        if (heroCost <= _gold) {
            heroUnit.name = heroNames[Random.Range(0, heroNames.Count)];
            
            while (city.GetFreePosition(alliesCount + 1) == null) {
                // if there is no free tile that could fit hero and all the allies, try again, with one less ally
                alliesCount -= 1;
            }

            HeroSpawnEventData eventData;
            eventData.heroCost = heroCost;
            eventData.heroUnit = heroUnit;
            eventData.city = city;
            eventData.alliesCount = alliesCount;

            EventManager.OnHeroSpawn(this, eventData);
        }
    }

    /// Spawns an army containing heroUnit and {alliesCount} randomly chosen allies in the specified city
    public void SpawnHero(Unit heroUnit, City city, int alliesCount)
    {
        if (city.owner != this) {
            throw new System.ArgumentException("Cannot spawn hero in someone else's city.");
        }

        List<Unit> heroArmyUnits = new List<Unit>();
        heroArmyUnits.Add(heroUnit);

        int allyUnitIndex = Random.Range(0, heroAllies.Count);
        for(int i = 0; i < alliesCount; i += 1) {
            Unit ally = Unit.FromJObject(ResourceManager.LoadResource(heroAllies[allyUnitIndex].baseFile));
            ally.economy.upkeep = 0;
            heroArmyUnits.Add(ally);
        }

        Position? freePosition = city.GetFreePosition(heroArmyUnits.Count);
        Army heroArmy = new Army(heroArmyUnits, freePosition.Value, this);
        heroArmy.AddToGame();
    }

    /// Orders all armies to move along their paths
    public void MoveAll()
    {
        foreach (Army army in armies) {
            gameController.StartArmyMove(army);
        }
    }

    /// Serializes this player into a JObject
    public JObject ToJObject()
    {
        JObject playerJObject = new JObject();

        if (baseFile != null) {
            playerJObject.Add("baseFile", baseFile);
        }

        playerJObject.Add("name", name);
        playerJObject.Add("color", ColorUtility.ToHtmlStringRGB(color));

        if (!active) {
            playerJObject.Add("active", active);
        }

        playerJObject.Add("gold", gold);

        playerJObject.Add("heroNames", new JArray(heroNames));
        playerJObject.Add("heroUnits", new JArray(heroUnits.Select(unit => unit.ToJObject().GetValue("baseFile"))));
        playerJObject.Add("heroAllies", new JArray(heroAllies.Select(unit => unit.ToJObject().GetValue("baseFile"))));

        ResourceManager.Minimize(playerJObject);

        return playerJObject;
    }

    /// Creates a new player from JObject
    public static Player FromJObject(JObject attributes)
    {
        ResourceManager.ExpandWithBaseFile(attributes);
        string baseFile = null;
        if (attributes.ContainsKey("baseFile")) {
            baseFile = (string)attributes.GetValue("baseFile");
        }

        string name = (string)attributes.GetValue("name");

        string colorCode = (string)attributes.GetValue("color");
        if (colorCode[0] != '#') {
            colorCode = '#' + colorCode;
        }
        Color color;
        ColorUtility.TryParseHtmlString(colorCode, out color);

        bool active = true;
        if (attributes.ContainsKey("active")) {
            active = (bool)attributes.GetValue("active");
        }

        int gold = (int)attributes.GetValue("gold");

        Texture2D cityTexture = ResourceManager.LoadTexture((string)attributes.GetValue("cityTexture"));
        Texture2D cityMaskTexture = ResourceManager.LoadTexture((string)attributes.GetValue("cityMaskTexture"));
        Texture2D razedCityTexture = ResourceManager.LoadTexture((string)attributes.GetValue("razedCityTexture"));
        Texture2D razedCityMaskTexture = ResourceManager.LoadTexture((string)attributes.GetValue("razedCityMaskTexture"));
        
        List<string> heroNames = new List<string>();
        foreach (string heroName in attributes.GetValue("heroNames")) {
            heroNames.Add(heroName);
        }

        List<Unit> heroUnits = new List<Unit>();
        foreach (string unitPath in attributes.GetValue("heroUnits")) {
            heroUnits.Add(Unit.FromJObject(ResourceManager.LoadResource(unitPath)));
        }

        List<Unit> heroAllies = new List<Unit>();
        foreach (string unitPath in attributes.GetValue("heroAllies")) {
            heroAllies.Add(Unit.FromJObject(ResourceManager.LoadResource(unitPath)));
        }

        return new Player(baseFile, name, color, active, gold, cityTexture, razedCityTexture, cityMaskTexture, razedCityMaskTexture, heroNames, heroUnits, heroAllies);
    }

    public override string ToString()
    {
        return $"Player(name={name})";
    }
}
