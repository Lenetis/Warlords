using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants
{
    public static readonly int maxUnitsPerTile = 8;

    public static readonly int pathfindingTransitionCost = 10;
    public static readonly int pathfindingMaxDistinctTransitions = 1;
    // maximum number of transitions to distinct set of pathfindingTypes (currently 1, because only transition to ["sea"] exists)

    public static readonly int cityCaptureMinGold = 100;
    public static readonly int cityCaptureMaxGold = 300;

    public static readonly string firstHeroItem = "Assets/Resources/Items/standard.json";

    public static readonly float ruinsUnitJoinChance = 0.333f;
    public static readonly float ruinsItemFindChance = 0.25f;
    public static readonly float ruinsBattleVictoryChance = 0.8f;
    public static readonly int ruinsExploreExperience = 5;
    public static readonly string[] ruinsUnits = {
        "Assets/Resources/Units/Ghost.json",
        "Assets/Resources/Units/GiantBat.json",
        "Assets/Resources/Units/Elephant.json",
        "Assets/Resources/Units/Giant.json",
        "Assets/Resources/Units/Spider.json"
    };
    public static readonly string[] ruinsEnemies = {
        "a Dragon",
        "a Ghost",
        "a Pack of Wolves",
        "an Elephant",
        "a Giant",
        "a Very Large Worm",
        "a Giant Space Spider"
    };
    public static readonly int ruinsMinGold = 750;
    public static readonly int ruinsMaxGold = 2500;

    public static readonly int simulatedBattlesCount = 100;

    public static readonly int battleMaxUnitBonus = 5;

    public static readonly string[] defeatText = {
        "thy cities are as dust!",
        "thou art no more!",
        "thou art vanquished!",
        "for thee the war is over!"
    };
}
