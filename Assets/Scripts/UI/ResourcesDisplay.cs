using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResourcesDisplay : MonoBehaviour
{
    private static GameController gameController;
    public TextMeshProUGUI[] resources = new TextMeshProUGUI[4];

    void Awake()
    {
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();

        EventManager.ArmyCreatedEvent += ArmyCreatedHandler;
        EventManager.ArmyDestroyedEvent += ArmyDestroyedHandler;

        EventManager.BattleEndedEvent += BattleEndedHandler;

        EventManager.CityCapturedEvent += CityCapturedHandler;
        EventManager.CityCreatedEvent += CityCreatedHandler;
        EventManager.CityDestroyedEvent += CityDestroyedHandler;
        EventManager.CityRazedEvent += CityRazedHandler;

        EventManager.TurnEvent += TurnHandler;
        EventManager.UnitBoughtEvent += UnitBoughtHandler;
    }

    void OnDestroy()
    {
        EventManager.ArmyCreatedEvent -= ArmyCreatedHandler;
        EventManager.ArmyDestroyedEvent -= ArmyDestroyedHandler;

        EventManager.BattleEndedEvent -= BattleEndedHandler;

        EventManager.CityCapturedEvent -= CityCapturedHandler;
        EventManager.CityCreatedEvent -= CityCreatedHandler;
        EventManager.CityDestroyedEvent -= CityDestroyedHandler;
        EventManager.CityRazedEvent -= CityRazedHandler;

        EventManager.TurnEvent -= TurnHandler;
        EventManager.UnitBoughtEvent -= UnitBoughtHandler;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ArmyCreatedHandler(object sender, System.EventArgs args)
    {
        UpdateResources();
    }

    private void ArmyDestroyedHandler(object sender, System.EventArgs args)
    {
        UpdateResources();
    }

    private void BattleEndedHandler(object sender, System.EventArgs args)
    {
        UpdateResources();
    }

    private void CityCapturedHandler(object sender, System.EventArgs args)
    {
        UpdateResources();
    }

    private void CityCreatedHandler(object sender, System.EventArgs args)
    {
        UpdateResources();
    }

    private void CityDestroyedHandler(object sender, System.EventArgs args)
    {
        UpdateResources();
    }

    private void CityRazedHandler(object sender, System.EventArgs args)
    {
        UpdateResources();
    }

    private void TurnHandler(object sender, System.EventArgs args)
    {
        UpdateResources();
    }

    private void UnitBoughtHandler(object sender, Unit unit)
    {
        UpdateResources();
    }

    private void UpdateResources()
    {
        resources[0].text = "Noc: "+ gameController.activePlayer.cities.Count.ToString();
        resources[1].text = "YT: " + gameController.activePlayer.gold.ToString();
        resources[2].text = "YI: " + gameController.activePlayer.income.ToString();
        resources[3].text = "YU: " + gameController.activePlayer.upkeep.ToString();
        Debug.Log("Resources updated");
    }
}
