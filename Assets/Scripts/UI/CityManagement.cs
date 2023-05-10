using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CityManagement : MonoBehaviour
{
    public GameObject[] cityPanels =new GameObject[4];
    public Button[] navButtons = new Button[4];
    private City selectedCity;

    //main, CP
    public GameObject cityManagementPanel;
    public TextMeshProUGUI cityName;
    public GameObject buildableUnitsPanel;
    public GameObject unitCityButton;
    public GameObject[] buildableUnits;
    public Image[] buildableUnitsImage;

    public GameObject currentUnitImage;
    public TextMeshProUGUI productionTurnsLeft;
    public TextMeshProUGUI[] unitCityStats = new TextMeshProUGUI[5];
    public GameObject unitCityStatsPanel;

    //CB
    public TextMeshProUGUI cityIncome;
    public TextMeshProUGUI cityDefence;
    public TextMeshProUGUI cityOwner;

    public GameObject buyableUnitsPanel;
    public GameObject[] buyableUnits;
    public Image[] buyableUnitsImage;

    //CI

    public TMP_InputField cityNameInput;
    public TextMeshProUGUI cityDescripton;

    public UIController uiController;

    // Start is called before the first frame update
    void Start()
    {
        uiController=GameObject.Find("UIController").GetComponent<UIController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SelectCity(City selectedCity)
    {
        this.selectedCity = selectedCity;
        cityName.text = selectedCity.name;
        cityDescripton.text = selectedCity.description;
        

        if (selectedCity.razed)
        {
            cityIncome.text = "Income: 0 Gold";
            cityDefence.text = "Defence: 0";
            cityOwner.text = "Owner: Razed City";

            for(int i = 1; i < navButtons.Length; i++)
            {
                navButtons[i].interactable = false;
            }

            ShowCityPanel(0);
        }
        else
        {
            for (int i = 1; i < navButtons.Length; i++)
            {
                navButtons[i].interactable = true;
            }

            if (selectedCity.producing)
            {
                productionTurnsLeft.text = (selectedCity.productionProgress).ToString()+"/"+(selectedCity.producedUnit.productionCost).ToString()+"t";
                currentUnitImage.GetComponent<Image>().sprite = Sprite.Create(selectedCity.producedUnit.texture, new Rect(0.0f, 0.0f, selectedCity.producedUnit.texture.width, selectedCity.producedUnit.texture.height), new Vector2(0.5f, 0.5f), 100.0f);
                currentUnitImage.SetActive(true);
                unitCityStats[0].text = selectedCity.producedUnit.name;
                unitCityStats[1].text = "Time: "+selectedCity.producedUnit.productionCost.ToString();
                unitCityStats[2].text = "Cost: " + selectedCity.producedUnit.upkeep.ToString();
                unitCityStats[3].text = "Strength: " + selectedCity.producedUnit.strength.ToString();
                unitCityStats[4].text = "Move: " + selectedCity.producedUnit.move.ToString();
                unitCityStatsPanel.SetActive(true);

            }
            else
            {
                productionTurnsLeft.text = "-";
                currentUnitImage.SetActive(false);
                unitCityStatsPanel.SetActive(false);
            }

            int buildableSize = selectedCity.buildableUnits.Count;
            //int armySize = mouseSelection.selectedArmy.units.Count;

            if(buildableSize> buildableUnits.Length)
            {
                buildableUnits = new GameObject[buildableSize];
                buildableUnitsImage = new Image[buildableSize];
            }


            for (int i = 0; i < buildableSize; i++)
            {
                if (buildableUnits[i] == null) {
                    //Debug.Log(buildableUnits[i]);
                    buildableUnits[i] = Instantiate(unitCityButton, buildableUnitsPanel.transform);
                    buildableUnits[i].transform.localPosition = new Vector3((i + 1) * ((buildableUnitsPanel.GetComponent<RectTransform>().sizeDelta.x / ((buildableSize) + 1))) - (buildableUnitsPanel.GetComponent<RectTransform>().sizeDelta.x / 2), 0, 0);
                    buildableUnits[i].transform.SetParent(buildableUnitsPanel.transform);
                    buildableUnits[i].name = i.ToString();

                    buildableUnitsImage[i] = buildableUnits[i].transform.GetChild(0).gameObject.GetComponent<Image>();
                    buildableUnitsImage[i].sprite = Sprite.Create(selectedCity.buildableUnits[i].texture, new Rect(0.0f, 0.0f, selectedCity.buildableUnits[i].texture.width, selectedCity.buildableUnits[i].texture.height), new Vector2(0.5f, 0.5f), 100.0f);
                }
                    
            }

            //CB

            cityIncome.text = "Income: " + selectedCity.income.ToString() + " Gold";
            cityDefence.text = "Defence: --";
            cityOwner.text = "Owner: " + selectedCity.owner.name.ToString();

            int buyableSize = selectedCity.buyableUnits.Count;
            //int armySize = mouseSelection.selectedArmy.units.Count;

            if (buyableSize > buyableUnits.Length)
            {
                buyableUnits = new GameObject[buyableSize];
                buyableUnitsImage = new Image[buyableSize];
            }

            for (int i = 0; i < buyableSize; i++)
            {
                if (buyableUnits[i] == null)
                {
                    buyableUnits[i] = Instantiate(unitCityButton, buyableUnitsPanel.transform);
                    buyableUnits[i].transform.localPosition = new Vector3((i + 1) * ((buyableUnitsPanel.GetComponent<RectTransform>().sizeDelta.x / ((buyableSize) + 1))) - (buyableUnitsPanel.GetComponent<RectTransform>().sizeDelta.x / 2), 0, 0);
                    buyableUnits[i].transform.SetParent(buyableUnitsPanel.transform);
                    buyableUnits[i].name = i.ToString();

                    buyableUnitsImage[i] = buyableUnits[i].transform.GetChild(0).gameObject.GetComponent<Image>();
                    buyableUnitsImage[i].sprite = Sprite.Create(selectedCity.buyableUnits[i].texture, new Rect(0.0f, 0.0f, selectedCity.buyableUnits[i].texture.width, selectedCity.buyableUnits[i].texture.height), new Vector2(0.5f, 0.5f), 100.0f);

                    buyableUnits[i].transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = selectedCity.buyableUnits[i].purchaseCost.ToString() + "gp";
                }
                
            }

            cityPanels[5].transform.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>().text = "Are you sure that you\nwant to\nraze " + selectedCity.name + "?\nYou won't be popular!";
        }
        cityManagementPanel.SetActive(true);
    }

    public void HideCityManagementPanel()
    {
        cityManagementPanel.SetActive(false);
        for(int i = 0; i < buildableUnits.Length; i++)
        {
            Destroy(buildableUnits[i]);
        }

        for (int i = 0; i < buyableUnits.Length; i++)
        {
            Destroy(buyableUnits[i]);
        }
    }
    public void SetUnitProduction(int index)
    {
        Debug.Log("Production: " + index);
        selectedCity.producedUnit = selectedCity.buildableUnits[index];
        //selectedCity.producing = true;
        SelectCity(selectedCity);
    }

    public void BuyUnit(int index)
    {
        Debug.Log("Purchase: " + index);
        selectedCity.BuyUnit(selectedCity.buyableUnits[index], 3);
        // todo add ability to replace a specific unit instead of the last one (replace 3 with the correct index)
        // also todo, refresh the units in the city production panel
        // also also todo, refresh the amount of gold in UI after the unit has been bought
    }

    public void ShowCityPanel(int index)
    {
        cityPanels[index].SetActive(true);
        for(int i = 0; i < cityPanels.Length; i++)
        {
            if (i != index && index!=5)
            {
                //Debug.Log(i);
                cityPanels[i].SetActive(false);
            }

            if (index == 0 || index == 1)
            {
                cityPanels[6].SetActive(true);
            }

            if (index == 7)
            {
                cityPanels[1].SetActive(true);
            }
            
        }
    }

    public void Raze()
    {
        selectedCity.Raze();
        //Debug.Log("City razed");
        SelectCity(selectedCity);
    }

    public void Rename()
    {
        selectedCity.name = cityNameInput.text;
        cityPanels[7].SetActive(false);
        SelectCity(selectedCity);
    }

    public void StopProduction()
    {
        selectedCity.producing = false;
        SelectCity(selectedCity);
    }

}
