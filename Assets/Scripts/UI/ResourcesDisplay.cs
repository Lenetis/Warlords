using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResourcesDisplay : MonoBehaviour
{
    private static GameController gameController;
    public TextMeshProUGUI[] resources = new TextMeshProUGUI[4];

    private System.EventHandler updateResourcesEventHandler;  // used to handle all events that cause updating resources

    void Awake()
    {
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();

        updateResourcesEventHandler = (object sender, System.EventArgs args) => UpdateResources();

        EventManager.ArmyCreatedEvent += updateResourcesEventHandler;
        EventManager.ArmyDestroyedEvent += updateResourcesEventHandler;

        EventManager.BattleEndedEvent += updateResourcesEventHandler;

        EventManager.CityCapturedEvent += CityCapturedHandler;
        EventManager.CityCreatedEvent += updateResourcesEventHandler;
        EventManager.CityDestroyedEvent += updateResourcesEventHandler;
        EventManager.CityRazedEvent += updateResourcesEventHandler;

        EventManager.ItemCreatedEvent += updateResourcesEventHandler;
        EventManager.ItemDestroyedEvent += updateResourcesEventHandler;

        EventManager.RuinsExploredEvent += updateResourcesEventHandler;

        EventManager.UnitBoughtEvent += UnitBoughtHandler;
        EventManager.TurnEvent += updateResourcesEventHandler;
    }

    void OnDestroy()
    {
        EventManager.ArmyCreatedEvent -= updateResourcesEventHandler;
        EventManager.ArmyDestroyedEvent -= updateResourcesEventHandler;

        EventManager.BattleEndedEvent -= updateResourcesEventHandler;

        EventManager.CityCapturedEvent -= CityCapturedHandler;
        EventManager.CityCreatedEvent -= updateResourcesEventHandler;
        EventManager.CityDestroyedEvent -= updateResourcesEventHandler;
        EventManager.CityRazedEvent -= updateResourcesEventHandler;

        EventManager.ItemCreatedEvent -= updateResourcesEventHandler;
        EventManager.ItemDestroyedEvent -= updateResourcesEventHandler;

        EventManager.RuinsExploredEvent -= updateResourcesEventHandler;

        EventManager.UnitBoughtEvent -= UnitBoughtHandler;
        EventManager.TurnEvent -= updateResourcesEventHandler;
    }

    private void UnitBoughtHandler(object sender, Unit unit)
    {
        UpdateResources();
    }

    private void CityCapturedHandler(object sender, CityCapturedEventData eventData)
    {
        UpdateResources();
    }

    private void UpdateResources()
    {
        resources[0].text = gameController.activePlayer.cities.Count.ToString();
        resources[1].text = gameController.activePlayer.gold.ToString() + "gp";
        resources[2].text = gameController.activePlayer.economy.income.ToString() + "gp";
        resources[3].text = gameController.activePlayer.economy.upkeep.ToString() + "gp";
        Debug.Log("Resources updated");
    }
}
