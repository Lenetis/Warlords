using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battle
{
    // todo maybe add strength/attack/defence modifiers etc? (or maybe add them in Army)

    private static GameController gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();

    public IOwnableMapObject defender {get; private set;}

    public List<Unit> attackingUnits {get; private set;}
    public List<Unit> defendingUnits {get; private set;}

    public Player attackingPlayer {get;}
    public Player defendingPlayer {get;}

    private List<Army> armies;

    private HashSet<Unit> deadUnits;

    public Player winner {get; private set;}

    public Battle(Army attacker, IOwnableMapObject defender)
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

        EventManager.OnBattleStarted(this);
    }

    /// Calculates a single turn of the battle. Returns winner if battle is over and null if it is not.
    public Player Turn()
    {
        if (winner != null) {
            throw new System.ArgumentException("Cannot make another turn in battle. The battle has already ended.");
        }

        if (defendingUnits.Count != 0 && attackingUnits.Count != 0) {
            if (Random.Range(0, attackingUnits[0].battleStats.strength + defendingUnits[0].battleStats.strength) < attackingUnits[0].battleStats.strength) {
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

        if (winner != null) {
            EventManager.OnBattleEnded(this);
        }

        Debug.Log($"{attackingUnits.Count} vs {defendingUnits.Count} --- {winner}");
        return winner;
    }

    /// Removes all units that died in this battle from their respective armies
    private void RemoveDeadUnits()
    {
        foreach (Unit unit in deadUnits) {
            unit.Destroy();
        }
    }
}
