using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CityManagement : MonoBehaviour
{
    public MouseSelection mouseSelection;

    public GameObject cityManagementPanel;
    public GameObject armyManagementPanel;

    public TextMeshProUGUI cityName;

    //L2
    public GameObject[] units = new GameObject[8];
    public Image[] unitsImage = new Image[8];
    public Image[] unitsCheckBox = new Image[8];
    public bool[] activeUnits = new bool[8];
    public TextMeshProUGUI[] movesAvailable = new TextMeshProUGUI[8];



    // Start is called before the first frame update
    void Start()
    {
        mouseSelection = GameObject.Find("Main Camera").GetComponent<MouseSelection>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (mouseSelection.highlightedTile.contents.armies != null)
            {
                //Debug.Log("Army");
                //Debug.Log(mouseSelection.highlightedTile.contents.armies[0].units.Count);
                armyManagementPanel.SetActive(true);
                for(int i = 0; i < 8; i++)
                {
                    if(i < mouseSelection.highlightedTile.contents.armies[0].owner.armies[0].units.Count)
                    {
                        units[i].SetActive(true);
                        unitsImage[i].sprite = mouseSelection.highlightedTile.contents.armies[0].owner.armies[0].mapSprite.GetComponent<SpriteRenderer>().sprite;
                        movesAvailable[i].text = mouseSelection.highlightedTile.contents.armies[0].owner.armies[0].units[i].remainingMove.ToString();
                        unitsCheckBox[i].color = new Color(176f / 255f, 255f / 255f, 145f / 255f);
                        activeUnits[i] = true;
                        //Debug.Log(mouseSelection.highlightedTile.contents.armies[0].owner.armies.Count); 
                    }
                    else
                    {
                        units[i].SetActive(false);
                        activeUnits[i] = false;
                        movesAvailable[i].text="0";
                    }
                    
                }
            }
            else if (mouseSelection.highlightedTile.contents.city != null)
            {
                cityName.text = "City";
                cityManagementPanel.SetActive(true);
            }
            else
            {
                //Debug.Log("Terrain");
            }
        }
    }

    public void HideCityManagementPanel()
    {
        cityManagementPanel.SetActive(false);
    }
    public void HideArmyManagementPanel()
    {
        armyManagementPanel.SetActive(false);
    }

    public void setUnitActivity(int index)
    {
        if (activeUnits[index] == true)
        {
            activeUnits[index] = false;
            unitsCheckBox[index].color = new Color(255f / 255f, 102f / 255f, 80f / 255f);
        }
        else
        {
            activeUnits[index] = true;
            unitsCheckBox[index].color = new Color(176f / 255f, 255f / 255f, 145f / 255f);
        }
    }

}
