using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArmyManagement : MonoBehaviour
{
    public MouseSelection mouseSelection;

    public GameObject armyManagementPanel;

    public Army selectedArmy;
    public List<Army> selectedArmies;

    //L2
    public GameObject unitButton;
    public GameObject[] units;
    public Image[] unitsImage;
    public Image[] unitsCheckBox;
    public bool[] activeUnits;
    public TextMeshProUGUI[] movesAvailable;

    // Start is called before the first frame update
    void Start()
    {
        mouseSelection = GameObject.Find("Main Camera").GetComponent<MouseSelection>();
    }

    // Update is called once per frame
    void Update()
    {/*
        if (mouseSelection.selectedArmies != null) {
            
            int counter = 0;
            for (int i = 0; i < mouseSelection.selectedArmies.Count; i += 1) {
                for (int j = 0; j < mouseSelection.selectedArmies[i].units.Count; j += 1)
                {
                    if (movesAvailable[counter].text != mouseSelection.selectedArmies[i].units[j].remainingMove.ToString())
                    {
                        movesAvailable[counter].text = mouseSelection.selectedArmies[i].units[j].remainingMove.ToString();
                    }
                    counter += 1;
                }
            }
            Debug.Log(counter);
        }*/

        if (mouseSelection.selectedArmy != null)
        {
            for (int i = 0; i < mouseSelection.selectedArmy.units.Count; i += 1)
            {
                if (movesAvailable[i].text != mouseSelection.selectedArmy.units[i].remainingMove.ToString())
                {
                    movesAvailable[i].text = mouseSelection.selectedArmy.units[i].remainingMove.ToString();
                }
                Debug.Log(i);
            }
        }
    }

    public void SelectArmy(List<Army> selectedArmies)
    {
        this.selectedArmies = selectedArmies;
        if (armyManagementPanel.activeSelf)
        {
            DeselectArmy();
        }
        int armiesSize = 0;
        //int armySize = mouseSelection.selectedArmy.units.Count;

        for (int i = 0; i < selectedArmies.Count; i++)
        {
            armiesSize += selectedArmies[i].units.Count;
        }

        units = new GameObject[armiesSize];
        unitsImage = new Image[armiesSize];
        unitsCheckBox = new Image[armiesSize];
        activeUnits = new bool[armiesSize];
        movesAvailable = new TextMeshProUGUI[armiesSize];

        int counter = 0;

        armyManagementPanel.SetActive(true);
        for (int i = 0; i < selectedArmies.Count; i++)
        {
            for (int j = 0; j < selectedArmies[i].units.Count; j++)
            {

                units[counter] = Instantiate(unitButton, armyManagementPanel.transform);
                units[counter].transform.localPosition = new Vector3((counter + 1) * ((armyManagementPanel.GetComponent<RectTransform>().sizeDelta.x / ((armiesSize) + 1))) - (armyManagementPanel.GetComponent<RectTransform>().sizeDelta.x / 2), 10, 0);
                units[counter].transform.SetParent(armyManagementPanel.transform);
                units[counter].name = counter.ToString();

                unitsImage[counter] = units[counter].transform.GetChild(1).gameObject.GetComponent<Image>();
                unitsImage[counter].sprite = Sprite.Create(selectedArmies[i].units[j].texture, new Rect(0.0f, 0.0f, selectedArmies[i].units[j].texture.width, selectedArmies[i].units[j].texture.height), new Vector2(0.5f, 0.5f), 100.0f);

                movesAvailable[counter] = units[counter].transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
                movesAvailable[counter].text = selectedArmies[i].units[j].remainingMove.ToString();

                unitsCheckBox[counter] = units[counter].transform.GetChild(2).gameObject.GetComponent<Image>();
                unitsCheckBox[counter].color = new Color(176f / 255f, 255f / 255f, 145f / 255f);

                activeUnits[counter] = true;
                //Debug.Log(mouseSelection.highlightedTile.contents.armies[0].owner.armies.Count); 
                counter++;
            }
        }
    }

    public void RefreshSelection()
    {
        if (selectedArmies != null)
        {
            SelectArmy(selectedArmies);
        }
    }

    public void DeselectArmy()
    {
        armyManagementPanel.SetActive(false);
        for (int i = 0; i < units.Length; i++)
        {
            Destroy(units[i]);
        }
    }

    public void HideArmyManagementPanel()
    {
        armyManagementPanel.SetActive(false);
    }

    public void setUnitActivity(int index)
    {
        Debug.Log(index);
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
