using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using Newtonsoft.Json.Linq;

public class Army : IPlayerMapObject
{
    private static GameController gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();

    public List<Unit> units {get;}

    public Player owner {get;}
    
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
    
    public Position position {get; set;}
    public HashSet<string> pathfindingTypes {get; private set;}
    
    public List<Position> path {get; private set;}
    private IPlayerMapObject attackTarget;

    public GameObject mapSprite;  // todo to remove

    public Army(List<Unit> units, Position position, Player owner)
    {        
        this.units = units;
        this.position = position;
        this.owner = owner;

        mapSprite = new GameObject("Army");
        mapSprite.transform.position = position;
        SpriteRenderer spriteRenderer = mapSprite.AddComponent<SpriteRenderer>();
        spriteRenderer.material = new Material(Shader.Find("Shader Graphs/ColorMaskShader"));

        Recalculate();

        AddToGame();
    }

    /// Removes unit from army
    public void RemoveUnit(Unit unit)
    {
        units.Remove(unit);
        if (units.Count == 0) {
            Destroy();
        } else {
            Recalculate();
        }
    }

    /// Adds unit to army
    public void AddUnit(Unit unit)
    {
        units.Add(unit);
        Recalculate();
    }

    /// Adds a list of units to army
    public void AddUnits(List<Unit> units)
    {
        this.units.AddRange(units);
        Recalculate();
    }

    /// Adds this army to the tileMap and to its owner armies
    private void AddToGame()
    {
        // todo, this could also be done by event handlers in tileMap and owner. Is it a good idea? todo?
        gameController.tileMap.GetTile(position).AddArmy(this);
        owner.AddArmy(this);

        EventManager.OnArmyCreated(this);
    }

    /// Destroys the army - removes this army (and all its units) from the game
    public void Destroy()
    {
        owner.RemoveArmy(this);
        gameController.tileMap.GetTile(position).RemoveArmy(this);

        // todo remove this!!
        for (int i = 0; i < mapSprite.transform.childCount; i += 1) {
            mapSprite.transform.GetChild(0).SetParent(null);  // todo this is awful, unit rendering should be moved to a separate script
        }
        GameObject.Destroy(mapSprite);

        EventManager.OnArmyDestroyed(this);
    }

    /// Merges this army into other army and destroys this army
    public void Merge(Army otherArmy)
    {
        if (otherArmy.position != position) {
            throw new System.ArgumentException($"Cannot merge armies on different positions ({position} and {otherArmy.position}");
        }
        otherArmy.AddUnits(units);
        Destroy();
    }

    /// Turns every unit in this army into a separate army
    public void Split()
    {
        while (units.Count > 1) {
            Unit lastUnit = units.Last();
            units.RemoveAt(units.Count - 1);

            List<Unit> newUnitList = new List<Unit>();
            newUnitList.Add(lastUnit);
            new Army(newUnitList, position, owner);
        }
        Recalculate();
    }

    /// Splits unit from this army and turns it into a separate army
    public void SplitUnit(Unit unit)
    {
        RemoveUnit(unit);

        List<Unit> newUnitList = new List<Unit>();
        newUnitList.Add(unit);
        new Army(newUnitList, position, owner);
    }

    // todo change name to something more descriptive 
    // (function is called everytime the unit list changes, to refresh the army move, remaining move and sprite)
    private void Recalculate()
    {
        units.Sort((unit1, unit2) => unit1.strength.CompareTo(unit2.strength));
        // units are sorted from weakest to strongest for now (todo change this when implementing custom unit orders)

        Unit strongestUnit = units[0];
        foreach (Unit unit in units.Skip(1)) {
            if (unit.strength > strongestUnit.strength) {
                strongestUnit = unit;
            }
        }

        Texture2D texture = strongestUnit.texture;
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0), 32);
        SpriteRenderer spriteRenderer = mapSprite.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingOrder = 20;
        spriteRenderer.color = owner.color;
        spriteRenderer.material.SetTexture("_MaskTexture", strongestUnit.maskTexture);

        pathfindingTypes = new HashSet<string>(units[0].pathfindingTypes);
        foreach (Unit unit in units.Skip(1)) {
            pathfindingTypes.IntersectWith(unit.pathfindingTypes);
        }
        if (pathfindingTypes.Count == 0) {
            throw new System.ArgumentException($"No common pathfinding type exists in army {string.Join(", ", units)}");
        }
    }

    /// Sets army path to a new value. If the last step of the new path is an enemy unit, that unit becomes the army's attackTarget
    public void SetPath(List<Position> path)  // todo shouldn't this be in the path setter instead of a new method?
    {        
        // a non-null path has to start from the army position
        if (path == null || path.Count <= 1 || path[0] != position) {
            this.path = null;
        } else {
            this.path = path;
            this.path.RemoveAt(0);  // skip the first element because we don't need to move to where we already are

            attackTarget = null;
            Tile targetTile = gameController.tileMap.GetTile(path.Last());
            if (targetTile.owner != null && targetTile.owner != owner) {
                if (targetTile.city != null) {  // todo this looks a bit spaghetti, but I'm not sure what else to do
                    attackTarget = targetTile.city;
                } else {
                    attackTarget = targetTile.armies[0];  // todo make sure that armies[0] is the one with a visible sprite
                }
            }
        }
    }

    /// Moves army by one step on its path. Returns true if the move succeeded and false otherwise (i.e. if there is no path or army doesn't have enough move points)
    public bool MoveOneStep()
    {
        if (path == null || path.Count == 0) {
            return false;
        }

        Position nextPosition = path[0];
        if (!gameController.tileMap.CanMoveInOneStep(position, nextPosition, pathfindingTypes)) {
            Debug.LogError($"Cannot move army from {position} to {nextPosition}. Either the positions are not adjacent, or there is no matching pathfinding type.");
            return false;
        }

        Tile nextTile = gameController.tileMap.GetTile(nextPosition);
        foreach (Unit unit in units) {
            if (unit.remainingMove - nextTile.moveCost < 0) {
                return false;
            }
        }
        
        if (attackTarget != null && attackTarget.OccupiesPosition(nextPosition)) {
            new Battle(this, attackTarget);  // this line looks super cursed, but everything communicates nicely through events, so it's fine. todo?

            path = null;
            attackTarget = null;
            
            return false;
        }
        else {
            if (nextTile.owner == null || nextTile.owner == owner) {
                //todo check if there is room for more units on the target tile

                Tile currentTile = gameController.tileMap.GetTile(position);

                ArmyMovedEventData eventData;
                eventData.startPosition = position;
                eventData.endPosition = nextPosition;

                currentTile.RemoveArmy(this);
                nextTile.AddArmy(this);
                position = nextPosition;

                mapSprite.transform.position = nextPosition;  // todo this probably shouldn't be here
                foreach (Unit unit in units) {
                    unit.remainingMove -= nextTile.moveCost;
                }
                path.RemoveAt(0);

                EventManager.OnArmyMoved(this, eventData);

                return true;
            }
            else {
                // todo maybe find a different path instead of cancelling the move?
                path = null;
                return false;
            }
        }   
    }

    /// Starts turn (resets move points) of every unit in this army and updates path if attackTarget has moved
    public void StartTurn()
    {
        foreach (Unit unit in units) {
            unit.StartTurn();
        }

        if (attackTarget != null && path != null && !attackTarget.OccupiesPosition(path.Last())) {
            // todo if fog of war is added, we need to check if attackTarget is visible
            SetPath(gameController.tileMap.FindPath(position, attackTarget.position, this));
 
            if (path == null) {
                return;
            }
        }
    }

    /// Returns a list of all armies that will support this army if it is attacked (a.k.a. all armies on the same tile and in the same city as this army)
    public List<Army> GetSupportingArmies()
    {
        List<Army> supportingArmies;
        Tile occupiedTile = gameController.tileMap.GetTile(position);
        if (occupiedTile.city != null) {
            supportingArmies = occupiedTile.city.GetSupportingArmies();
        } else {
            supportingArmies = occupiedTile.armies;
        }
        return supportingArmies;
    }

    /// Returns true if the position is occupied by this army and false otherwise
    public bool OccupiesPosition(Position position) {
        return this.position == position;
    }

    /// Serializes this army into a JObject
    public JObject ToJObject()
    {
        JObject armyJObject = new JObject();
        armyJObject.Add("owner", owner?.name);
        armyJObject.Add("position", new JArray(position.x, position.y));

        if (path != null) {
            armyJObject.Add("path", new JArray(path.Select(position => new JArray(position.x, position.y))));
        }
        
        armyJObject.Add("units", new JArray(units.Select(unit => unit.ToJObject())));

        return armyJObject;
    }

    /// Creates a new army from JObject
    public static Army FromJObject(JObject attributes)
    {
        ResourceManager.ExpandWithBaseFile(attributes);

        List<Unit> units = new List<Unit>();
        foreach (JObject unitJObject in attributes.GetValue("units")) {
            units.Add(Unit.FromJObject(unitJObject));
        }

        Position position = new Position((int)attributes.GetValue("position")[0], (int)attributes.GetValue("position")[1]);

        Player owner = gameController.GetPlayerByName((string)attributes.GetValue("owner"));

        return new Army(units, position, owner);
    }

    public override string ToString()
    {
        return $"[{string.Join(", ", units)}], PathfindingTypes={string.Join(", ", pathfindingTypes)}";
    }
}