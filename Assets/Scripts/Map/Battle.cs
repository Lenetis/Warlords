using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battle
{
    // todo maybe add strength/attack/defence modifiers etc? (or maybe add them in Army)

    private List<Unit> attackingUnits;
    private List<Unit> defendingUnits;

    private Player attackingPlayer;
    private Player defendingPlayer;

    private List<Army> armies;

    public Player winner {get; private set;}

    public ArmyManagement armyManagement;
    public BattleScreen battleScreen;

    public Battle(Army attacker, IPlayerMapObject defender)
    {
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
    }

    public void Start()
    {
        battleScreen = GameObject.Find("Main").GetComponent<BattleScreen>();
        battleScreen.battlePanel.SetActive(true);

        Debug.Log("Def: "+ defendingUnits.Count);
        Debug.Log("Att: "+ attackingUnits.Count);

        battleScreen.UpdateDefender(defendingUnits,0);
        battleScreen.UpdateAttacker(attackingUnits,0);

        HashSet<Unit> deadUnits = new HashSet<Unit>();
        while (attackingUnits.Count > 0 && defendingUnits.Count > 0) {
            if (Random.Range(0, attackingUnits[0].strength + defendingUnits[0].strength) < attackingUnits[0].strength) {
                Debug.Log($"Defender {defendingUnits[0]} died");
                deadUnits.Add(defendingUnits[0]);
                defendingUnits.RemoveAt(0);
                battleScreen.UpdateDefender(defendingUnits,1);
            } else {
                Debug.Log($"Attacker {attackingUnits[0]} died");
                deadUnits.Add(attackingUnits[0]);
                attackingUnits.RemoveAt(0);
                battleScreen.UpdateAttacker(attackingUnits,1);
            }
        }

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

        string info;
        if (attackingUnits.Count == 0) {
            winner = defendingPlayer;
            info = "You have lost!";
        } else {
            winner = attackingPlayer;
            info = winner.name + " have won\nthe battle!";
        }

        armyManagement= GameObject.Find("Main").GetComponent<ArmyManagement>();
        armyManagement.SelectArmy(armyManagement.selectedArmy);
        Debug.Log($"Battle ended. Winner = {winner}");
        battleScreen.winInfo.text = info;
    }
}
