using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TurnInfoDisplay : MonoBehaviour
{

    public TextMeshProUGUI armyName;
    public TextMeshProUGUI turnNumber;

    public GameObject TurnInfoPanel;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            TurnInfoPanel.SetActive(false);
        }   
    }

    public void showTurnInfo(string playerName, int turn)
    {
        armyName.text = playerName;
        turnNumber.text = "Turn "+turn.ToString();
        TurnInfoPanel.SetActive(true);
    }

}
