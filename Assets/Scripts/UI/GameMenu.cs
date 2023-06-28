using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
using UnityEngine.SceneManagement;

public class GameMenu : MonoBehaviour
{
    public GameController gameController;
    public GameObject cityManagementPanel;
    public GameObject armyManagementPanel;
    public GameObject mainUI;
    public UIController uiController;

    public GameObject[] UIPanels;
    public GameObject loadGameButton;
    public string[] savedGames;
    public GameObject[] savedGameButtons;
    public GameObject deleteGameButton;
    public GameObject[] deleteGameButtons;
    public GameObject buttonsLabel;

    public TMP_InputField saveNameInput;

    // Start is called before the first frame update
    void Start()
    {
        gameController = FindObjectOfType<GameController>();
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
        HidePanel(0);
        gameObject.SetActive(false);
    }

    public void SaveGame()
    {
        if (CheckForDuplicate(saveNameInput.text))
        {
            ResourceManager.SaveGame(gameController.saveFileDirectory + saveNameInput.text + ".json");
            HidePanel(1);
            gameObject.SetActive(false);
            //Debug.Log("File saved");
        }
    }

    public bool CheckForDuplicate(string name)
    {
        savedGames = Directory.GetFiles($"./{gameController.saveFileDirectory}", "*.json");
        for(int i = 0; i < savedGames.Length; i++)
        {
            if (savedGames[i].Substring((2 + gameController.saveFileDirectory.Length), savedGames[i].Length-7 - gameController.saveFileDirectory.Length) == name)
            {
                //Debug.Log("File exists");
                saveNameInput.text = "FILE ALREADY EXISTS";
                return false;
            }
        }
        return true;
    }

    public void HidePanel(int index)
    {
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
        for (int i = 0; i < savedGameButtons.Length; i++)
        {
            Destroy(savedGameButtons[i]);
            Destroy(deleteGameButtons[i]);
        }
        savedGames = Directory.GetFiles($"./{gameController.saveFileDirectory}", "*.json");
        buttonsLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(buttonsLabel.GetComponent<RectTransform>().sizeDelta.x, savedGames.Length*20+20+(10*(savedGames.Length-1)));
        savedGameButtons = new GameObject[savedGames.Length];
        deleteGameButtons = new GameObject[savedGames.Length];
        for (int i = 0; i < savedGames.Length; i++)
        {
            savedGameButtons[i] = Instantiate(loadGameButton, buttonsLabel.transform);
            savedGameButtons[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(-15, buttonsLabel.GetComponent<RectTransform>().sizeDelta.y/2-((i+1) * 25));
            savedGameButtons[i].name = i.ToString();
            savedGameButtons[i].transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text= savedGames[i].Substring((2 + gameController.saveFileDirectory.Length), savedGames[i].Length-7 - gameController.saveFileDirectory.Length);

            deleteGameButtons[i] = Instantiate(deleteGameButton, buttonsLabel.transform);
            deleteGameButtons[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(60, buttonsLabel.GetComponent<RectTransform>().sizeDelta.y / 2 - ((i + 1) * 25));
            deleteGameButtons[i].name = "D"+i.ToString();
        }
    }

    public void DeleteGame(int index)
    {
        Debug.Log(savedGames[index]);
        File.Delete(savedGames[index]);
        ShowLoadPanel();
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu"); 
    }

    public void CloseApp()
    {
        Application.Quit();
    }
}
