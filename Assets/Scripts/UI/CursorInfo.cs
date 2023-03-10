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
    public string buttonDescription;

    public GameObject unitImage;
    public GameObject[] selectedUnits;
    public GameController gameController;


    // Start is called before the first frame update
    void Start()
    {
        mouseSelection = GameObject.Find("Main Camera").GetComponent<MouseSelection>();
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
    }

    // Update is called once per frame
    void Update()
    {
        float infoPanelWidth = (infoPanel.GetComponent<RectTransform>().anchorMax.x - infoPanel.GetComponent<RectTransform>().anchorMin.x) * Screen.width + infoPanel.GetComponent<RectTransform>().sizeDelta.x * gui.GetComponent<Canvas>().scaleFactor;
        float infoPanelHeight = (infoPanel.GetComponent<RectTransform>().anchorMax.y - infoPanel.GetComponent<RectTransform>().anchorMin.y) * Screen.height + infoPanel.GetComponent<RectTransform>().sizeDelta.y * gui.GetComponent<Canvas>().scaleFactor;
    
        //Debug.Log(dispAreaOriginX+" "+Input.mousePosition.x);

        if (Input.GetMouseButton(1) && mouseSelection.isOverDispArea || mode==1)
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

            if (!isMoved && !mouseSelection.isSelected)
            {
                if (mode == 0)
                {
                    if (mouseSelection.highlightedTile.contents.armies != null && mouseSelection.highlightedTile.contents.armies[0].owner==gameController.activePlayer)
                    {
                        objectName.text = "";
                        objectDescription.text = "";
                        
                        if (selectedUnits.Length == 0)
                        {
                            selectedUnits = new GameObject[mouseSelection.highlightedTile.contents.armies[0].units.Count];
                        }
                        
                        for (int i = 0; i < selectedUnits.Length; i++)
                        {
                            if (selectedUnits[i] == null)
                            {
                                selectedUnits[i] = Instantiate(unitImage, infoPanel.transform);
                                selectedUnits[i].transform.localPosition = new Vector3((i + 1) * ((infoPanel.GetComponent<RectTransform>().sizeDelta.x / ((mouseSelection.highlightedTile.contents.armies[0].units.Count) + 1))) - (infoPanel.GetComponent<RectTransform>().sizeDelta.x / 2), 0, 0);
                                selectedUnits[i].transform.SetParent(infoPanel.transform);
                                selectedUnits[i].name = i.ToString();

                                selectedUnits[i].GetComponent<Image>().sprite = Sprite.Create(mouseSelection.highlightedTile.contents.armies[0].units[i].texture, new Rect(0.0f, 0.0f, mouseSelection.highlightedTile.contents.armies[0].units[i].texture.width, mouseSelection.highlightedTile.contents.armies[0].units[i].texture.height), new Vector2(0.5f, 0.5f), 100.0f);

                            }

                        }
                    }
                    else if (mouseSelection.highlightedTile.contents.city != null)
                    {
                        objectName.text = mouseSelection.highlightedTile.contents.city.name;
                        objectDescription.text = mouseSelection.highlightedTile.contents.city.description;
                    }
                    else
                    {
                        objectName.text = mouseSelection.highlightedTile.data.name;
                        objectDescription.text = mouseSelection.highlightedTile.data.description;
                    }
                }
                else if(mode==1)
                {
                    objectName.text = buttonName;
                    objectDescription.text = buttonDescription;
                }
                

                //Debug.Log(gameObject.GetComponent<RectTransform>().anchoredPosition.x * gui.GetComponent<Canvas>().scaleFactor);
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
