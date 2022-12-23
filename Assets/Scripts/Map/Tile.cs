using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile  // todo change to struct maybe?
{
    public TileData data;
    public TileContents contents;

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
