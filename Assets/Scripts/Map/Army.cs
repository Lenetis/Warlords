using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

public class Army
{
    public int owner = 0;  // todo

    public List<Unit> units {get;}
    
    private int move;
    private int remainingMove;
    // todo it's probably better to store move and remaining move only in Unit (tile movement bonuses will cause problems)
    
    public Position position {get; private set;}
    public HashSet<string> pathfindingTypes {get; private set;}
    
    private List<Position> path;

    private bool moving;

    private IEnumerator moveCoroutine;

    private TileMap tileMap;
    
    private GameObject mapSprite;

    public Army(List<Unit> units, Position position)
    {
        this.units = units;
        this.position = position;

        mapSprite = new GameObject("Army");
        mapSprite.transform.position = position;
        mapSprite.AddComponent<SpriteRenderer>();

        tileMap = GameObject.FindGameObjectWithTag("TileMap").GetComponent<TileMap>();

        Recalculate();
    }

    // todo change name to something more descriptive 
    // (function is called everytime the unit list changes, to refresh the army move, remaining move and sprite)
    private void Recalculate()
    {
        units.Sort((unit1, unit2) => unit1.strength.CompareTo(unit2.strength));
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

        pathfindingTypes = new HashSet<string>(units[0].pathfindingTypes);
        foreach (Unit unit in units.Skip(1)) {
            pathfindingTypes.IntersectWith(unit.pathfindingTypes);
        }
        if (pathfindingTypes.Count == 0) {
            throw new System.ArgumentException($"No common pathfinding type exists in army {string.Join(", ", units)}");
            // todo check this in the merging armies method (but don't remove this exception, it will still be useful in loading save files for example)
        }
    }

    public void SetPath(List<Position> path)
    {
        if (moving) {
            tileMap.StopCoroutine(moveCoroutine);
            moving = false;
        }
        this.path = path;
    }

    public void Move()
    {
        if (!moving && path != null) {
            moving = true;
            moveCoroutine = MoveCoroutine();
            tileMap.StartCoroutine(moveCoroutine);
        }
    }
    
    private IEnumerator MoveCoroutine()
    {
        while (moving && path.Count > 0 && true) {  // todo replace "true" with check if all units have enough movement points
            yield return new WaitForSeconds(0.2f);
            Position nextPosition = path[0];
            // todo maybe check if nextPosition is adjacent to the current position?
            tileMap.GetTile(position).contents.RemoveArmy(this);
            tileMap.GetTile(nextPosition).contents.AddArmy(this);
            position = nextPosition;
            mapSprite.transform.position = nextPosition;
            path.RemoveAt(0);

            // todo check if there is room for more units on nextPosition tile
        }
        moving = false;
    }

    public override string ToString()
    {
        return $"[{string.Join(", ", units)}], PathfindingTypes={string.Join(", ", pathfindingTypes)}";
    }
}