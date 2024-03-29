using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroPanel : MonoBehaviour
{
    private GameController gameController;
    private Minimap minimap;
    public TMP_InputField heroNameInput;
    public GameObject heroNameInputPlaceholder;
    public GameObject heroPanel;
    public TextMeshProUGUI[] info=new TextMeshProUGUI[2];
    public Button cancelButton;
    public Button acceptButton;

    private HeroSpawnEventData eventData;

    private int mode;

    public GameObject infoPanel;

    private bool firstTurn;
    private bool firstTurnLoaded;

    // Start is called before the first frame update
    void Start()
    {
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
        minimap = GameObject.Find("Main").GetComponent<Minimap>();
        heroNameInput.onValueChanged.AddListener(delegate {OnInputValueChanged(); });
    }

    private void Awake()
    {
        firstTurn = true;
        firstTurnLoaded = false;
        EventManager.HeroSpawnedEvent += HeroSpawnEventHandler;
    }

    void OnDestroy()
    {
        EventManager.HeroSpawnedEvent -= HeroSpawnEventHandler;
    }

    // Update is called once per frame
    void Update()
    {
        if(minimap.isTileMapLoaded && !firstTurnLoaded)
        {
            HeroSpawnEventHandler(this, eventData);
            firstTurnLoaded = true;
        }
    }

    private void HeroSpawnEventHandler(object sender, HeroSpawnEventData eventData)
    {
        heroNameInput.text = "";
        acceptButton.interactable = true;
        if (firstTurn)
        {
            this.eventData = eventData;
            firstTurn = false;
        }
        else
        {
            this.eventData = eventData;
            minimap.DrawHero(eventData);

            if (gameController.turn == 0)
            {
                //Debug.Log(eventData.heroUnit.name);
                heroNameInputPlaceholder.GetComponent<TextMeshProUGUI>().text = eventData.heroUnit.name;
                mode = 0;
                Debug.Log($"A Hero emerges in {eventData.city.name}");
                info[0].text = $"A Hero emerges in\n{eventData.city.name}";
                cancelButton.interactable = false;
                heroPanel.SetActive(true);
            }
            else
            {
                heroNameInputPlaceholder.GetComponent<TextMeshProUGUI>().text = eventData.heroUnit.name;
                mode = 1;
                cancelButton.interactable = true;
                Debug.Log($"A Hero in {eventData.city.name} offers to join you for {eventData.heroCost} gold. You have {gameController.activePlayer.gold} gold to spend. Will you accept?");
                info[0].text = $"A Hero in {eventData.city.name}\noffers to join you for {eventData.heroCost} gold.\nYou have {gameController.activePlayer.gold} gold to spend.\nWill you accept?";
                cancelButton.interactable = true;
                acceptButton.interactable = true;
                
                info[1].text = "And the Hero brings no allies...";
                if (eventData.alliesCount == 1)
                {
                    Debug.Log($"And the Hero brings 1 ally!");
                    info[1].text = $"And the Hero brings 1 ally!";
                }
                else if (eventData.alliesCount > 1)
                {
                    Debug.Log($"And the Hero brings {eventData.alliesCount} allies!");
                    info[1].text = $"And the Hero brings {eventData.alliesCount} allies!";
                }

                heroPanel.SetActive(true);
            }
        }
    }

    public void AcceptHero()
    {
        if(heroNameInput.text != "")
        {
            eventData.heroUnit.name = heroNameInput.text;
        }
       
        gameController.activePlayer.gold -= eventData.heroCost;
        gameController.activePlayer.SpawnHero(eventData.heroUnit, eventData.city, eventData.alliesCount);
        if (mode == 1)
        {
            infoPanel.SetActive(true);
        }
        else
        {
            heroPanel.SetActive(false);
        }
    }

    public void RejectHero()
    {
        
        heroPanel.SetActive(false);
        
    }

    public void CloseInfo()
    {
        infoPanel.SetActive(false);
        heroPanel.SetActive(false);
    }

    public void OnInputValueChanged()
    {
        if (heroNameInput.text.Length>=3 || heroNameInput.text=="")
        {
            acceptButton.interactable = true;
        }
        else
        {
            acceptButton.interactable = false;
        }
    }
}
