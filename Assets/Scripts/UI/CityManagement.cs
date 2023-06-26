using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public GameObject replaceableUnitsPanel;
    public GameObject[] replaceableUnits;
    public Image[] replaceableUnitsImage;
    private int buyIndex;

    public TextMeshProUGUI purchaseWarning;

    //CI

    public TMP_InputField cityNameInput;
    public TextMeshProUGUI cityDescripton;

    public UIController uiController;
    public GameController gameController;
    public Minimap minimap;

    // Start is called before the first frame update
    void Start()
    {
        uiController=GameObject.Find("UIController").GetComponent<UIController>();
        gameController = FindObjectOfType<GameController>();
        minimap = GameObject.Find("Main").GetComponent<Minimap>();
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
            if (selectedCity.owner == gameController.activePlayer)
            {
                for (int i = 1; i < navButtons.Length; i++)
                {
                    navButtons[i].interactable = true;
                }
            }
            else
            {
                for (int i = 1; i < navButtons.Length; i++)
                {
                    navButtons[i].interactable = false;
                }

                ShowCityPanel(0);
            }
            

            if (selectedCity.producing)
            {
                productionTurnsLeft.text = (selectedCity.productionProgress).ToString()+"/"+(selectedCity.producedUnit.productionCost).ToString()+"t";
                currentUnitImage.GetComponent<Image>().sprite = Sprite.Create(selectedCity.producedUnit.texture, new Rect(0.0f, 0.0f, selectedCity.producedUnit.texture.width, selectedCity.producedUnit.texture.height), new Vector2(0.5f, 0.5f), 100.0f);
                currentUnitImage.SetActive(true);
                unitCityStats[0].text = selectedCity.producedUnit.name;
                unitCityStats[1].text = "Time: "+selectedCity.producedUnit.productionCost.ToString();
                unitCityStats[2].text = "Cost: " + selectedCity.producedUnit.economy.upkeep.ToString();
                unitCityStats[3].text = "Strength: " + selectedCity.producedUnit.battleStats.strength.ToString();
                unitCityStats[4].text = "Move: " + selectedCity.producedUnit.pathfinder.move.ToString();
                unitCityStatsPanel.SetActive(true);
            }
            else
            {
                productionTurnsLeft.text = "-";
                currentUnitImage.SetActive(false);
                unitCityStatsPanel.SetActive(false);
            }

            ShowBuildableUnits();

            //CB

            cityIncome.text = "Income: " + selectedCity.economy.income.ToString() + " Gold";
            cityDefence.text = "Defence: " + selectedCity.battleStats.strength;
            cityOwner.text = "Owner: " + selectedCity.owner.name.ToString();

            for (int j = 0; j < buyableUnits.Length; j++)
            {
                Destroy(buyableUnits[j]);
            }

            int buyableSize = selectedCity.buyableUnits.Count;
            //int armySize = mouseSelection.selectedArmy.units.Count;

            //Debug.Log("Buyable");

            buyableUnits = new GameObject[buyableSize];
            buyableUnitsImage = new Image[buyableSize];

            int row = 0;
            int column = 0;
            if (selectedCity.buyableUnits.Count > 15)
            {
                int height = selectedCity.buyableUnits.Count / 5 * 50;

                if (selectedCity.buyableUnits.Count % 5 > 0){
                    height += 50;
                }
                buyableUnitsPanel.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            }
            
            for (int i = 0; i < buyableSize; i++)
            {
                if (column == 5)
                {
                    row++;
                    column = 0;
                }

                if (buyableUnits[i] == null)
                {
                    buyableUnits[i] = Instantiate(unitCityButton, buyableUnitsPanel.transform);
                    buyableUnits[i].transform.localPosition = new Vector3((column + 1) * ((buyableUnitsPanel.GetComponent<RectTransform>().sizeDelta.x / ((5) + 1))), (150/2-100)-(row*50/ buyableUnitsPanel.GetComponent<RectTransform>().sizeDelta.y)* buyableUnitsPanel.GetComponent<RectTransform>().sizeDelta.y, 0);
                    buyableUnits[i].transform.SetParent(buyableUnitsPanel.transform);
                    buyableUnits[i].name = i.ToString();
                    buyableUnits[i].GetComponent<ButtonRightClick>().buttonName = selectedCity.buyableUnits[i].name;
                    buyableUnits[i].GetComponent<ButtonRightClick>().buttonDescription[0] = selectedCity.buyableUnits[i].battleStats.strength.ToString();
                    buyableUnits[i].GetComponent<ButtonRightClick>().buttonDescription[1] = selectedCity.buyableUnits[i].pathfinder.move.ToString();
                    buyableUnits[i].GetComponent<ButtonRightClick>().buttonDescription[2] = selectedCity.buyableUnits[i].productionCost.ToString();
                    buyableUnits[i].GetComponent<ButtonRightClick>().buttonDescription[3] = selectedCity.buyableUnits[i].economy.upkeep.ToString();

                    buyableUnitsImage[i] = buyableUnits[i].transform.GetChild(0).gameObject.GetComponent<Image>();
                    buyableUnitsImage[i].sprite = Sprite.Create(selectedCity.buyableUnits[i].texture, new Rect(0.0f, 0.0f, selectedCity.buyableUnits[i].texture.width, selectedCity.buyableUnits[i].texture.height), new Vector2(0.5f, 0.5f), 100.0f);

                    buyableUnits[i].transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = selectedCity.buyableUnits[i].purchaseCost.ToString() + "gp";
                }
                column++;
                
            }

            cityPanels[5].transform.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>().text = "Are you sure that you\nwant to\nraze " + selectedCity.name + "?\nYou won't be popular!";
        }
        minimap.selectedCity = selectedCity;
        minimap.DrawCities(this, System.EventArgs.Empty);
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
        buyIndex = index;

        //Debug.Log("count: " + selectedCity.buyableUnits.Count);
        if (selectedCity.buildableUnits.Count >= 4)
        {
            if (!selectedCity.buildableUnits.Any(buildableUnit => buildableUnit.baseFile == selectedCity.buyableUnits[buyIndex].baseFile) && selectedCity.owner.gold >= selectedCity.buyableUnits[buyIndex].purchaseCost)
            {
                ShowCityPanel(8);

                int replaceableSize = selectedCity.buildableUnits.Count;

                replaceableUnits = new GameObject[replaceableSize];
                replaceableUnitsImage = new Image[replaceableSize];

                for (int i = 0; i < replaceableSize; i++)
                {
                    if (replaceableUnits[i] == null)
                    {
                        replaceableUnits[i] = Instantiate(unitCityButton, replaceableUnitsPanel.transform);
                        replaceableUnits[i].transform.localPosition = new Vector3((i + 1) * ((replaceableUnitsPanel.GetComponent<RectTransform>().sizeDelta.x / ((replaceableSize) + 1))) - (replaceableUnitsPanel.GetComponent<RectTransform>().sizeDelta.x / 2), 0, 0);
                        replaceableUnits[i].transform.SetParent(replaceableUnitsPanel.transform);
                        replaceableUnits[i].name = i.ToString();

                        replaceableUnitsImage[i] = replaceableUnits[i].transform.GetChild(0).gameObject.GetComponent<Image>();
                        replaceableUnitsImage[i].sprite = Sprite.Create(selectedCity.buildableUnits[i].texture, new Rect(0.0f, 0.0f, selectedCity.buildableUnits[i].texture.width, selectedCity.buildableUnits[i].texture.height), new Vector2(0.5f, 0.5f), 100.0f);
                    }
                }
            }
            else
            {
                string warning="";

                if (selectedCity.owner.gold < selectedCity.buyableUnits[buyIndex].purchaseCost)
                {
                    warning = "More gold required";
                }
                if (selectedCity.buildableUnits.Any(buildableUnit => buildableUnit.baseFile == selectedCity.buyableUnits[buyIndex].baseFile))
                {
                    warning = "Cannot buy a unit that has already been bought";
                }
                purchaseWarning.text = warning;
            }
            
        }
        else
        {
            if (!selectedCity.buildableUnits.Any(buildableUnit => buildableUnit.baseFile == selectedCity.buyableUnits[buyIndex].baseFile) && selectedCity.owner.gold >= selectedCity.buyableUnits[buyIndex].purchaseCost)
            {
                selectedCity.BuyUnit(selectedCity.buyableUnits[index], 3);
                ShowCityPanel(2);
            }
            else
            {
                string warning = "";

                if (selectedCity.owner.gold < selectedCity.buyableUnits[buyIndex].purchaseCost)
                {
                    warning = "More gold required";
                }
                if (selectedCity.buildableUnits.Any(buildableUnit => buildableUnit.baseFile == selectedCity.buyableUnits[buyIndex].baseFile))
                {
                    warning = "Cannot buy a unit that has already been bought";
                }
                purchaseWarning.text = warning;
            }
        }
    }

    public void ReplaceUnit(int index)
    {
        selectedCity.BuyUnit(selectedCity.buyableUnits[buyIndex], index);

        for (int i = 0; i < replaceableUnits.Length; i++)
        {
            Destroy(replaceableUnits[i]);
        }

        ShowCityPanel(2);
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

            if(index == 2)
            {
                SelectCity(selectedCity);
            }

            if (index == 4)
            {
                purchaseWarning.text = "";
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

    public void ShowBuildableUnits()
    {
        for (int j = 0; j < buildableUnits.Length; j++)
        {
            Destroy(buildableUnits[j]);
        }

        int buildableSize = selectedCity.buildableUnits.Count;
        //int armySize = mouseSelection.selectedArmy.units.Count;

        buildableUnits = new GameObject[buildableSize];
        buildableUnitsImage = new Image[buildableSize];
        //Debug.Log("Buldable");
        for (int i = 0; i < buildableSize; i++)
        {
            if (buildableUnits[i] == null)
            {
                //Debug.Log(buildableUnits[i]);
                buildableUnits[i] = Instantiate(unitCityButton, buildableUnitsPanel.transform);
                buildableUnits[i].transform.localPosition = new Vector3((i + 1) * ((buildableUnitsPanel.GetComponent<RectTransform>().sizeDelta.x / ((buildableSize) + 1))) - (buildableUnitsPanel.GetComponent<RectTransform>().sizeDelta.x / 2), 0, 0);
                buildableUnits[i].transform.SetParent(buildableUnitsPanel.transform);
                buildableUnits[i].name = i.ToString();
                buildableUnits[i].GetComponent<ButtonRightClick>().buttonName = selectedCity.buildableUnits[i].name;
                buildableUnits[i].GetComponent<ButtonRightClick>().buttonDescription[0] = selectedCity.buildableUnits[i].battleStats.strength.ToString();
                buildableUnits[i].GetComponent<ButtonRightClick>().buttonDescription[1] = selectedCity.buildableUnits[i].pathfinder.move.ToString();
                buildableUnits[i].GetComponent<ButtonRightClick>().buttonDescription[2] = selectedCity.buildableUnits[i].productionCost.ToString();
                buildableUnits[i].GetComponent<ButtonRightClick>().buttonDescription[3] = selectedCity.buildableUnits[i].economy.upkeep.ToString();

                buildableUnitsImage[i] = buildableUnits[i].transform.GetChild(0).gameObject.GetComponent<Image>();
                buildableUnitsImage[i].sprite = Sprite.Create(selectedCity.buildableUnits[i].texture, new Rect(0.0f, 0.0f, selectedCity.buildableUnits[i].texture.width, selectedCity.buildableUnits[i].texture.height), new Vector2(0.5f, 0.5f), 100.0f);
            }
        }
    }

}
