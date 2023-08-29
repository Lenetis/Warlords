using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MapObject
{
    protected static GameController gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
    public Position position {get; protected set;}

    public GameObject mapSprite {get; protected set;}

    public MapObject(Position position)
    {
        this.position = position;
    }

    /// Creates the mapSprite GameObject
    protected abstract void CreateSprite();

    /// Updates the sprite of mapSprite GameObject
    public abstract void UpdateSprite();
    
    /// Returns true if this MapObject can be added to the game and tilemap and false otherwise
    public abstract bool CanAddToGame();

    /// Adds this MapObject to the game and tilemap. Throws System.ArgumentException if it cannot be added
    public virtual void AddToGame()
    {
        if (!CanAddToGame()) {
            throw new System.ArgumentException($"Cannot add {this} to tilemap at {position}");
        }
    }

    /// Returns true if this MapObject occupies the given position and false otherwise
    public virtual bool OccupiesPosition(Position position)
    {
        return position == this.position;
    }

    /// Destroys this MapObject, removing it from the game and tilemap
    public abstract void Destroy();
}
