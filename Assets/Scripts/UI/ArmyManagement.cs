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
    /////public Image[] unitsCheckBox;
    public List<bool> activeUnits = new List<bool>();
    public TextMeshProUGUI[] movesAvailable;
    public int[,] colorPalette = {{114, 161, 255 } ,{114, 255, 157 },{ 255, 255, 114 },{ 255, 125, 114 },
                                { 255, 114, 228 },{193, 114, 255 },{ 114, 251, 255 },{ 199, 255, 114 }};
    public int MSMode = 1;
    public TextMeshProUGUI modeButonText;

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
                // todo this throws an IndexOutOfRangeException during battles, because movesAvailable.Length is equal to 0, even though selected army has more than 0 units.
                //      I haven't checked why this happens, but it's probably related to "int armiesSize = 0;" in SelectArmy()
                if (movesAvailable[i].text != mouseSelection.selectedArmy.units[i].remainingMove.ToString())
                {
                    movesAvailable[i].text = mouseSelection.selectedArmy.units[i].remainingMove.ToString();
                }
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
        /////unitsCheckBox = new Image[armiesSize];
        activeUnits = new List<bool>();
        movesAvailable = new TextMeshProUGUI[armiesSize];

        int counter = 0;

        armyManagementPanel.SetActive(true);

        int colorIndex=0;
        for (int i = 0; i < selectedArmies.Count; i++)
        {
            for (int j = 0; j < selectedArmies[i].units.Count; j++)
            {

                units[counter] = Instantiate(unitButton, armyManagementPanel.transform.GetChild(0).gameObject.transform);
                units[counter].transform.localPosition = new Vector3((counter + 1) * ((armyManagementPanel.transform.GetChild(0).gameObject.GetComponent<RectTransform>().sizeDelta.x / ((armiesSize) + 1))) - (armyManagementPanel.transform.GetChild(0).gameObject.GetComponent<RectTransform>().sizeDelta.x / 2), 10, 0);
                units[counter].transform.SetParent(armyManagementPanel.transform.GetChild(0).gameObject.transform);
                units[counter].name = counter.ToString();
                units[counter].GetComponent<UnitButton>().army = i;
                units[counter].GetComponent<UnitButton>().unit = j;

                unitsImage[counter] = units[counter].transform.GetChild(1).gameObject.GetComponent<Image>();
                unitsImage[counter].sprite = Sprite.Create(selectedArmies[i].units[j].texture, new Rect(0.0f, 0.0f, selectedArmies[i].units[j].texture.width, selectedArmies[i].units[j].texture.height), new Vector2(0.5f, 0.5f), 100.0f);

                movesAvailable[counter] = units[counter].transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
                movesAvailable[counter].text = selectedArmies[i].units[j].remainingMove.ToString();

                /////unitsCheckBox[counter] = units[counter].transform.GetChild(2).gameObject.GetComponent<Image>();
                /////unitsCheckBox[counter].color = new Color(176f / 255f, 255f / 255f, 145f / 255f);

                units[counter].transform.GetChild(3).gameObject.GetComponent<TextMeshProUGUI>().text = (i+1).ToString();

                if (colorIndex >= 8)
                {
                    colorIndex = 0;
                }
                units[counter].GetComponent<Image>().color = new Color32(System.Convert.ToByte(colorPalette[colorIndex, 0]), System.Convert.ToByte(colorPalette[colorIndex, 1]), System.Convert.ToByte(colorPalette[colorIndex, 2]), 100);
                activeUnits.Add(true);
                counter++;
            }
            colorIndex++;
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

    public void SetUnitActivity(int index)
    {
        Debug.Log("Button index: "+index);
        Debug.Log("Button index: " + index+"; Army: " + units[index].GetComponent<UnitButton>().army+"; Unit: "+units[index].GetComponent<UnitButton>().unit);

        if (MSMode == -1)
        {
            selectedArmies[units[index].GetComponent<UnitButton>().army].SplitUnit(selectedArmies[units[index].GetComponent<UnitButton>().army].units[units[index].GetComponent<UnitButton>().unit]);
        }
        else
        {
            selectedArmies[0].AddUnit(selectedArmies[units[index].GetComponent<UnitButton>().army].units[units[index].GetComponent<UnitButton>().unit]);
            selectedArmies[units[index].GetComponent<UnitButton>().army].RemoveUnit(selectedArmies[units[index].GetComponent<UnitButton>().army].units[units[index].GetComponent<UnitButton>().unit]);
        }
        

        
        SelectArmy(selectedArmies);
        if (activeUnits[index] == true)
        {
            activeUnits[index] = false;
            /////unitsCheckBox[index].color = new Color(255f / 255f, 102f / 255f, 80f / 255f);
        }
        else
        {
            activeUnits[index] = true;
            /////unitsCheckBox[index].color = new Color(176f / 255f, 255f / 255f, 145f / 255f);
        }
    }

    public void changeMSMode()
    {
        if (MSMode == 1)
        {
            MSMode = -1;
            modeButonText.text = "DEL";
        }
        else
        {
            MSMode = 1;
            modeButonText.text = "ADD";
        }
    }
}
