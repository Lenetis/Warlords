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

public static class EventManager
{
    public static event System.EventHandler<ArmyMovedEventData> ArmyMovedEvent;
    public static event System.EventHandler ArmyCreatedEvent;
    public static event System.EventHandler ArmyDestroyedEvent;
    public static event System.EventHandler BattleStartedEvent;
    public static event System.EventHandler CityCapturedEvent;
    public static event System.EventHandler CityCreatedEvent;
    public static event System.EventHandler CityDestroyedEvent;
    public static event System.EventHandler CityRazedEvent;
    public static event System.EventHandler<TileMapResizedEventData> TileMapResizedEvent;
    public static event System.EventHandler TurnEvent;


    public static void OnArmyMoved(object sender, ArmyMovedEventData eventData) {
        ArmyMovedEvent?.Invoke(sender, eventData);
    }

    public static void OnArmyCreated(object sender) {
        ArmyCreatedEvent?.Invoke(sender, System.EventArgs.Empty);
    }

    public static void OnArmyDestroyed(object sender) {
        ArmyDestroyedEvent?.Invoke(sender, System.EventArgs.Empty);
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

    public static void OnTileMapResized(object sender, TileMapResizedEventData eventData) {
        TileMapResizedEvent?.Invoke(sender, eventData);
    }

    public static void OnTurn(object sender) {
        TurnEvent?.Invoke(sender, System.EventArgs.Empty);
    }
}
