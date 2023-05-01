using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;

public class GameMenu : MonoBehaviour
{
    public GameObject cityManagementPanel;
    public GameObject armyManagementPanel;
    public GameObject mainUI;
    public UIController uiController;

    public GameObject[] UIPanels;
    public GameObject LoadGameButton;
    public string[] savedGames;
    public GameObject[] savedGameButtons;
    public GameObject buttonsLabel;

    public TMP_InputField saveNameInput;

    // Start is called before the first frame update
    void Start()
    {
        uiController = GameObject.Find("UIController").GetComponent<UIController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void LoadGame(int index)
    {
        ResourceManager.LoadGame(savedGames[index]);
        if(cityManagementPanel.activeSelf)
        {
            mainUI.GetComponent<CityManagement>().HideCityManagementPanel();
        }
        if(armyManagementPanel.activeSelf)
        {
            armyManagementPanel.SetActive(false);
        }
        uiController.setDispAreaAvailability(true);
        HidePanel(0);
        gameObject.SetActive(false);
    }

    public void SaveGame()
    {
        ResourceManager.SaveGame(saveNameInput.text + ".json");
        uiController.setDispAreaAvailability(true);
        HidePanel(1);
        gameObject.SetActive(false);
    }

    public void HidePanel(int index)
    {
        if (index == 0)
        {
            for (int i = 0; i < savedGameButtons.Length; i++)
            {
                Destroy(savedGameButtons[i]);
            }
        }
        UIPanels[index].SetActive(false);
    }

    public void ShowPanel(int index)
    {
        UIPanels[index].SetActive(true);
        if (index == 0)
        {
            ShowLoadPanel();
        }
    }

    public void ShowLoadPanel()
    {
        savedGames = Directory.GetFiles(@".\", "*.json");
        buttonsLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(buttonsLabel.GetComponent<RectTransform>().sizeDelta.x, savedGames.Length*20+20+(10*(savedGames.Length-1)));
        savedGameButtons = new GameObject[savedGames.Length];
        for (int i = 0; i < savedGames.Length; i++)
        {
            savedGameButtons[i] = Instantiate(LoadGameButton, buttonsLabel.transform);
            savedGameButtons[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(0, buttonsLabel.GetComponent<RectTransform>().sizeDelta.y/2-((i+1) * 25));
            savedGameButtons[i].name = i.ToString();
            savedGameButtons[i].transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text= savedGames[i].Substring(2, savedGames[i].Length-7);
        }
    }
}
