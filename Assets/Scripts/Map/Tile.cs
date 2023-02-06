using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile  // todo change to struct maybe?
{
    public TileData data;
    public TileContents contents {get;}  // todo change this to private as well

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
        this.contents = new TileContents();  // todo add option to initialise tile with TileContents
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
