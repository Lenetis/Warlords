using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.IO;

public class UltimateMainMenu : MonoBehaviour
{

    public GameObject startUpPanel;

    //0-scennarios, 1-load game, 2-create world
    public GameObject[] panels;
    public Button[] buttons;

    //scenarios
    public GameObject scenariosButtonsLabel;
    public GameObject[] savedScenariosButtons;
    public GameObject[] deleteScenariosButtons;
    
    //loadGame
    public GameObject buttonsLabel;
    public string[] savedGames;
    public GameObject savedGameButton;
    public GameObject[] savedGameButtons;
    public GameObject deleteGameButton;
    public GameObject[] deleteGameButtons;

    //editor
    public TMP_InputField width;
    public TMP_InputField height;

    public void ShowPanel(int index)
    {
        for(int i = 0; i < panels.Length; i++)
        {
            if (index == i)
            {
                panels[i].SetActive(true);
                buttons[i].interactable = false;
            }
            else
            {
                panels[i].SetActive(false);
                buttons[i].interactable = true;
            }
        }
        
        if (index == 0)
        {
            for (int i = 0; i < savedScenariosButtons.Length; i++)
            {
                Destroy(savedScenariosButtons[i]);
                Destroy(deleteScenariosButtons[i]);
            }
            savedGames = Directory.GetFiles($"./Scenarios", "*.json");
            scenariosButtonsLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(scenariosButtonsLabel.GetComponent<RectTransform>().sizeDelta.x, savedGames.Length * 20 + 20 + (10 * (savedGames.Length - 1)));
            savedScenariosButtons = new GameObject[savedGames.Length];
            deleteScenariosButtons = new GameObject[savedGames.Length];
            for (int i = 0; i < savedGames.Length; i++)
            {
                savedScenariosButtons[i] = Instantiate(savedGameButton, scenariosButtonsLabel.transform);
                savedScenariosButtons[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(-15, scenariosButtonsLabel.GetComponent<RectTransform>().sizeDelta.y / 2 - ((i + 1) * 25));
                savedScenariosButtons[i].name = i.ToString();
                savedScenariosButtons[i].transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = savedGames[i].Substring((2 + 10), savedGames[i].Length - 7 - 10);

                deleteScenariosButtons[i] = Instantiate(deleteGameButton, scenariosButtonsLabel.transform);
                deleteScenariosButtons[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(60, scenariosButtonsLabel.GetComponent<RectTransform>().sizeDelta.y / 2 - ((i + 1) * 25));
                deleteScenariosButtons[i].name = "D" + i.ToString();
            }
        }

        if (index == 1)
        {
            for (int i = 0; i < savedGameButtons.Length; i++)
            {
                Destroy(savedGameButtons[i]);
                Destroy(deleteGameButtons[i]);
            }
            savedGames = Directory.GetFiles($"./Saves", "*.json");
            buttonsLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(buttonsLabel.GetComponent<RectTransform>().sizeDelta.x, savedGames.Length * 20 + 20 + (10 * (savedGames.Length - 1)));
            savedGameButtons = new GameObject[savedGames.Length];
            deleteGameButtons = new GameObject[savedGames.Length];
            for (int i = 0; i < savedGames.Length; i++)
            {
                savedGameButtons[i] = Instantiate(savedGameButton, buttonsLabel.transform);
                savedGameButtons[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(-15, buttonsLabel.GetComponent<RectTransform>().sizeDelta.y / 2 - ((i + 1) * 25));
                savedGameButtons[i].name = i.ToString();
                savedGameButtons[i].transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = savedGames[i].Substring((2 + 6), savedGames[i].Length - 7 - 6);

                deleteGameButtons[i] = Instantiate(deleteGameButton, buttonsLabel.transform);
                deleteGameButtons[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(60, buttonsLabel.GetComponent<RectTransform>().sizeDelta.y / 2 - ((i + 1) * 25));
                deleteGameButtons[i].name = "D" + i.ToString();
            }
        }
    }

    public void HidePanel(int index)
    {
        panels[index].SetActive(false);
        buttons[index].interactable = true;
    }

    public void SendData(int index, string mode)
    {
        if (mode == "scenarioMode")
        {
            PlayerPrefs.SetString("saveFileDirectory", "Saves/");
            PlayerPrefs.SetString("startFilePath", savedGames[index]);
            PlayerPrefs.SetString("saveFilePath", "");
        }
        else if (mode == "gameMode")
        {
            PlayerPrefs.SetString("saveFileDirectory", "Saves/");
            PlayerPrefs.SetString("startFilePath", "");
            PlayerPrefs.SetString("saveFilePath", savedGames[index]);
        }
        Debug.Log(savedGames[index]);
        Debug.Log(mode);
        PlayerPrefs.SetString("mode", mode);
        SceneManager.LoadScene("Game");

        
    }

    public void LaunchEditor()
    {
        if(width.text!=""&& height.text != "")
        {
            if (int.TryParse(width.text, out _))
            {
                if (int.TryParse(height.text, out _))
                {
                    //XD it works
                }
                else
                {
                    return;
                }
            }
            else
            {
                return;
            }

            if (int.Parse(width.text) > 0 && int.Parse(height.text) > 0 && int.Parse(width.text) < 200 && int.Parse(height.text) < 200)
            {
                PlayerPrefs.SetString("saveFileDirectory", "Scenarios/");
                PlayerPrefs.SetString("startFilePath", "");
                PlayerPrefs.SetString("saveFilePath", "");

                PlayerPrefs.SetInt("width", int.Parse(width.text));
                PlayerPrefs.SetInt("height", int.Parse(height.text));

                PlayerPrefs.SetString("mode", "editorMode");
                Debug.Log("EditorMode");
                SceneManager.LoadScene("Game");
            }
        }
    }
}
