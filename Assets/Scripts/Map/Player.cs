using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using Newtonsoft.Json.Linq;

public class Player
{
    public string name {get;}
    public Color color {get;}

    public Texture2D cityTexture {get;}
    public Texture2D razedCityTexture {get;}

    public List<Army> armies;
    private List<City> cities;

    public int gold {get; private set;}

    public Player(string jsonPath, string name, Color color)
    {
        this.name = name;
        this.color = color;

        this.gold = 0;

        armies = new List<Army>();
        cities = new List<City>();

        string json = File.ReadAllText(jsonPath);
        JObject jObject = JObject.Parse(json);

        string texturePath = (string)jObject.GetValue("cityTexture");
        byte[] binaryImageData = File.ReadAllBytes(texturePath);
        cityTexture = new Texture2D(0, 0);  // todo for some reason this works, but I *really* don't like this
        cityTexture.LoadImage(binaryImageData);
        cityTexture.filterMode = FilterMode.Point;
        cityTexture.Apply();

        texturePath = (string)jObject.GetValue("razedCityTexture");
        binaryImageData = File.ReadAllBytes(texturePath);
        razedCityTexture = new Texture2D(0, 0);  // todo for some reason this works, but I *really* don't like this
        razedCityTexture.LoadImage(binaryImageData);
        razedCityTexture.filterMode = FilterMode.Point;
        razedCityTexture.Apply();

        GameController gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        gameController.AddPlayer(this);
    }

    public void AddArmy(Army army)
    {
        armies.Add(army);
    }

    public void RemoveArmy(Army army)
    {
        armies.Remove(army);
    }

    public void AddCity(City city)
    {
        cities.Add(city);
    }

    public void RemoveCity(City city)
    {
        cities.Remove(city);
    }

    public void StartTurn()
    {
        foreach (Army army in armies) {
            gold -= army.upkeep;
        }
        foreach (City city in cities) {
            gold += city.income;
        }

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
    public void MoveAll()
    {
        foreach (Army army in armies) {
            army.Move();
        }
    }

    public override string ToString()
    {
        return $"Player(name={name})";
    }
}
