using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battle
{
    // todo maybe add strength/attack/defence modifiers etc? (or maybe add them in Army)

    private IPlayerMapObject defender;

    public List<Unit> attackingUnits {get; private set;}
    public List<Unit> defendingUnits {get; private set;}

    public Player attackingPlayer {get;}
    public Player defendingPlayer {get;}

    private List<Army> armies;

    private HashSet<Unit> deadUnits;

    public Player winner {get; private set;}

    private TileMap tileMap;

    public BattleScreen battleScreen;

    public Battle(Army attacker, IPlayerMapObject defender)
    {
        this.defender = defender;

        attackingUnits = new List<Unit>();
        attackingUnits.AddRange(attacker.units);

        List<Army> defendingArmies = defender.GetSupportingArmies();
        defendingUnits = new List<Unit>();
        foreach (Army defendingArmy in defendingArmies) {
            defendingUnits.AddRange(defendingArmy.units);
        }

        // todo sort attackingUnits and defendingUnits

        Debug.Log("Battle!");
        Debug.Log($"Attackers: {string.Join(", ", attackingUnits)}");
        Debug.Log($"Defenders: {string.Join(", ", defendingUnits)}");

        attackingPlayer = attacker.owner;
        defendingPlayer = defender.owner;

        armies = new List<Army>();
        armies.Add(attacker);
        armies.AddRange(defendingArmies);

        deadUnits = new HashSet<Unit>();

        if (defendingUnits.Count == 0) {
            winner = attackingPlayer;
        } else {
            winner = null;
        }

        tileMap = GameObject.FindGameObjectWithTag("TileMap").GetComponent<TileMap>();

        battleScreen = GameObject.Find("Main").GetComponent<BattleScreen>();
        battleScreen.battlePanel.SetActive(true);
        battleScreen.winInfo.text = "";
        battleScreen.battle = this;
    }

    // calculates a single turn of the battle. Returns winner if battle is over and null if it is not.
    public Player Turn()
    {
        if (winner == null) {
            if (Random.Range(0, attackingUnits[0].strength + defendingUnits[0].strength) < attackingUnits[0].strength) {
                Debug.Log($"Defender {defendingUnits[0]} died");
                deadUnits.Add(defendingUnits[0]);
                defendingUnits.RemoveAt(0);
            } else {
                Debug.Log($"Attacker {attackingUnits[0]} died");
                deadUnits.Add(attackingUnits[0]);
                attackingUnits.RemoveAt(0);
            }
        }

        if (attackingUnits.Count == 0) {
            winner = defendingPlayer;
        } else if (defendingUnits.Count == 0) {
            winner = attackingPlayer;
        }
        RemoveDeadUnits();

        if (winner == attackingPlayer) {
            City attackedCity = tileMap.GetTile(defender.position).contents.city;
            if (attackedCity != null) {
                attackedCity.Capture(attackingPlayer);
            }
        }

        Debug.Log($"{attackingUnits.Count} vs {defendingUnits.Count} --- {winner}");
        return winner;
    }

    private void RemoveDeadUnits()
    {
        foreach (Army army in armies) {
            int aliveUnitIndex = 0;
            while (aliveUnitIndex < army.units.Count) {
                if (deadUnits.Contains(army.units[aliveUnitIndex])) {
                    army.RemoveUnit(army.units[aliveUnitIndex]);
                } else {
                    aliveUnitIndex += 1;
                }
            }
        }
    }
}
