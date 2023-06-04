using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using Newtonsoft.Json.Linq;

public class Army : MapObject /* todo? maybe add MovableMapObject class? */, IOwnableMapObject
{
    public List<Unit> units {get;}

    public Player owner {get;}
    
    public EconomyData economy
    {
        get {
            int totalUpkeep = 0;
            int totalIncome = 0;
            foreach (Unit unit in units) {
                totalUpkeep += unit.economy.upkeep;
                totalIncome += unit.economy.income;
            }
            return new EconomyData(totalIncome, totalUpkeep);
        }
    }

    public HashSet<string> pathfindingTypes
    {
        get {
            if (transitionPathfindingTypes != null) {
                return transitionPathfindingTypes;
            }
            return basePathfindingTypes;
        }
    }
    public bool isTransitioned
    {
        get {
            return transitionPathfindingTypes != null;
        }
    }
    public HashSet<string> basePathfindingTypes {get; private set;}
    public HashSet<string> transitionPathfindingTypes {get; private set;}
    
    public List<Position> path {get; private set;}
    private IOwnableMapObject attackTarget;

    public Army(List<Unit> units, Position position, Player owner) : base(position)
    {        
        this.units = units;
        this.owner = owner;
        
        UpdatePathfindingTypes();
        SortUnits();
    }

    /// Creates the mapSprite GameObject representing this army
    protected override void CreateSprite()
    {
        mapSprite = new GameObject("Army");
        mapSprite.transform.position = position;
        SpriteRenderer spriteRenderer = mapSprite.AddComponent<SpriteRenderer>();
        spriteRenderer.material = new Material(Shader.Find("Shader Graphs/ColorMaskShader"));
    }

    /// Updates the sprite of mapSprite GameObject. (E.g. when the unit list changes)
    public override void UpdateSprite()
    {
        Unit strongestUnit = units[^1];
        // (assuming the list is already sorted and the stronged unit is always the last)

        Texture2D texture = strongestUnit.texture;
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0), 32);
        SpriteRenderer spriteRenderer = mapSprite.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingOrder = 20;
        spriteRenderer.color = owner.color;
        spriteRenderer.material.SetTexture("_MaskTexture", strongestUnit.maskTexture);

        if (isTransitioned) {
            if (mapSprite.transform.Find("Boat") == null) {
                GameObject boatMapSprite = new GameObject("Boat");
                boatMapSprite.transform.SetParent(mapSprite.transform);
                boatMapSprite.transform.localPosition = Vector3.zero;
                SpriteRenderer boatSpriteRenderer = boatMapSprite.AddComponent<SpriteRenderer>();
                //boatSpriteRenderer.material = new Material(Shader.Find("Shader Graphs/ColorMaskShader"));
                Texture2D boatTexture = ResourceManager.LoadTexture("Assets/Resources/Units/Boat.png");  // todo, this path probably shouldn't be hardcoded
                Sprite boatSprite = Sprite.Create(boatTexture, new Rect(0, 0, boatTexture.width, boatTexture.height), new Vector2(0, 0), 32);
                boatSpriteRenderer.sprite = boatSprite;
                boatSpriteRenderer.sortingOrder = 30;
                //boatSpriteRenderer.color = owner.color;
                //boatSpriteRenderer.material.SetTexture("_MaskTexture", strongestUnit.maskTexture);
            }
        }
        else {
            if (mapSprite.transform.Find("Boat") != null) {
                GameObject.Destroy(mapSprite.transform.Find("Boat").gameObject);
            }
        }
    }

    /// Sorts the unit list from weakest to strongest and alphabetically in case of the same strength (todo change this when implementing custom unit orders)
    private void SortUnits()
    {
        units.Sort((unit1, unit2) => {
            int result = unit1.battleStats.strength.CompareTo(unit2.battleStats.strength);
            if (result == 0) {
                result = unit1.name.CompareTo(unit2.name);
            }
            return result;
        });
    }

    /// Updates the pathfindingTypes HashSet to include only the common pathfinding types from the unit list.
    /// Throws ArgumentException if no common pathfinding type exists.
    private void UpdatePathfindingTypes()
    {
        basePathfindingTypes = new HashSet<string>(units[0].basePathfinder.pathfindingTypes);

        foreach (Unit unit in units.Skip(1)) {
            basePathfindingTypes.IntersectWith(unit.basePathfinder.pathfindingTypes);
        }
        if (basePathfindingTypes.Count == 0) {
            throw new System.ArgumentException($"No common pathfinding type exists in army {string.Join(", ", units)}");
        }

        // assuming here that either all units are transitioned or none. This will not always be the case. todo
        if (units[0].isTransitioned) {
            transitionPathfindingTypes = units[0].transitionPathfinder.pathfindingTypes;
        }
        else {
            transitionPathfindingTypes = null;
        }
    }

    /// Removes unit from army
    public void RemoveUnit(Unit unit)
    {
        units.Remove(unit);
        if (units.Count == 0) {
            Destroy();
        } else {
            UpdatePathfindingTypes();
            UpdateSprite();
        }
    }

    /// Adds unit to army
    public void AddUnit(Unit unit)
    {
        units.Add(unit);

        UpdatePathfindingTypes();
        SortUnits();

        UpdateSprite();
    }

    /// Adds a list of units to army
    public void AddUnits(List<Unit> units)
    {
        this.units.AddRange(units);

        UpdatePathfindingTypes();
        SortUnits();

        UpdateSprite();
    }

    /// Should check if there is still place for (units.Count) units on the occupied tile. Currently always returns true. TODO
    public override bool CanAddToGame()
    {
        return true;  // todo check if there is space for this army on this position
    }

    /// Adds this army to the tileMap and to its owner armies, and creates the army sprite
    public override void AddToGame()
    {
        base.AddToGame();

        gameController.tileMap.GetTile(position).AddArmy(this);
        owner.AddArmy(this);

        EventManager.OnArmyCreated(this);

        CreateSprite();
        UpdateSprite();
    }

    /// Destroys the army - removes this army (and all its units) from the game
    public override void Destroy()
    {
        owner.RemoveArmy(this);
        gameController.tileMap.GetTile(position).RemoveArmy(this);

        // todo remove this!!
        while (mapSprite.transform.childCount > 0) {
            if (mapSprite.transform.GetChild(0).name == "Boat") {
                GameObject.Destroy(mapSprite.transform.GetChild(0).gameObject);
            }
            mapSprite.transform.GetChild(0).SetParent(null);
            // todo this whole loop is awful
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
            Army newArmy = new Army(newUnitList, position, owner);
            newArmy.AddToGame();
        }
        UpdatePathfindingTypes();
        SortUnits();

        UpdateSprite();
    }

    /// Splits unit from this army and turns it into a separate army
    public void SplitUnit(Unit unit)
    {
        RemoveUnit(unit);

        List<Unit> newUnitList = new List<Unit>();
        newUnitList.Add(unit);
        Army newArmy = new Army(newUnitList, position, owner);
        newArmy.AddToGame();
    }

    /// Sets army path to a new value. If the last step of the new path is an enemy unit, that unit becomes the army's attackTarget
    public void SetPath(List<Position> path)  // todo shouldn't this be in the path setter instead of a new method?
    {
        if (path == null || path.Count == 0) {
            this.path = null;
        } else {
            this.path = path;

            attackTarget = null;
            Tile targetTile = gameController.tileMap.GetTile(path.Last());
            if (targetTile.owner != null && targetTile.owner != owner) {
                if (targetTile.structure as IOwnableMapObject != null) {  // todo this looks a bit spaghetti, but I'm not sure what else to do
                    attackTarget = (IOwnableMapObject)targetTile.structure;
                } else {
                    attackTarget = targetTile.armies[0];  // todo make sure that armies[0] is the one with a visible sprite
                }
            }
        }
    }

    /// Moves army by one step on its path. Returns true if the move succeeded and false otherwise (e.g. if there is no path or army doesn't have enough move points)
    public bool MoveOneStep()
    {
        if (path == null || path.Count == 0) {
            return false;
        }

        Position nextPosition = path[0];
        if (!gameController.tileMap.Adjacent(position, nextPosition)) {
            Debug.LogError($"Cannot move army from {position} to {nextPosition}. Positions are not adjacent.");
            return false;
        }

        Tile currentTile = gameController.tileMap.GetTile(position);
        Tile nextTile = gameController.tileMap.GetTile(nextPosition);        

        bool canTransition = false;
        if (!nextTile.pathfindingTypes.Overlaps(pathfindingTypes)) {
            if (!isTransitioned && currentTile.transition != null) {
                if (currentTile.transition.from.IsSubsetOf(pathfindingTypes)) {
                    canTransition = true;
                }
            }
            else if (isTransitioned && currentTile.transitionReturn != null) {
                if (currentTile.transitionReturn.from.IsSubsetOf(pathfindingTypes)) {
                    canTransition = true;
                }
            }

            if (!canTransition) {
                Debug.LogError($"Cannot move army from {position} to {nextPosition}. There are no matching pathfindingTypes and no pathfinding transitions are possible.");
                return false;
            }
        }

        foreach (Unit unit in units) {
            if (unit.pathfinder.remainingMove - nextTile.moveCost < 0) {
                return false;
            }
        }
        
        if (attackTarget != null && ((MapObject)attackTarget).OccupiesPosition(nextPosition)) {
            new Battle(this, attackTarget);  // this line looks super cursed, but everything communicates nicely through events, so it's fine. todo?

            path = null;
            attackTarget = null;
            
            return false;
        }
        else {
            if (nextTile.owner == null || nextTile.owner == owner) {
                //todo check if there is room for more units on the target tile

                ArmyMovedEventData eventData;
                eventData.startPosition = position;
                eventData.endPosition = nextPosition;

                currentTile.RemoveArmy(this);
                nextTile.AddArmy(this);
                position = nextPosition;

                mapSprite.transform.position = nextPosition;

                if (canTransition) {
                    if (!isTransitioned) {
                        foreach (Unit unit in units) {
                            unit.Transition(currentTile.transition);
                        }
                    }
                    else {
                        foreach (Unit unit in units) {
                            unit.TransitionReturn();
                        }
                    }
                    UpdatePathfindingTypes();
                    UpdateSprite();
                }
                else {
                    foreach (Unit unit in units) {
                        unit.pathfinder.remainingMove -= nextTile.moveCost;
                    }
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

        if (attackTarget != null && path != null && !((MapObject)attackTarget).OccupiesPosition(path.Last())) {
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
        if (occupiedTile.structure as IOwnableMapObject != null) {
            supportingArmies = ((IOwnableMapObject)occupiedTile.structure).GetSupportingArmies();
        } else {
            supportingArmies = occupiedTile.armies;
        }
        return supportingArmies;
    }

    /// Serializes this army into a JObject
    public JObject ToJObject()
    {
        JObject armyJObject = new JObject();
        armyJObject.Add("owner", owner?.name);
        armyJObject.Add("position", new JArray(position.x, position.y));

        if (path != null && path.Count > 0) {
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
        return $"[{string.Join(", ", units)}], BasePathfindingTypes={string.Join(", ", basePathfindingTypes)},"
             + $"TransitionPathfindingTypes={(isTransitioned == true ? string.Join(", ", transitionPathfindingTypes) : "null")}";
    }
}