using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile
{
    public TileData tileData;
    public List<string> tileObjects;  // todo

    public Tile(TileData tileData) {
        if (tileData == null) {
            throw new System.NullReferenceException("tileData cannot be null");
        }

        Debug.Log(tileData);
        this.tileData = tileData;
        this.tileObjects = new List<string>();  // todo add option to initialise tile with a list of tile objects
    }

    public override string ToString()
    {
        return $"{tileData}, objects = {/*string.Join(",", tileObjects)*/0}";
    }
}
