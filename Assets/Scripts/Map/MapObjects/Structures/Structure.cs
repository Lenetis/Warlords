using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Structure : MapObject
{
    public PathfindingData pathfinding {get; protected set;}

    public HashSet<string> tileTypes {get; protected set;}

    public Structure(Position position, PathfindingData pathfinding=null) : base(position)
    {
        this.pathfinding = pathfinding;
        tileTypes = new HashSet<string>();
    }

    /// Returns true if this Structure can be added to the game and tilemap (if position isn't already occupied by another structure) and false otherwise
    public override bool CanAddToGame()
    {
        if (gameController.tileMap.GetTile(position).structure == null) {
            return true;
        }
        return false;
    }

    /// Adds this Structure to the game and tilemap. Throws System.ArgumentException if it cannot be added
    public override void AddToGame()
    {
        base.AddToGame();

        gameController.tileMap.GetTile(position).AddStructure(this);

        foreach (Tile neighbourTile in gameController.tileMap.GetNeighbouringTiles(position)) {
            neighbourTile?.structure?.UpdateSprite();
        }

        EventManager.OnStructureCreated(this);
    }

    /// Destroys this Structure, removing it from the game and tilemap
    public override void Destroy()
    {
        gameController.tileMap.GetTile(position).RemoveStructure();

        EventManager.OnStructureDestroyed(this);
    }
}
