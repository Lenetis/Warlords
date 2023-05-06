using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json.Linq;

public class Player
{
    public string baseFile {get; private set;}
    
    private static GameController gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
    
    public string name {get;}
    public Color color {get;}

    public Texture2D cityTexture {get;}
    public Texture2D razedCityTexture {get;}

    public List<Army> armies {get;}
    public List<City> cities {get;}

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

    public int income 
    {
        get {
            int income = 0;
            foreach (City city in cities) {
               income += city.income;
            }
            return income;
        }
    }

    public int upkeep 
    {
        get {
            int upkeep = 0;
            foreach (Army army in armies) {
                upkeep += army.upkeep;
            }
            return upkeep;
        }
    }

    // todo maybe change this constructor to use less arguments
    public Player(string baseFile, string name, Color color, int gold, Texture2D cityTexture, Texture2D razedCityTexture)
    {
        this.baseFile = baseFile;

        this.name = name;
        this.color = color;

        this.gold = gold;

        this.cityTexture = cityTexture;
        this.razedCityTexture = razedCityTexture;

        armies = new List<Army>();
        cities = new List<City>();
    }

    /// Adds army to this player's army list
    public void AddArmy(Army army)
    {
        armies.Add(army);
    }

    /// Removes army from this player's army list
    public void RemoveArmy(Army army)
    {
        armies.Remove(army);
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
        _gold -= upkeep;
        _gold += income;

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
        playerJObject.Add("gold", gold);

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

        int gold = (int)attributes.GetValue("gold");

        Texture2D cityTexture = ResourceManager.LoadTexture((string)attributes.GetValue("cityTexture"));
        Texture2D razedCityTexture = ResourceManager.LoadTexture((string)attributes.GetValue("razedCityTexture"));

        return new Player(baseFile, name, color, gold, cityTexture, razedCityTexture);
    }

    public override string ToString()
    {
        return $"Player(name={name})";
    }
}
