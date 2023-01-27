using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using Newtonsoft.Json.Linq;

public class City
{
    public Player owner {get; private set;}

    public Position position {get;}
    private List<Position> occupiedPositions;

    public int moveCost {get;}
    public HashSet<string> pathfindingTypes {get;}

    private GameObject mapSprite;

    public bool razed {get; private set;}

    public City(string jsonPath, Position position, Player owner)
    {
        this.position = position;
        this.owner = owner;
        razed = false;

        mapSprite = new GameObject("Army");
        mapSprite.transform.position = position;
        mapSprite.AddComponent<SpriteRenderer>();

        string json = File.ReadAllText(jsonPath);
        JObject jObject = JObject.Parse(json);

        occupiedPositions = new List<Position>();
        foreach (JToken jsonPosition in jObject.GetValue("occupiedPositions")) {
            occupiedPositions.Add(new Position((int)jsonPosition[0], (int)jsonPosition[1]));
        }

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
        spriteRenderer.color = owner.color;  // todo change the recoloring to something more fancy

        if (razed) {
            owner = null;
        }
    }

    public void Raze()
    {
        razed = true;
        Recalculate();
    }

    public override string ToString()
    {
        return "City";
    }
}
