using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private int turn = 0;
    private int activePlayerIndex = 0;

    private TurnInfoDisplay turnInfoDisplay;

    public Player activePlayer
    {
        get { return players[activePlayerIndex]; }
    }

    private List<Player> players;


    // Start is called before the first frame update
    void Start()
    {
        turnInfoDisplay= GameObject.Find("Main").GetComponent<TurnInfoDisplay>();
        //players = new List<Player>();  // todo uncomment this after removing the tmp players from tilemap generation
    }

    public void AddPlayer(Player player)
    {
        if (players == null) {
            players = new List<Player>();  // todo remove this after removing the tmp players from tilemap generation
        }
        players.Add(player);
    }

    public void Turn()
    {
        // todo only allow ending turn if none of the active player's armies are moving
        activePlayerIndex += 1;
        if (activePlayerIndex == players.Count) {
            turn += 1;
            activePlayerIndex = 0;
        }
        
        Debug.Log(activePlayer.name + " Turn " + (turn + 1));
        turnInfoDisplay.showTurnInfo(activePlayer.name,(turn+1));
        players[activePlayerIndex].StartTurn();
    }
}
