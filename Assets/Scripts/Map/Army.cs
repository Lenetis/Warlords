using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Army
{
    public int owner = 0;  // todo
    public List<int> units;  // todo change to List<Unit>
    public int moveSpeed = 4;  // todo
    // todo add something to store path/orders/something like that

    public GameObject mapSprite;

    public Position position;

    public Army(List<int> units, Position position)
    {
        this.units = units;
        this.position = position;
    }

    public void Move(Position newPosition)
    {
        TileMap tileMap = GameObject.FindGameObjectWithTag("TileMap").GetComponent<TileMap>();

        tileMap.GetTile(position).contents.RemoveArmy(this);
        tileMap.GetTile(newPosition).contents.AddArmy(this);

        position = newPosition;
        mapSprite.transform.position = newPosition;
        // todo don't do it like that because if there will be problems is there is no room for more units on target tile
    }

    public override string ToString()
    {
        return string.Join(", ", units);
    }
}