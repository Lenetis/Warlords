using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile  // todo change to struct maybe?
{
    public TileData data {get; set;}
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
            if (contents.structure as IOwnableMapObject != null) {
                return ((IOwnableMapObject)contents.structure).owner;
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

    public int unitCount
    {
        get {
            if (contents.armies == null) {
                return 0;
            }
            else {
                int totalCount = 0;
                foreach (Army army in contents.armies) {
                    totalCount += army.units.Count;
                }
                return totalCount;
            }            
        }
    }

    public Structure structure
    {
        get {
            return contents.structure;
        }
    }

    public List<Item> items
    {
        get {
            return contents.items;
        }
    }

    public int moveCost
    {
        get {
            if (contents.structure != null && contents.structure.pathfinding != null) {
                return contents.structure.pathfinding.moveCost;
            }
            return data.moveCost;
        }
    }

    public HashSet<string> pathfindingTypes
    {
        get {
            if (contents.structure?.pathfinding != null) {
                return contents.structure.pathfinding.pathfindingTypes;
            }
            return data.pathfindingTypes;
        }
    }

    public HashSet<string> tileTypes
    {
        get {
            if (contents.structure?.tileTypes != null) {
                HashSet<string> mergedTileTypes = new HashSet<string>();
                mergedTileTypes.UnionWith(data.tileTypes);
                mergedTileTypes.UnionWith(contents.structure.tileTypes);
                return mergedTileTypes;
            }
            return data.tileTypes;
        }
    }

    public PathfindingTransition transition
    {
        get {
            return contents.structure?.pathfinding?.transition;
        }
    }

    public PathfindingTransition transitionReturn
    {
        get {
            return contents.structure?.pathfinding?.transitionReturn;
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

    /// Returns texture depending on the neighbouring tiles. If no neighbour condition has been matched, returns default texture
    public Texture2D GetRelativeTexture(Neighbours<Tile> neighbouringTiles)
    {
        return data.GetRelativeTexture(neighbouringTiles);
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

    /// Adds item to this tile's contents
    public void AddItem(Item item)
    {
        contents.AddItem(item);
    }

    /// Removes item from this tile's contents
    public void RemoveItem(Item item)
    {
        contents.RemoveItem(item);
    }

    /// Adds city to this tile's contents
    public void AddStructure(Structure structure)
    {
        if (contents.structure != null) {
            throw new System.ArgumentException($"Cannot add structure to this tile. Tile is already occupied by another structure");
        }
        contents.structure = structure;
    }

    /// Removes city from this tile's contents
    public void RemoveStructure()
    {
        if (contents.structure == null) {
            throw new System.ArgumentException($"Cannot remove structure from this tile. Tile has no structure");
        }
        contents.structure = null;
    }

    /// Removes everything from this tile's contents
    public void Clear()
    {
        if (contents.structure != null) {
            contents.structure.Destroy();
        }
        while (contents.armies != null) {
            contents.armies[0].Destroy();
        }
        while (contents.items != null) {
            contents.items[0].Destroy();
        }
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
