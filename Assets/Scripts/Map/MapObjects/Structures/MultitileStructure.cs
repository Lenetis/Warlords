using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MultitileStructure : Structure
{
    public HashSet<Position> occupiedPositions {get; private set;}

    public MultitileStructure(Position position, IEnumerable<Position> occupiedPositions) : base(position)
    {
        this.occupiedPositions = new HashSet<Position>();
        foreach(Position occupiedPosition in occupiedPositions) {
            this.occupiedPositions.Add(occupiedPosition + position);
        }
    }

    /// Returns true if this MultitileStructure can be added to the game and tilemap (if none of its occupiedPositions is occupied by another structure) and false otherwise
    public override bool CanAddToGame()
    {
        foreach (Position occupiedPosition in occupiedPositions) {
            if (gameController.tileMap.GetTile(occupiedPosition).structure != null) {
                return false;
            }
        }
        return true;
    }

    /// Adds this MultitileStructure to the game and tilemap (to every occupied tile). Throws System.ArgumentException if it cannot be added
    public override void AddToGame()
    {
        if (!CanAddToGame()) {
            throw new System.ArgumentException($"Cannot add {this} to tilemap at {position}");
        }
        
        foreach (Position occupiedPosition in occupiedPositions) {
            gameController.tileMap.GetTile(occupiedPosition).AddStructure(this);
        }
    }

    /// Destroys this MultitileStructure, removing it from the game and from every occupied tile in tilemap
    public override void Destroy()
    {
        foreach (Position occupiedPosition in occupiedPositions) {
            gameController.tileMap.GetTile(occupiedPosition).RemoveStructure();
        }
    }

    /// Returns true if this MultitileStructure occupies the given position and false otherwise
    public override bool OccupiesPosition(Position position)
    {
        return occupiedPositions.Contains(position);
    }
}
