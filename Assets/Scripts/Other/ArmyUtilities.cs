using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ArmyUtilities
{
    public static void SortUnits(List<Unit> units)
    {
        units.Sort((unit1, unit2) => CompareUnits(unit1, unit2));
    }

    public static void SortArmies(List<Army> armies)
    {
        armies.Sort((army1, army2) => -CompareUnits(army1.units[^1], army2.units[^1]));
    }

    private static int CompareUnits(Unit unit1, Unit unit2)
    {
        int result = unit1.isHero.CompareTo(unit2.isHero);
        if (result == 0) {
            result = unit1.battleStats.strength.CompareTo(unit2.battleStats.strength);
        }
        if (result == 0) {
            result = unit1.name.CompareTo(unit2.name);
        }
        return result;
    }
}
