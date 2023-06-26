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

    /// Returns a list of all hero units in this army (may be empty)
    public List<Unit> heroes
    {
        get {
            List<Unit> heroesList = new List<Unit>();
            foreach (Unit unit in units) {
                if (unit.isHero) {
                    heroesList.Add(unit);
                }
            }
            return heroesList;
        }
    }

    public bool isIdle {get; set;}

    public Army(List<Unit> units, Position position, Player owner) : base(position)
    {
        foreach (Unit unit in units) {
            unit.army = this;
        }
        this.units = units;
        this.owner = owner;

        isIdle = true;
        
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
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0), TileMap.tileSize);
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
                Sprite boatSprite = Sprite.Create(boatTexture, new Rect(0, 0, boatTexture.width, boatTexture.height), new Vector2(0, 0), TileMap.tileSize);
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

    /// Moves this army to the last index of the tile's armies list
    public void MoveToBack()
    {
        Tile currentTile = gameController.tileMap.GetTile(position);
        currentTile.RemoveArmy(this);
        currentTile.AddArmy(this, currentTile.armies.Count);

        EventManager.OnArmyReordered(this);
    }

    /// Sorts the unit list from weakest to strongest and alphabetically in case of the same strength (todo change this when implementing custom unit orders)
    private void SortUnits()
    {
        ArmyUtilities.SortUnits(units);
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
        unit.army = this;

        UpdatePathfindingTypes();
        SortUnits();

        UpdateSprite();
    }

    /// Adds a list of units to army
    public void AddUnits(List<Unit> units)
    {
        this.units.AddRange(units);
        foreach (Unit unit in units) {
            unit.army = this;
        }

        UpdatePathfindingTypes();
        SortUnits();

        UpdateSprite();
    }

    /// Returns true if there is enough space on this tile for all units in this army, otherwise returns false
    public override bool CanAddToGame()
    {
        return gameController.tileMap.GetTile(position).unitCount + units.Count <= Constants.maxUnitsPerTile;
    }

    /// Adds this army to the tileMap and to its owner armies, and creates the army sprite
    public override void AddToGame()
    {
        base.AddToGame();

        CreateSprite();
        UpdateSprite();

        gameController.tileMap.GetTile(position).AddArmy(this);
        owner.AddArmy(this);

        EventManager.OnArmyCreated(this);
    }

    /// Destroys the army - removes this army (and all its units) from the game
    public override void Destroy()
    {
        owner.RemoveArmy(this);
        gameController.tileMap.GetTile(position).RemoveArmy(this);

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
        int splitIndex = units.Count - 2;
        // starting with the pre-last unit, because we want to keep the last (strongest) unit as the first and selected army
        //     and have all other units sorted by strength in reverse order (so the stronger units are displayed first in army preview)

        while (units.Count > 1) {
            Unit splitUnit = units[splitIndex];
            units.RemoveAt(splitIndex);
            splitIndex -= 1;

            List<Unit> newUnitList = new List<Unit>();
            newUnitList.Add(splitUnit);
            Army newArmy = new Army(newUnitList, position, owner);
            newArmy.AddToGame();

            newArmy.MoveToBack();
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
        newArmy.MoveToBack();
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
                if (targetTile.structure as IOwnableMapObject != null) {
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
        isIdle = true;
        
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
            else if (isTransitioned && nextTile.transitionReturn != null) {
                if (nextTile.transitionReturn.from.IsSubsetOf(pathfindingTypes)) {
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
                isIdle = false;
                return false;
            }
        }
        
        if (attackTarget != null && ((MapObject)attackTarget).OccupiesPosition(nextPosition)) {
            if (!Input.GetKey(KeyCode.LeftShift)) {
                new Battle(this, attackTarget);  // this line looks super cursed, but everything communicates nicely through events, so it's fine. todo?
            }
            else {
                // TMP advisor, should be moved elsewhere. todo
                int victories = 0;
                for (int i = 0; i < Constants.simulatedBattlesCount; i += 1) {
                    Battle simulatedBattle = new Battle(this, attackTarget, true);
                    Player winner = null;
                    while (winner == null) {
                        winner = simulatedBattle.Turn();
                    }
                    if (winner == owner) {
                        victories += 1;
                    }
                }
                Debug.Log($"O Great Warlord! Your Advisor says: {(double)victories/(double)Constants.simulatedBattlesCount} victory chance!");
            }

            path = null;
            attackTarget = null;
            
            return false;
        }
        else {
            if ((nextTile.owner == null || nextTile.owner == owner) && nextTile.unitCount + units.Count <= Constants.maxUnitsPerTile) {

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

        isIdle = true;

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