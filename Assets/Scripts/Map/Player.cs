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

    private List<Army> armies;
    private List<City> cities;

    public Player(string jsonPath, string name, Color color)
    {
        this.name = name;
        this.color = color;

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

    public void AddCity(City city)
    {
        cities.Add(city);
    }

    public void StartTurn()
    {
        foreach (Army army in armies) {
            army.StartTurn();
        }
        // do the same thing with cities
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
