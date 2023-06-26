using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TurnInfoDisplay : MonoBehaviour
{
    private static GameController gameController;

    public TextMeshProUGUI armyName;
    public TextMeshProUGUI turnNumber;

    public GameObject TurnInfoPanel;

    void Awake()
    {
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();

        EventManager.TurnEvent += TurnHandler;
    }

    void OnDestroy()
    {
        EventManager.TurnEvent -= TurnHandler;
    }


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            TurnInfoPanel.SetActive(false);
        }   
    }

    private void TurnHandler(object sender, System.EventArgs args)
    {
        ShowTurnInfo(gameController.activePlayer.name, gameController.turn + 1);
    }

    private void ShowTurnInfo(string playerName, int turn)
    {
        armyName.text = playerName;
        turnNumber.text = "Turn "+turn.ToString();
        TurnInfoPanel.SetActive(true);
    }

}
