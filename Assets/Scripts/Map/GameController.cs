using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public TileMap tileMap {get; private set;}
    public ResourceManager resourceManager {get; private set;}

    public float armyMoveDelay = 0.15f;
    private float currentArmyMoveDelay = 0;

    private int turn = 0;
    private int activePlayerIndex = 0;
    public Player activePlayer
    {
        get { return players[activePlayerIndex]; }
    }

    private List<Player> players;

    private List<Army> movingArmies;

    private Battle activeBattle;

    private TurnInfoDisplay turnInfoDisplay;  // todo remove this, replace with a main UI Controller object
    private ResourcesDisplay resourcesDisplay;  // todo remove this, replace with a main UI Controller object

    /// Start is called before the first frame update
    void Start()
    {
        tileMap = GameObject.FindGameObjectWithTag("TileMap").GetComponent<TileMap>();  // todo replace with new TileMap(...) and change TileMap to not be a MonoBehaviour
        resourceManager = new ResourceManager();

        movingArmies = new List<Army>();
        players = new List<Player>();

        tileMap.Initialize();

        turnInfoDisplay= GameObject.Find("Main").GetComponent<TurnInfoDisplay>();
        resourcesDisplay = GameObject.Find("Main").GetComponent<ResourcesDisplay>();

        resourcesDisplay.UpdateResources(activePlayer.cities.Count, activePlayer.gold, activePlayer.income, activePlayer.upkeep);
        
        turnInfoDisplay.showTurnInfo(activePlayer.name, turn + 1);
    }

    /// Update is called once per frame
    void Update()
    {
        if (activeBattle != null) {
            // a not-so-pretty way to check if the active battle has ended, probably a //todo
            if (activeBattle.winner != null) {
                activeBattle = null;
            }

            // stop all moving armies if a battle is in progress
            while (movingArmies.Count > 0) {
                StopArmyMove(movingArmies[0]);
            }
        }

        if (movingArmies.Count > 0) {
            if (currentArmyMoveDelay <= 0) {
                currentArmyMoveDelay = armyMoveDelay;
                MoveArmies();
            } else {
                currentArmyMoveDelay -= Time.deltaTime;
            }
        } else {
            currentArmyMoveDelay = 0;
        }
    }

    /// Starts the army's automatic movement along its path
    public void StartArmyMove(Army army)
    {
        if (!movingArmies.Contains(army)) {
            movingArmies.Add(army);
        }
    }

    /// Stops the army's automatic movement along its path
    public void StopArmyMove(Army army)
    {
        if (movingArmies.Contains(army)) {
            movingArmies.Remove(army);
        }
    }

    /// Moves every army from the movingArmies list by one step, if possible
    private void MoveArmies()
    {
        for (int i = 0; i < movingArmies.Count; i += 1) {
            bool success = movingArmies[i].MoveOneStep();
            if (!success) {
                movingArmies.RemoveAt(i);
                i -= 1;
            }
        }
    }

    /// Moves army from it's position to newPosition. Throws System.ArgumentException if newPosition is not adjacent to army position or is an invalid position for this army
    public void MoveArmy(Army army, Position newPosition)
    {
        if (!tileMap.GetNeighbouringPositions(army.position).Contains(newPosition)) {
            throw new System.ArgumentException($"Cannot move army from {army.position} to {newPosition}. Positions are not adjacent.");
        }
        Tile targetTile = tileMap.GetTile(newPosition);
        if (!targetTile.pathfindingTypes.Overlaps(army.pathfindingTypes)) {
            throw new System.ArgumentException($"No common pathfinding type exists between army at {army.position} and tile at {newPosition}.");
        }

        //todo check if there is room for more units on the target tile

        tileMap.GetTile(army.position).RemoveArmy(army);
        targetTile.AddArmy(army);
        army.position = newPosition;  // todo I don't like this... Why should setting army position be done from an outside object?
    }

    /// Stops automatic movement of all moving armies and starts a battle between attacker and defender
    public void StartBattle(Army attacker, IPlayerMapObject defender)
    {
        if (activeBattle != null) {
            // this should never happen, but let's throw an exception just in case
            throw new System.ArgumentException("Cannot start a battle while another battle is in progress");
        }

        // todo add a super cool camera animation centering on the attacker or something

        activeBattle = new Battle(attacker, defender);

        // todo replace all of this with some nice elegant communication with a main "UI Controller" object
        BattleScreen battleScreen = GameObject.Find("Main").GetComponent<BattleScreen>();
        battleScreen.battlePanel.SetActive(true);
        battleScreen.winInfo.text = "";
        battleScreen.battle = activeBattle;
    }

    /// Adds a new player to the list of players
    public void AddPlayer(Player player)
    {
        players.Add(player);
    }

    /// Adds an army to the tile at the army's position, and to the army owner's armies
    public void AddArmy(Army army)
    {
        tileMap.GetTile(army.position).AddArmy(army);
        army.owner.AddArmy(army);
    }

    /// Removes the army from TileMap and from its owner armies
    public void DestroyArmy(Army army)
    {
        army.owner.RemoveArmy(army);
        tileMap.GetTile(army.position).RemoveArmy(army);
    }

    /// Adds a city to all tiles in the city's occupied position, and to the city owner's cities
    public void AddCity(City city)
    {
        foreach (Position occupiedPosition in city.occupiedPositions) {
            Tile occupiedTile = tileMap.GetTile(city.position + occupiedPosition);
            if (occupiedTile.city != null) {
                throw new System.ArgumentException($"Position {city.position + occupiedPosition} is already occupied by another city");
            }
            occupiedTile.AddCity(city);
        }

        city.owner.AddCity(city);
    }

    /// Removes the city from its owner cities
    public void RazeCity(City city)
    {
        city.owner.RemoveCity(city);
    }

    /// Changes the owner of the city
    public void CaptureCity(City city, Player newOwner)
    {
        city.owner.RemoveCity(city);
        newOwner.AddCity(city);
    }

    /// Ends the active player's turn and starts the next player's turn
    public void Turn()
    {
        // don't allow the turn end if a battle is in progress, or there are units still moving
        if (activeBattle != null || movingArmies.Count > 0) {
            return;
        }

        activePlayerIndex += 1;
        if (activePlayerIndex == players.Count) {
            turn += 1;
            activePlayerIndex = 0;
        }

        Debug.Log(activePlayer.name + " Turn " + (turn + 1));
        turnInfoDisplay.showTurnInfo(activePlayer.name, turn + 1);
        activePlayer.StartTurn();
        resourcesDisplay.UpdateResources(activePlayer.cities.Count, activePlayer.gold, activePlayer.income, activePlayer.upkeep);
    }
}
