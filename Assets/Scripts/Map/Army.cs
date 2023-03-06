using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

public class Army : IPlayerMapObject
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
    private IPlayerMapObject attackTarget;

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

    public void RemoveUnit(Unit unit)
    {
        units.Remove(unit);
        if (units.Count == 0) {
            Destroy();
        }
        Recalculate();
    }

    public void AddUnit(Unit unit)
    {
        units.Add(unit);
        Recalculate();
    }

    public void AddUnits(List<Unit> units)
    {
        this.units.AddRange(units);
        Recalculate();
    }

    public void Destroy()
    {
        owner.RemoveArmy(this);
        tileMap.GetTile(position).contents.RemoveArmy(this);

        for (int i = 0; i < mapSprite.transform.childCount; i += 1) {
            mapSprite.transform.GetChild(0).SetParent(null);  // todo this is awful, unit rendering should be moved to a separate script
        }
        GameObject.Destroy(mapSprite);
    }

    // merge this army into other army and destroy this army
    public void Merge(Army otherArmy)
    {
        if (otherArmy.position != position) {
            throw new System.ArgumentException($"Cannot merge armies on different positions ({position} and {otherArmy.position}");
        }
        otherArmy.AddUnits(units);
        Destroy();
    }

    // turn every unit in this army into a separate army
    public void Split()
    {
        while (units.Count > 1) {
            Unit lastUnit = units.Last();
            units.RemoveAt(units.Count - 1);

            List<Unit> newUnitList = new List<Unit>();
            newUnitList.Add(lastUnit);
            Army newArmy = new Army(newUnitList, position, owner);
        }
        Recalculate();
    }

    // turn unit into a separate army
    public void SplitUnit(Unit unit)
    {
        RemoveUnit(unit);

        List<Unit> newUnitList = new List<Unit>();
        newUnitList.Add(unit);
        Army newArmy = new Army(newUnitList, position, owner);
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
        spriteRenderer.sortingOrder = 20;
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
        if (path == null || path.Count <= 1 || path[0] != position) {
            this.path = null;
        } else {
            this.path = path;
            this.path.RemoveAt(0);  // skip the first element because we don't need to move to where we already are

            attackTarget = null;
            Tile targetTile = tileMap.GetTile(path.Last());
            if (targetTile.owner != null && targetTile.owner != owner) {
                if (targetTile.contents.city != null) {  // todo this looks a bit spaghetti, but I'm not sure what else to do
                    attackTarget = targetTile.contents.city;
                } else {
                    attackTarget = targetTile.contents.armies[0];  // todo make sure that armies[0] is the one with a visible sprite
                }
            }
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

            if (nextTile.owner == null || nextTile.owner == owner) {
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
            } else {
                if (path.Count == 1) {
                    Battle battle = new Battle(this, attackTarget);

                    moving = false;
                    path = null;
                    attackTarget = null;

                } else {
                    // todo maybe find a different path instead of cancelling the move?
                    moving = false;
                    path = null;
                }
            }
        }
        moving = false;
    }

    public void StartTurn()
    {
        if (attackTarget != null && path != null && !attackTarget.OccupiesPosition(path.Last())) {
            // todo if fog of war is added, we need to check if attackTarget is visible
            SetPath(tileMap.FindPath(position, attackTarget.position, this));
 
            if (path == null) {
                return;
            }
        }
        
        foreach (Unit unit in units) {
            unit.StartTurn();
        }
    }

    public List<Army> GetSupportingArmies()
    {
        List<Army> supportingArmies;
        Tile occupiedTile = tileMap.GetTile(position);
        if (occupiedTile.contents.city != null) {
            supportingArmies = occupiedTile.contents.city.GetSupportingArmies();
        } else {
            supportingArmies = occupiedTile.contents.armies;
        }
        return supportingArmies;
    }

    public bool OccupiesPosition(Position position) {
        return this.position == position;
    }

    public override string ToString()
    {
        return $"[{string.Join(", ", units)}], PathfindingTypes={string.Join(", ", pathfindingTypes)}";
    }
}