using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public TileMap tileMap {get; private set;}

    public float armyMoveDelay = 0.15f;
    private float currentArmyMoveDelay = 0;

    public int turn {get; private set;} = 0;
    private int activePlayerIndex = 0;
    public Player activePlayer
    {
        get {return players[activePlayerIndex];}
    }

    public List<Player> players {get; private set;}
    public List<Army> armies {get; private set;}
    public List<Item> items {get; private set;}
    public List<City> cities {get; private set;}

    public List<Road> roads {get; private set;}
    public List<Signpost> signposts {get; private set;}
    public List<Port> ports {get; private set;}

    private List<Army> movingArmies;

    private Battle activeBattle;

    private System.EventHandler itemCreatedHandler;
    private System.EventHandler itemDestroyedHandler;
    private System.EventHandler armyCreatedHandler;
    private System.EventHandler armyDestroyedHandler;
    private System.EventHandler cityCreatedHandler;
    private System.EventHandler cityDestroyedHandler;

    void Awake()
    {
        itemCreatedHandler = (object sender, System.EventArgs args) => items.Add((Item)sender);
        itemDestroyedHandler = (object sender, System.EventArgs args) => items.Remove((Item)sender);

        armyCreatedHandler = (object sender, System.EventArgs args) => armies.Add((Army)sender);
        armyDestroyedHandler = (object sender, System.EventArgs args) => armies.Remove((Army)sender);

        cityCreatedHandler = (object sender, System.EventArgs args) => cities.Add((City)sender);
        cityDestroyedHandler = (object sender, System.EventArgs args) => cities.Remove((City)sender);

        EventManager.ArmyCreatedEvent += armyCreatedHandler;
        EventManager.ArmyDestroyedEvent += armyDestroyedHandler;

        EventManager.BattleStartedEvent += BattleStartedHandler;

        EventManager.CityCreatedEvent += cityCreatedHandler;
        EventManager.CityDestroyedEvent += cityDestroyedHandler;

        EventManager.ItemCreatedEvent += itemCreatedHandler;
        EventManager.ItemDestroyedEvent += itemDestroyedHandler;

        EventManager.StructureCreatedEvent += StructureCreatedHandler;
        EventManager.StructureDestroyedEvent += StructureDestroyedHandler;
    }

    void OnDestroy()
    {
        EventManager.ArmyCreatedEvent -= armyCreatedHandler;
        EventManager.ArmyDestroyedEvent -= armyDestroyedHandler;

        EventManager.BattleStartedEvent -= BattleStartedHandler;

        EventManager.CityCreatedEvent -= cityCreatedHandler;
        EventManager.CityDestroyedEvent -= cityDestroyedHandler;

        EventManager.ItemCreatedEvent -= itemCreatedHandler;
        EventManager.ItemDestroyedEvent -= itemDestroyedHandler;

        EventManager.StructureCreatedEvent -= StructureCreatedHandler;
        EventManager.StructureDestroyedEvent -= StructureDestroyedHandler;
    }

    /// Start is called before the first frame update
    void Start()
    {
        tileMap = GameObject.FindGameObjectWithTag("TileMap").GetComponent<TileMap>();  // todo replace with new TileMap(...) and change TileMap to not be a MonoBehaviour

        players = new List<Player>();
        armies = new List<Army>();
        items = new List<Item>();
        cities = new List<City>();

        roads = new List<Road>();
        signposts = new List<Signpost>();
        ports = new List<Port>();

        movingArmies = new List<Army>();

        tileMap.Initialize();

        ResourceManager.LoadGame("save.json");

        EventManager.OnTurn(this);
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
            }
            else {
                currentArmyMoveDelay -= Time.deltaTime;
            }
        }
        else {
            currentArmyMoveDelay = 0;
        }
    }

    /// Starts the army's automatic movement along its path
    public void StartArmyMove(Army army)
    {
        if (!armies.Contains(army)) {
            throw new System.ArgumentException($"Army does not exist in the game: {army}");
        }
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
            if (activeBattle != null) {
                return;
            }
        }
    }

    /// Stops automatic movement of all moving armies and starts a battle between attacker and defender
    private void BattleStartedHandler(object sender, System.EventArgs args)
    {
        if (activeBattle != null) {
            // this should never happen, but let's throw an exception just in case
            throw new System.ArgumentException("Cannot start a battle while another battle is in progress");
        }

        // todo add a super cool camera animation centering on the attacker or something

        activeBattle = (Battle)sender;
    }

    /// Adds the newly created structure to the corresponding structure list
    public void StructureCreatedHandler(object sender, System.EventArgs args)
    {
        // there will be a few such if statements, but I think that's better than having separate events and handlers for every kind of structure
        if (sender as Road != null) {
            roads.Add((Road)sender);
        }
        else if (sender as Signpost != null) {
            signposts.Add((Signpost)sender);
        }
        else if (sender as Port != null) {
            ports.Add((Port)sender);
        }
    }

    /// Removes the structure from the corresponding structure list
    public void StructureDestroyedHandler(object sender, System.EventArgs args)
    {
        // there will be a few such if statements, but I think that's better than having separate events and handlers for every kind of structure
        if (sender as Road != null) {
            roads.Remove((Road)sender);
        }
        else if (sender as Signpost != null) {
            signposts.Remove((Signpost)sender);
        }
        else if (sender as Port != null) {
            ports.Remove((Port)sender);
        }
    }

    /// Adds a new player to the list of players
    public void AddPlayer(Player newPlayer)
    {
        foreach (Player player in players) {
            if (player.name == newPlayer.name) {
                throw new KeyNotFoundException($"Cannot add player. Player with name {newPlayer.name} already exists.");
            }
        }
        players.Add(newPlayer);
    }

    /// Returns a player with the provided name. Throws KeyNotFoundException if no such player exists
    public Player GetPlayerByName(string playerName)
    {
        foreach (Player player in players) {
            if (player.name == playerName) {
                return player;
            }
        }
        throw new KeyNotFoundException($"Player {playerName} does not exist.");
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

        activePlayer.StartTurn();

        EventManager.OnTurn(this);
    }

    /// Removes all armies, cities, players, and structures from the game
    public void Clear()
    {
        while (armies.Count > 0) {
            armies[0].Destroy();
        }
        while (items.Count > 0) {
            items[0].Destroy();
        }
        while (cities.Count > 0) {
            cities[0].Destroy();
        }
        while (roads.Count > 0) {
            roads[0].Destroy();
        }
        while (signposts.Count > 0) {
            signposts[0].Destroy();
        }
        while (ports.Count > 0) {
            ports[0].Destroy();
        }
        players.Clear();
    }
}
