using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

public class Army
{
    public int owner = 0;  // todo
    public List<Unit> units;
    private int move;  // todo
    private int remainingMove;
    // todo add something to store path/orders/something like that
    public Position position;

    private GameObject mapSprite;

    public Army(List<Unit> units, Position position)
    {
        this.units = units;
        this.position = position;

        mapSprite = new GameObject("Army");
        mapSprite.transform.position = position;
        mapSprite.AddComponent<SpriteRenderer>();

        Recalculate();
    }


    // todo change name to something more descriptive 
    // (function is called everytime the unit list changes, to refresh the army move, remaining move and sprite)
    private void Recalculate()
    {
        this.units.Sort((o1, o2) => o1.strength.CompareTo(o2.strength));
        // units are sorted from weakest to strongest for now (todo change this when implementing custom unit orders)

        Unit strongestUnit = units[0];
        Unit leastRemainingMoveUnit = units[0];
        Unit leastMoveUnit = units[0];
        foreach (Unit unit in units.Skip(1)) {
            if (unit.strength > strongestUnit.strength) {
                strongestUnit = unit;
            }
            if (unit.remainingMove < leastRemainingMoveUnit.remainingMove) {
                leastRemainingMoveUnit = unit;
            }
            if (unit.move < leastMoveUnit.move) {
                leastMoveUnit = unit;
            }
        }
        move = leastMoveUnit.move;
        remainingMove = leastMoveUnit.remainingMove;

        Texture2D texture = strongestUnit.texture;
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0), 32);
        mapSprite.GetComponent<SpriteRenderer>().sprite = sprite;
    }

    public void Move(Position newPosition)
    {
        TileMap tileMap = GameObject.FindGameObjectWithTag("TileMap").GetComponent<TileMap>();

        tileMap.GetTile(position).contents.RemoveArmy(this);
        tileMap.GetTile(newPosition).contents.AddArmy(this);

        position = newPosition;
        mapSprite.transform.position = newPosition;
        // todo don't do it like that because there will be problems if there is no room for more units on target tile
    }

    public override string ToString()
    {
        return $"[{string.Join(", ", units)}]";
    }
}