using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public string saveFileDirectory = "Saves/";
    public string startFilePath = "Scenarios/The Spiral.json";
    public string saveFilePath = "Saves/quicksave.json";

    public TileMap tileMap {get; private set;}

    public float armyMoveDelay = 0.15f;
    private float currentArmyMoveDelay = 0;

    public int turn {get; private set;} = 0;
    private int activePlayerIndex = 0;
    public Player activePlayer
    {
        get {return players[activePlayerIndex];}
    }

    public bool gameOver {get; private set;} = false;

    public List<ItemData> ruinsItems {get; private set;}

    public List<Player> players {get; private set;}
    public List<Army> armies {get; private set;}
    public List<Item> items {get; private set;}
    public List<City> cities {get; private set;}

    public List<Road> roads {get; private set;}
    public List<Signpost> signposts {get; private set;}
    public List<Port> ports {get; private set;}
    public List<Ruins> ruins {get; private set;}

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
        tileMap = GameObject.FindGameObjectWithTag("TileMap").GetComponent<TileMap>();  // todo replace with new TileMap(...) and change TileMap to not be a MonoBehaviour

        saveFileDirectory =PlayerPrefs.GetString("saveFileDirectory", "");
        startFilePath=PlayerPrefs.GetString("startFilePath", "");
        saveFilePath=PlayerPrefs.GetString("saveFilePath", "");

        if(PlayerPrefs.GetString("mode", "") == "editorMode")
        {
            tileMap.width= PlayerPrefs.GetInt("width", 0);
            tileMap.height = PlayerPrefs.GetInt("height", 0);
        }

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
        

        players = new List<Player>();
        ruinsItems = new List<ItemData>();
        armies = new List<Army>();
        items = new List<Item>();
        cities = new List<City>();

        roads = new List<Road>();
        signposts = new List<Signpost>();
        ports = new List<Port>();
        ruins = new List<Ruins>();

        movingArmies = new List<Army>();

        tileMap.Initialize();

        if (startFilePath != null && startFilePath.Length != 0) {
            ResourceManager.LoadGame(startFilePath);

            activePlayer.StartTurn();
        } else if (saveFilePath != null && saveFilePath.Length != 0) {
            ResourceManager.LoadGame(saveFilePath);
        }
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

    /// Initializes the game with current turnNumber and name of active player and raises the OnTurn event. Called to start the game after everything is loaded.
    public void Initialize(int turnNumber, string activePlayerName)
    {
        activePlayerIndex = players.IndexOf(GetPlayerByName(activePlayerName));

        turn = turnNumber;

        EventManager.OnTurn(this);
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
    private void StructureCreatedHandler(object sender, System.EventArgs args)
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
        else if (sender as Ruins != null) {
            ruins.Add((Ruins)sender);
        }
    }

    /// Removes the structure from the corresponding structure list
    private void StructureDestroyedHandler(object sender, System.EventArgs args)
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
        else if (sender as Ruins != null) {
            ruins.Remove((Ruins)sender);
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
        if (gameOver) {
            return;
        }

        // don't allow the turn end if a battle is in progress, or there are units still moving
        if (activeBattle != null || movingArmies.Count > 0) {
            return;
        }

        int activePlayers = 0;
        foreach (Player player in players) {
            if (player.active) {
                activePlayers += 1;
            }
        }

        activePlayerIndex += 1;
        if (activePlayerIndex == players.Count) {
            turn += 1;
            activePlayerIndex = 0;
        }
        while (!players[activePlayerIndex].active && activePlayers != 0) {
            activePlayerIndex += 1;
            if (activePlayerIndex == players.Count) {
                turn += 1;
                activePlayerIndex = 0;
            }            
        }

        Debug.Log(activePlayer.name + " Turn " + (turn + 1));

        activePlayer.StartTurn();

        if (activePlayers == 1) {
            Debug.Log("Congratulations! You have conquered the world!");
            gameOver = true;
            EventManager.OnGameWon(this);
        } else {
            EventManager.OnTurn(this);
        }
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
        while (ruins.Count > 0) {
            ruins[0].Destroy();
        }
        players.Clear();
        ruinsItems.Clear();
    }
}
