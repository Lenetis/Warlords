using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

public class Army
{
    public List<Unit> units {get;}

    public Player owner {get;}
    
    private int move;
    private int remainingMove;
    // todo it's probably better to store move and remaining move only in Unit (tile movement bonuses will cause problems)

    public int upkeep
    {
        get {
            int totalUpkeep = 0;
            foreach (Unit unit in units) {
                totalUpkeep += unit.upkeep;
            }
            return totalUpkeep;
        }
    }
    
    public Position position {get; private set;}
    public HashSet<string> pathfindingTypes {get; private set;}
    
    public List<Position> path {get; private set;}

    private bool moving;

    private IEnumerator moveCoroutine;

    private TileMap tileMap;

    public GameObject mapSprite;

    public Army(List<Unit> units, Position position, Player owner)
    {
        this.units = units;
        this.position = position;
        this.owner = owner;

        mapSprite = new GameObject("Army");
        mapSprite.transform.position = position;
        mapSprite.AddComponent<SpriteRenderer>();

        tileMap = GameObject.FindGameObjectWithTag("TileMap").GetComponent<TileMap>();

        tileMap.GetTile(position).contents.AddArmy(this);
        // todo check if this is a legal tile for this army (common pathfinding types, not full, without enemies, etc)

        owner.AddArmy(this);

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
        SpriteRenderer spriteRenderer = mapSprite.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.color = owner.color;  // todo change the recoloring to something more fancy

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
        
        // a non-null path has to start from the army position
        if (path == null || path[0] != position) {
            this.path = null;
        } else {
            this.path = path;
            this.path.RemoveAt(0);  // skip the first element because we don't need to move to where we already are
        }
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
        foreach(Unit unit in units) {
            if (unit.remainingMove <= 0) {
                moving = false;
                yield break;
            }
        }

        while (moving && path.Count > 0) {
            yield return new WaitForSeconds(0.2f);
            Position nextPosition = path[0];
            // todo maybe check if nextPosition is adjacent to the current position?

            Tile currentTile = tileMap.GetTile(position);
            Tile nextTile = tileMap.GetTile(nextPosition);

            foreach(Unit unit in units) {  // todo maybe move this to a separate method?
                if (unit.remainingMove - nextTile.moveCost < 0) {
                    moving = false;
                    break;
                }
            }
            if (!moving) {
                break;
            }
            foreach(Unit unit in units) {
                unit.remainingMove -= nextTile.moveCost;
            }

            currentTile.contents.RemoveArmy(this);
            nextTile.contents.AddArmy(this);

            position = nextPosition;
            mapSprite.transform.position = nextPosition;

            path.RemoveAt(0);

            // todo check if there is room for more units on nextPosition tile
        }
        moving = false;
    }

    public void StartTurn()
    {
        foreach (Unit unit in units) {
            unit.StartTurn();
        }
    }

    public override string ToString()
    {
        return $"[{string.Join(", ", units)}], PathfindingTypes={string.Join(", ", pathfindingTypes)}";
    }
}