using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Structure : MapObject
{
    public Pathfinding pathfinding {get; protected set;}

    public Structure(Position position, Pathfinding pathfinding=null) : base(position)
    {
        this.pathfinding = pathfinding;
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
    }

    /// Destroys this Structure, removing it from the game and tilemap
    public override void Destroy()
    {
        gameController.tileMap.GetTile(position).RemoveStructure();
    }
}
