using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ArmyMovedEventData
{
    public Position startPosition;
    public Position endPosition;
}

public struct TileMapResizedEventData
{
    public int oldWidth;
    public int oldHeight;
    public int newWidth;
    public int newHeight;
}

public struct HeroSpawnEventData
{
    public int heroCost;
    public Unit heroUnit;
    public City city;
    public int alliesCount;
}

public static class EventManager
{
    public static event System.EventHandler<ArmyMovedEventData> ArmyMovedEvent;
    public static event System.EventHandler ArmyCreatedEvent;
    public static event System.EventHandler ArmyDestroyedEvent;
    public static event System.EventHandler BattleEndedEvent;
    public static event System.EventHandler BattleStartedEvent;
    public static event System.EventHandler CityCapturedEvent;
    public static event System.EventHandler CityCreatedEvent;
    public static event System.EventHandler CityDestroyedEvent;
    public static event System.EventHandler CityRazedEvent;
    public static event System.EventHandler<HeroSpawnEventData> HeroSpawnedEvent;
    public static event System.EventHandler ItemCreatedEvent;
    public static event System.EventHandler ItemDestroyedEvent;
    public static event System.EventHandler RuinsExploredEvent;
    public static event System.EventHandler StructureCreatedEvent;
    public static event System.EventHandler StructureDestroyedEvent;
    public static event System.EventHandler<TileMapResizedEventData> TileMapResizedEvent;
    public static event System.EventHandler TurnEvent;
    public static event System.EventHandler<Unit> UnitBoughtEvent;
    public static event System.EventHandler UnitDestroyedEvent;


    public static void OnArmyMoved(object sender, ArmyMovedEventData eventData) {
        ArmyMovedEvent?.Invoke(sender, eventData);
    }

    public static void OnArmyCreated(object sender) {
        ArmyCreatedEvent?.Invoke(sender, System.EventArgs.Empty);
    }

    public static void OnArmyDestroyed(object sender) {
        ArmyDestroyedEvent?.Invoke(sender, System.EventArgs.Empty);
    }

    public static void OnBattleEnded(object sender) {
        BattleEndedEvent?.Invoke(sender, System.EventArgs.Empty);
    }

    public static void OnBattleStarted(object sender) {
        BattleStartedEvent?.Invoke(sender, System.EventArgs.Empty);
    }

    public static void OnCityCaptured(object sender) {
        CityCapturedEvent?.Invoke(sender, System.EventArgs.Empty);
    }

    public static void OnCityCreated(object sender) {
        CityCreatedEvent?.Invoke(sender, System.EventArgs.Empty);
    }

    public static void OnCityDestroyed(object sender) {
        CityDestroyedEvent?.Invoke(sender, System.EventArgs.Empty);
    }

    public static void OnCityRazed(object sender) {
        CityRazedEvent?.Invoke(sender, System.EventArgs.Empty);
    }

    public static void OnHeroSpawn(object sender, HeroSpawnEventData eventData) {
        HeroSpawnedEvent?.Invoke(sender, eventData);
    }

    public static void OnItemCreated(object sender) {
        ItemCreatedEvent?.Invoke(sender, System.EventArgs.Empty);
    }

    public static void OnItemDestroyed(object sender) {
        ItemDestroyedEvent?.Invoke(sender, System.EventArgs.Empty);
    }

    public static void OnRuinsExplored(object sender) {
        RuinsExploredEvent?.Invoke(sender, System.EventArgs.Empty);
    }

    public static void OnStructureCreated(object sender) {
        StructureCreatedEvent?.Invoke(sender, System.EventArgs.Empty);
    }

    public static void OnStructureDestroyed(object sender) {
        StructureDestroyedEvent?.Invoke(sender, System.EventArgs.Empty);
    }

    public static void OnTileMapResized(object sender, TileMapResizedEventData eventData) {
        TileMapResizedEvent?.Invoke(sender, eventData);
    }

    public static void OnTurn(object sender) {
        TurnEvent?.Invoke(sender, System.EventArgs.Empty);
    }

    public static void OnUnitBought(object sender, Unit unit) {
        UnitBoughtEvent?.Invoke(sender, unit);
    }

    public static void OnUnitDestroyed(object sender) {
        UnitDestroyedEvent?.Invoke(sender, System.EventArgs.Empty);
    }
}
