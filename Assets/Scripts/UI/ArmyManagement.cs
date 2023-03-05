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
    {
        if (mouseSelection.selectedArmy != null) {
            for (int i = 0; i < mouseSelection.selectedArmy.units.Count; i += 1) {
                if (movesAvailable[i].text != mouseSelection.selectedArmy.units[i].remainingMove.ToString()) {
                    movesAvailable[i].text = mouseSelection.selectedArmy.units[i].remainingMove.ToString();
                }
            }
        }
    }

    public void SelectArmy(Army selectedArmy)
    {
        this.selectedArmy = selectedArmy;
        if (armyManagementPanel.activeSelf)
        {
            DeselectArmy();
        }
        int armySize = mouseSelection.selectedArmy.units.Count;
        //int armySize = mouseSelection.selectedArmy.units.Count;
        units = new GameObject[armySize];
        unitsImage = new Image[armySize]; 
        unitsCheckBox = new Image[armySize]; 
        activeUnits = new bool[armySize]; 
        movesAvailable = new TextMeshProUGUI[armySize];

        armyManagementPanel.SetActive(true);
        for(int i = 0; i < armySize; i++)
        {
            units[i]=Instantiate(unitButton, armyManagementPanel.transform);
            units[i].transform.localPosition = new Vector3((i+1)*((armyManagementPanel.GetComponent<RectTransform>().sizeDelta.x/((armySize)+1))) - (armyManagementPanel.GetComponent<RectTransform>().sizeDelta.x / 2), 10, 0);
            units[i].transform.SetParent(armyManagementPanel.transform);
            units[i].name = i.ToString();

            unitsImage[i] = units[i].transform.GetChild(1).gameObject.GetComponent<Image>();
            unitsImage[i].sprite = Sprite.Create(mouseSelection.selectedArmy.units[i].texture, new Rect(0.0f, 0.0f, mouseSelection.selectedArmy.units[i].texture.width, mouseSelection.selectedArmy.units[i].texture.height), new Vector2(0.5f, 0.5f), 100.0f);

            movesAvailable[i] = units[i].transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
            movesAvailable[i].text = mouseSelection.selectedArmy.units[i].remainingMove.ToString();

            unitsCheckBox[i] = units[i].transform.GetChild(2).gameObject.GetComponent<Image>();
            unitsCheckBox[i].color = new Color(176f / 255f, 255f / 255f, 145f / 255f);
            
            activeUnits[i] = true;
            //Debug.Log(mouseSelection.highlightedTile.contents.armies[0].owner.armies.Count); 
        }
    }

    public void DeselectArmy()
    {
        armyManagementPanel.SetActive(false);
        for(int i = 0; i < units.Length; i++)
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
