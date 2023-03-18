using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleScreen : MonoBehaviour
{
    public GameObject battlePanel;

    private Battle _battle;
    public Battle battle
    {
        private get {return _battle;}
        set {
            _battle = value;
            if (value != null) {
                currentTurnDelay = 0;
                UpdateUnitImages(value.attackingUnits, attackerUnits, attackerPanel);
                UpdateUnitImages(value.defendingUnits, defenderUnits, defenderPanel);
            }
        }
    }

    public float turnDelay = 1;  // todo move this delay to game, use BattleScreen only for displaying the battle info, not for time-related stuff
    private float currentTurnDelay = 0;
    private bool boool;
    public Player winner;

    public GameObject unitImage;
    public GameObject attackerPanel;
    public GameObject defenderPanel;
    public List<GameObject> attackerUnits;
    public List<GameObject> defenderUnits;

    public TextMeshProUGUI winInfo;


    // Start is called before the first frame update
    void Start()
    {
        boool = true;
        battle = null;
        attackerUnits = new List<GameObject>();
        defenderUnits = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (battle != null) {
            currentTurnDelay += Time.deltaTime;
            if (currentTurnDelay >= turnDelay*0.75 || battle.winner != null) {

                if (boool)
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
                    boool = false;
                }
                

                if(currentTurnDelay >= turnDelay)
                {
                    UpdateAttacker(battle.attackingUnits);
                    UpdateDefender(battle.defendingUnits);


                    if (winner != null)
                    {
                        string info;
                        if (winner != battle.attackingPlayer)
                        {
                            info = "You have lost!";
                        }
                        else
                        {
                            info = winner.name + " have won\nthe battle!";
                        }

                        battle = null;

                        ArmyManagement armyManagement = GameObject.Find("Main").GetComponent<ArmyManagement>();
                        armyManagement.RefreshSelection();
                        Debug.Log($"Battle ended. Winner = {winner}");
                        winInfo.text = info;


                    }
                    currentTurnDelay -= turnDelay;
                    boool = true;

                }
            }
        } else {
            if (Input.GetMouseButtonUp(0))
            {
                battlePanel.SetActive(false);

                for (int i = 0; i < attackerUnits.Count; i++)
                {
                    Destroy(attackerUnits[i]);
                }

                for (int i = 0; i < defenderUnits.Count; i++)
                {
                    Destroy(defenderUnits[i]);
                }
            }
        }
    }

    private void UpdateUnitImages(List<Unit> units, List<GameObject> unitImages, GameObject unitPanel)
    {
        for (int i = 0; i < unitImages.Count; i += 1) {
            Destroy(unitImages[i]);
        }

        unitImages.Clear();
        for (int i = 0; i < units.Count; i += 1) {
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
}
