using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile  // todo change to struct maybe?
{
    public TileData data {get;}
    private TileContents contents;

    public Texture2D texture
    {
        get { return data.texture; }
    }

    public Player owner
    {
        get {
            if (contents.armies != null) {
                return contents.armies[0].owner;
            }
            if (contents.city != null) {
                return contents.city.owner;
            }
            return null;
        }
    }

    public List<Army> armies
    {
        get {
            return contents.armies;
        }
    }

    public City city
    {
        get {
            return contents.city;
        }
    }

    public int moveCost
    {
        get {
            if (contents.city != null) {
                return contents.city.moveCost;
            }
            return data.moveCost;
        }
    }

    public HashSet<string> pathfindingTypes
    {
        get {
            if (contents.city != null) {
                return contents.city.pathfindingTypes;
            }
            return data.pathfindingTypes; 
        }
    }

    public Tile(TileData tileData)
    {
        if (tileData == null) {
            throw new System.NullReferenceException("tileData cannot be null");
        }

        this.data = tileData;
        this.contents = new TileContents();
    }

    /// Adds army to this tile's contents
    public void AddArmy(Army army)
    {
        contents.AddArmy(army);
    }

    /// Removes army from this tile's contents
    public void RemoveArmy(Army army)
    {
        contents.RemoveArmy(army);
    }

    /// Adds city to this tile's contents
    public void AddCity(City city)
    {
        if (contents.city != null) {
            throw new System.ArgumentException($"Cannot add city to this tile. Tile is already occupied by another city");
        }
        contents.city = city;
    }

    /// Removes city from this tile's contents
    public void RemoveCity()
    {
        if (contents.city == null) {
            throw new System.ArgumentException($"Cannot remove city from this tile. Tile has no city");
        }
        contents.city = null;
    }

    public override string ToString()
    {
        if (contents != null) {
            return $"{data}, {contents}";
        } else {
            return $"{data}";
        }
    }
}
