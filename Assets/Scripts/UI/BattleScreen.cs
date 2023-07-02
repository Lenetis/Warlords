using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleScreen : MonoBehaviour
{
    private static GameController gameController;
    private CityManagement cityManagement;
    private MouseSelection mouseSelection;

    public GameObject battlePanel;
    public GameObject animationPanel;
    public GameObject actionPanel;

    private Battle _battle;
    public Battle battle
    {
        private get { return _battle; }
        set
        {
            _battle = value;
            if (value != null)
            {
                currentTurnDelay = 0;
                UpdateUnitImages(value.attackingUnits, attackerUnits, attackerPanel);
                UpdateUnitImages(value.defendingUnits, defenderUnits, defenderPanel);
            }
        }
    }

    public float turnDelay = 1;  // todo move this delay to game, use BattleScreen only for displaying the battle info, not for time-related stuff
    private float currentTurnDelay = 0;
    private bool waitingForUnitsUpdate;
    public Player winner;

    public GameObject unitImage;
    public GameObject attackerPanel;
    public GameObject defenderPanel;
    public List<GameObject> attackerUnits;
    public List<GameObject> defenderUnits;

    public TextMeshProUGUI winInfo;

    private City attackedCity;
    private Player attackingPlayer;


    void Awake()
    {
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();

        EventManager.BattleStartedEvent += BattleStartedHandler;
    }

    void OnDestroy()
    {
        EventManager.BattleStartedEvent -= BattleStartedHandler;
    }

    // Start is called before the first frame update
    void Start()
    {
        attackerUnits = new List<GameObject>();
        defenderUnits = new List<GameObject>();
        cityManagement=GameObject.Find("Main").GetComponent<CityManagement>();
        mouseSelection = GameObject.Find("Main Camera").GetComponent<MouseSelection>();
    }

    // Update is called once per frame
    void Update()
    {
        if (battle != null)
        {
            currentTurnDelay += Time.deltaTime;
            if (currentTurnDelay >= turnDelay * 0.75)
            {
                if (battle.winner == null && !waitingForUnitsUpdate)
                {
                    winner = battle.Turn();

                    if (battle.attackingUnits.Count < attackerUnits.Count)
                    {
                        attackerUnits[0].transform.GetChild(0).gameObject.SetActive(true);
                        Debug.Log("BOOOOM");
                    }

                    if (battle.defendingUnits.Count < defenderUnits.Count)
                    {
                        defenderUnits[0].transform.GetChild(0).gameObject.SetActive(true);
                        Debug.Log("BOOOOM");
                    }

                    waitingForUnitsUpdate = true;
                }

                if (currentTurnDelay >= turnDelay && waitingForUnitsUpdate)
                {
                    UpdateAttacker(battle.attackingUnits);
                    UpdateDefender(battle.defendingUnits);

                    waitingForUnitsUpdate = false;

                    currentTurnDelay -= turnDelay;

                    if (battle.winner != null)
                    {
                        string info;
                        if (battle.winner != battle.attackingPlayer)
                        {
                            info = "You have lost!";
                        }
                        else
                        {
                            info = battle.winner.name + " have won\nthe battle!";
                        }

                        Debug.Log($"Battle ended. Winner = {battle.winner}");
                        winInfo.text = info;

                        battle = null;
                    }
                }
            }
        }
        else
        {
            if (Input.GetMouseButtonUp(0))
            {
                for (int i = 0; i < attackerUnits.Count; i++)
                {
                    Destroy(attackerUnits[i]);
                }

                for (int i = 0; i < defenderUnits.Count; i++)
                {
                    Destroy(defenderUnits[i]);
                }

                if (attackedCity != null && winner == attackingPlayer)
                {
                    animationPanel.SetActive(false);
                    actionPanel.SetActive(true);
                    mouseSelection.DeselectArmy();
                }
                else {
                    battlePanel.SetActive(false);
                }
            }
        }
    }

    private void BattleStartedHandler(object sender, System.EventArgs args)
    {
        attackerUnits.Clear();
        defenderUnits.Clear();
        
        actionPanel.SetActive(false);
        animationPanel.SetActive(true);
        battlePanel.SetActive(true);

        winInfo.text = "";
        battle = (Battle)sender;

        attackingPlayer = battle.attackingPlayer;
        attackedCity = (gameController.tileMap.GetTile(battle.defender.position).structure) as City;

        currentTurnDelay = 0;
        waitingForUnitsUpdate = false;
    }

    private void UpdateUnitImages(List<Unit> units, List<GameObject> unitImages, GameObject unitPanel)
    {
        for (int i = 0; i < unitImages.Count; i += 1)
        {
            Destroy(unitImages[i]);
        }

        unitImages.Clear();
        for (int i = 0; i < units.Count; i += 1)
        {
            GameObject newUnitImage = Instantiate(unitImage, unitPanel.transform);
            newUnitImage.transform.localPosition = new Vector3((i + 1) * ((unitPanel.GetComponent<RectTransform>().sizeDelta.x / ((units.Count) + 1))) - (unitPanel.GetComponent<RectTransform>().sizeDelta.x / 2), 0, 0);
            newUnitImage.transform.SetParent(unitPanel.transform);
            newUnitImage.name = i.ToString();

            newUnitImage.GetComponent<Image>().sprite = Sprite.Create(units[i].texture, new Rect(0.0f, 0.0f, units[i].texture.width, units[i].texture.height), new Vector2(0.5f, 0.5f), 100.0f);

            unitImages.Add(newUnitImage);
        }
    }

    public void UpdateAttacker(List<Unit> units)
    {
        UpdateUnitImages(units, attackerUnits, attackerPanel);
    }

    public void UpdateDefender(List<Unit> units)
    {
        UpdateUnitImages(units, defenderUnits, defenderPanel);
    }

    public void OccupyCity()
    {
        attackedCity.Capture(attackingPlayer);
        actionPanel.SetActive(false);
        animationPanel.SetActive(true);
        battlePanel.SetActive(false);
        cityManagement.SelectCity(attackedCity);
    }

    public void PillageCity()
    {
        attackedCity.Capture(attackingPlayer, 1);
        actionPanel.SetActive(false);
        animationPanel.SetActive(true);
        battlePanel.SetActive(false);
        cityManagement.SelectCity(attackedCity);
    }
    
    public void SackCity()
    {
        attackedCity.Capture(attackingPlayer, 4);
        actionPanel.SetActive(false);
        animationPanel.SetActive(true);
        battlePanel.SetActive(false);
        cityManagement.SelectCity(attackedCity);
    }

    public void RazeCity()
    {
        attackedCity.Raze();
        actionPanel.SetActive(false);
        animationPanel.SetActive(true);
        battlePanel.SetActive(false);
    }
}
