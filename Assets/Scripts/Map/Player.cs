using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using Newtonsoft.Json.Linq;

public class Player
{
    private GameController gameController;
    
    public string name {get;}
    public Color color {get;}

    public Texture2D cityTexture {get;}
    public Texture2D razedCityTexture {get;}

    public List<Army> armies {get;}
    public List<City> cities {get;}

    public int gold {get; private set;}

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

    public Player(JObject attributes, string name, Color color)
    {
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();

        this.name = name;
        this.color = color;

        this.gold = 0;

        armies = new List<Army>();
        cities = new List<City>();

        string texturePath = (string)attributes.GetValue("cityTexture");
        cityTexture = gameController.resourceManager.LoadTexture(texturePath);

        texturePath = (string)attributes.GetValue("razedCityTexture");
        razedCityTexture = gameController.resourceManager.LoadTexture(texturePath);
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
        gold -= upkeep;
        gold += income;

        foreach (Army army in armies) {
            army.StartTurn();
        }

        if (gold >= 0) {
            foreach (City city in cities) {
                city.StartTurn();
            }
        } else {
            gold = 0;
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

    public override string ToString()
    {
        return $"Player(name={name})";
    }
}
