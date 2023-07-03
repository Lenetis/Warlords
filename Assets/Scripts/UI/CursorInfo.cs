using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CursorInfo : MonoBehaviour
{
    public GameObject infoPanel;
    public GameObject gui;

    public MouseSelection mouseSelection;

    public TextMeshProUGUI objectName;
    public TextMeshProUGUI objectDescription;

    //public GameObject displayArea;
    public bool isOverDispArea;

    public Vector3 mousePosTmp;
    public bool isMoved;
    public bool isSaved;

    public float correctionX, correctionY;
    public int mode;
    public string buttonName;
    public string[] buttonDescription=new string[4];

    public GameObject cityStats;
    public GameObject unitStats;
    public GameObject goldImage;
    public GameObject swordsImage;

    public GameObject unitImage;
    public GameObject[] selectedUnits;
    public GameController gameController;
    public UIController uiController;

    public GameObject stone;
    public GameObject wood;

    public int dispMode;

    public TextMeshProUGUI text1;
    public TextMeshProUGUI text2;

    // Start is called before the first frame update
    void Start()
    {
        mouseSelection = GameObject.Find("Main Camera").GetComponent<MouseSelection>();
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
        uiController = GameObject.Find("UIController").GetComponent<UIController>();
        dispMode = 0;
    }

    // Update is called once per frame
    void Update()
    {
        float infoPanelWidth = (infoPanel.GetComponent<RectTransform>().anchorMax.x - infoPanel.GetComponent<RectTransform>().anchorMin.x) * Screen.width + infoPanel.GetComponent<RectTransform>().sizeDelta.x * gui.GetComponent<Canvas>().scaleFactor;
        float infoPanelHeight = (infoPanel.GetComponent<RectTransform>().anchorMax.y - infoPanel.GetComponent<RectTransform>().anchorMin.y) * Screen.height + infoPanel.GetComponent<RectTransform>().sizeDelta.y * gui.GetComponent<Canvas>().scaleFactor;
    
        //Debug.Log(dispAreaOriginX+" "+Input.mousePosition.x);

        if (Input.GetMouseButton(1) && mouseSelection.isOverDispArea && uiController.controllsAvailable() || mode==1 || mode==2 || mode==3)
        {
            correctionY = 0;
            correctionX = 0;

            if (Screen.height - Input.mousePosition.y < infoPanelHeight / 2)
            {
                correctionY = -Input.mousePosition.y + Screen.height - infoPanelHeight / 2;
            }

            if (Input.mousePosition.y < infoPanelHeight / 2)
            {
                correctionY = -Input.mousePosition.y + infoPanelHeight / 2;
            }

            if (Screen.width - Input.mousePosition.x < infoPanelWidth / 2)
            {
                correctionX = -Input.mousePosition.x + Screen.width - infoPanelWidth / 2;
            }

            if (Input.mousePosition.x < infoPanelWidth / 2)
            {
                correctionX = -Input.mousePosition.x + infoPanelWidth / 2;
            }

            //Debug.Log(Screen.height - Input.mousePosition.y);
            //Debug.Log(infoPanelHeight);

            if (!isSaved)
            {
                mousePosTmp = Input.mousePosition;
                isSaved = true;
            }

            if (Input.mousePosition != mousePosTmp)
            {
                isMoved = true;
            }

            if (!isMoved)
            {
                objectName.GetComponent<RectTransform>().anchoredPosition = new Vector2(objectName.GetComponent<RectTransform>().anchoredPosition.x, 15);
                if (mode == 0 && mouseSelection.selectedArmy == null)
                {
                    if (mouseSelection.highlightedTile.armies != null)
                    {
                        unitStats.SetActive(false);
                        cityStats.SetActive(false);
                        if (wood.activeSelf)
                        {
                            wood.SetActive(false);
                        }
                        if (!stone.activeSelf)
                        {
                            stone.SetActive(true);
                        }
                        objectName.text = "";
                        objectDescription.text = "";

                        int allUnitsCount = 0;
                        for(int i=0; i< mouseSelection.highlightedTile.armies.Count; i++)
                        {
                            for(int j=0; j< mouseSelection.highlightedTile.armies[i].units.Count; j++)
                            {
                                allUnitsCount++;
                            }
                        }

                        if (selectedUnits.Length == 0)
                        {
                            selectedUnits = new GameObject[allUnitsCount];
                        }

                        int index = 0;
                        for (int i = 0; i < mouseSelection.highlightedTile.armies.Count; i++)
                        {

                            for(int j=0; j< mouseSelection.highlightedTile.armies[i].units.Count; j++)
                            {
                                if (selectedUnits[index] == null)
                                {
                                    selectedUnits[index] = Instantiate(unitImage, infoPanel.transform);
                                    selectedUnits[index].transform.localPosition = new Vector3((index + 1) * ((infoPanel.GetComponent<RectTransform>().sizeDelta.x / ((allUnitsCount) + 1))) - (infoPanel.GetComponent<RectTransform>().sizeDelta.x / 2), 0, 0);
                                    selectedUnits[index].transform.SetParent(infoPanel.transform);
                                    selectedUnits[index].name = index.ToString();

                                    selectedUnits[index].GetComponent<Image>().sprite = Sprite.Create(mouseSelection.highlightedTile.armies[i].units[j].texture, new Rect(0.0f, 0.0f, mouseSelection.highlightedTile.armies[i].units[j].texture.width, mouseSelection.highlightedTile.armies[i].units[j].texture.height), new Vector2(0.5f, 0.5f), 100.0f);
                                }
                                index++;
                            }
                        }
                    }
                    else if (mouseSelection.highlightedTile.structure as City != null)
                    {
                        unitStats.SetActive(false);
                        if (stone.activeSelf)
                        {
                            stone.SetActive(true);
                        }
                        if (!wood.activeSelf)
                        {
                            wood.SetActive(false);
                        }

                        City city = (City)mouseSelection.highlightedTile.structure;
                        objectName.text = city.name;
                        if (city.razed) {
                            goldImage.SetActive(false);
                            swordsImage.SetActive(false);
                            objectDescription.text = "Razed!";
                            cityStats.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
                            cityStats.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = "";
                        }
                        else {
                            goldImage.SetActive(true);
                            swordsImage.SetActive(true);
                            objectDescription.text = "";
                            cityStats.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = city.economy.income.ToString();
                            cityStats.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = city.battleStats.strength.ToString();
                        }
                        cityStats.SetActive(true);
                    }
                    else if (mouseSelection.highlightedTile.structure as Signpost != null)
                    {
                        unitStats.SetActive(false);
                        cityStats.SetActive(false);
                        if (stone.activeSelf)
                        {
                            stone.SetActive(false);
                        }
                        if (!wood.activeSelf)
                        {
                            wood.SetActive(true);
                        }

                        Signpost signpost = (Signpost)mouseSelection.highlightedTile.structure;
                        objectName.text = signpost.name;
                        objectDescription.text = signpost.description;
                    }
                    else if (mouseSelection.highlightedTile.structure as Port != null)
                    {
                        unitStats.SetActive(false);
                        cityStats.SetActive(false);
                        if (stone.activeSelf)
                        {
                            stone.SetActive(true);
                        }
                        if (!wood.activeSelf)
                        {
                            wood.SetActive(false);
                        }

                        objectName.text = "Port";
                        objectDescription.text = "A way for armies to put to sea";
                    }
                    else if (mouseSelection.highlightedTile.structure as Road != null)
                    {
                        unitStats.SetActive(false);
                        cityStats.SetActive(false);
                        if (stone.activeSelf)
                        {
                            stone.SetActive(true);
                        }
                        if (!wood.activeSelf)
                        {
                            wood.SetActive(false);
                        }

                        Road road = (Road)mouseSelection.highlightedTile.structure;
                        if (mouseSelection.highlightedTile.pathfindingTypes.Contains("sea")) {
                            objectName.text = "Bridge";
                            objectDescription.text = "For land armies to cross water";
                        } else {
                            objectName.text = "Road";
                            objectDescription.text = "The fastest way to travel";
                        }
                    }
                    else if (mouseSelection.highlightedTile.structure as Ruins != null)
                    {
                        unitStats.SetActive(false);
                        cityStats.SetActive(false);
                        if (stone.activeSelf)
                        {
                            stone.SetActive(true);
                        }
                        if (!wood.activeSelf)
                        {
                            wood.SetActive(false);
                        }

                        Ruins ruins = (Ruins)mouseSelection.highlightedTile.structure;
                        objectName.text = "Ruins";
                        objectDescription.text = ruins.explored ? "Explored!" : "Unexplored!";
                    }
                    else
                    {
                        unitStats.SetActive(false);
                        cityStats.SetActive(false);
                        if (wood.activeSelf)
                        {
                            wood.SetActive(false);
                        }
                        if (!stone.activeSelf)
                        {
                            stone.SetActive(true);
                        }

                        objectName.text = mouseSelection.highlightedTile.data.name;
                        objectDescription.text = mouseSelection.highlightedTile.data.description;
                    }

                    infoPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2((Input.mousePosition.x + correctionX) - Screen.width / 2, (Input.mousePosition.y + correctionY) - Screen.height / 2) / gui.GetComponent<Canvas>().scaleFactor;
                    Cursor.visible = false;
                    infoPanel.SetActive(true);
                }
                else if(mode== 1 && mouseSelection.selectedArmy == null)
                {
                    unitStats.SetActive(false);
                    cityStats.SetActive(false);
                    if (wood.activeSelf)
                    {
                        wood.SetActive(false);
                    }
                    if (!stone.activeSelf)
                    {
                        stone.SetActive(true);
                    }
                    objectName.text = buttonName;
                    objectDescription.text = buttonDescription[0];
                    if (dispMode == 1)
                    {
                        objectName.GetComponent<RectTransform>().anchoredPosition = new Vector2(objectName.GetComponent<RectTransform>().anchoredPosition.x, 0);
                    }

                    infoPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2((Input.mousePosition.x + correctionX) - Screen.width / 2, (Input.mousePosition.y + correctionY) - Screen.height / 2) / gui.GetComponent<Canvas>().scaleFactor;
                    Cursor.visible = false;
                    infoPanel.SetActive(true);
                }
                else if (mode == 2 && mouseSelection.selectedArmy == null)
                {
                    unitStats.SetActive(false);
                    if (buttonDescription[0] == "Razed!")
                    {
                        goldImage.SetActive(false);
                        swordsImage.SetActive(false);
                        objectDescription.text = buttonDescription[0];
                        cityStats.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
                        cityStats.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = "";
                    }
                    else
                    {
                        goldImage.SetActive(true);
                        swordsImage.SetActive(true);
                        objectDescription.text = "";
                        cityStats.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = buttonDescription[0];
                        cityStats.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = buttonDescription[1];
                    }
                    cityStats.SetActive(true);
                    

                    if (wood.activeSelf)
                    {
                        wood.SetActive(false);
                    }
                    if (!stone.activeSelf)
                    {
                        stone.SetActive(true);
                    }
                    objectName.text = buttonName;

                    infoPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2((Input.mousePosition.x + correctionX) - Screen.width / 2, (Input.mousePosition.y + correctionY) - Screen.height / 2) / gui.GetComponent<Canvas>().scaleFactor;
                    Cursor.visible = false;
                    infoPanel.SetActive(true);
                }
                else if (mode == 3)
                {
                    cityStats.SetActive(false);
                    unitStats.SetActive(true);
                    if (wood.activeSelf)
                    {
                        wood.SetActive(false);
                    }
                    if (!stone.activeSelf)
                    {
                        stone.SetActive(true);
                    }

                    objectDescription.text = "";
                    objectName.text = buttonName;
                    unitStats.transform.GetChild(4).GetComponent<TextMeshProUGUI>().text = buttonDescription[0];
                    unitStats.transform.GetChild(5).GetComponent<TextMeshProUGUI>().text = buttonDescription[1];
                    unitStats.transform.GetChild(6).GetComponent<TextMeshProUGUI>().text = buttonDescription[2];
                    unitStats.transform.GetChild(7).GetComponent<TextMeshProUGUI>().text = buttonDescription[3];

                    if (mouseSelection.selectedArmy == null)
                    {
                        unitStats.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Time:";
                        unitStats.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = "Cost:";
                    }
                    else
                    {
                        unitStats.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Remain:";
                        unitStats.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = "Upkeep:";
                    }

                    infoPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2((Input.mousePosition.x + correctionX) - Screen.width / 2, (Input.mousePosition.y + correctionY) - Screen.height / 2) / gui.GetComponent<Canvas>().scaleFactor;
                    Cursor.visible = false;
                    infoPanel.SetActive(true);
                }
                else
                {
                    infoPanel.SetActive(false);
                    DeleteUnits();
                    selectedUnits = new GameObject[0];
                    Cursor.visible = true;
                }                
            }
            else
            {
                infoPanel.SetActive(false);
                DeleteUnits();
                selectedUnits = new GameObject[0];
                Cursor.visible = true;
            }
        }
        else
        {
            infoPanel.SetActive(false);
            DeleteUnits();
            selectedUnits = new GameObject[0];
            Cursor.visible = true;
            isSaved = false;
            isMoved = false;
        }
    }

    public void DeleteUnits()
    {
        for (int i = 0; i < selectedUnits.Length; i++)
        {
            Destroy(selectedUnits[i]);
        }
    }

}
