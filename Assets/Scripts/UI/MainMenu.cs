using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public GameObject startUpPanel;


    //Random Map
    public GameObject randomWorldPanel;
    public Button randomMapButton;
    public Button[] randomizers = new Button[4];
    public bool[] isRandomized = { false, false, false, false };
    public Slider[] sliders = new Slider[4];
    public TextMeshProUGUI[] percentage = new TextMeshProUGUI[4];
    public TextMeshProUGUI[] extras = new TextMeshProUGUI[2];
    public Dictionary<int, string> terrainTypes = new Dictionary<int, string>{
            { 0, "Grassland" },
            { 1, "Iceland" },
            { 2, "Otherland" },
            { 3, "PiotraDom" }
        };
    public int currentTerrainType=0;
    public bool alliesProduction = false;

    //Load Game
    public GameObject loadGamePanel;
    public Button loadGameButton;

    //Begin
    public GameObject beginPanel;
    public int difficulty=0;
    public Button[] difficultySelectors = new Button[3];
    public TextMeshProUGUI difficultyRating;

    public Dictionary<int, string> playerTypes = new Dictionary<int, string>{
            { 0, "Human" },
            { 1, "Knight" },
            { 2, "Lord" },
            { 3, "Warlord" },
            { 4, "Off" }
        };
    public int[] currentplayerType = new int[8];
    public TextMeshProUGUI[] playerTypeText = new TextMeshProUGUI[8];
    public Sprite[] playerTypeSprites = new Sprite[5];
    public Image[] playerTypeImage = new Image[8];

    //Game Options

    
    public GameObject gameOptionsPanel;
    public GameObject raycastBlocker;

    /*TODO
    public Dictionary<int, string> neutralCities = new Dictionary<int, string>{
            { 0, "Average" },
            { 1, "Strong" },
            { 2, "Active" }
        };

    public bool quests=false;
    public bool viewEnemies = true;
    public bool diplomacy = false;
    public bool hiddenMap = false;
    public bool viewProduction = true;

    public bool intenseCombat = false;
    public bool militaryAdvisor = true;
    public bool quickStart = false;
    public bool randomTurns = false;
    */

    //Audio

    public AudioSource audioSource;
    public bool isPlayingMusic = false;
    public float counter;

    // Start is called before the first frame update
    void Start()
    {


    }

    // Update is called once per frame
    void Update()
    {
        counter += Time.deltaTime;
        if (counter >= 5 && isPlayingMusic == false)
        {
            audioSource.GetComponent<AudioSource>().Play();
            isPlayingMusic = true;
        }

    }


    public void ShowRandomMapPanel()
    {
        randomWorldPanel.SetActive(true);
        startUpPanel.SetActive(false);
        loadGamePanel.SetActive(false);
        randomMapButton.GetComponent<Button>().interactable = false;
        loadGameButton.GetComponent<Button>().interactable = true;
    }

    public void Randomize(int type)
    {
        if (isRandomized[type] == false)
        {
            randomizers[type].GetComponent<Image>().color = Color.cyan;
            sliders[type].GetComponent<Slider>().interactable = false;
            percentage[type].text = "(?)";
            isRandomized[type] = true;
        }
        else
        {
            randomizers[type].GetComponent<Image>().color = Color.white;
            sliders[type].GetComponent<Slider>().interactable = true;
            SetPercentage(type);
            isRandomized[type] = false;
        }
    }

    public void SetPercentage(float i)
    {
        if (i == 2)
        {
            percentage[(int)i].text = "("+(sliders[(int)i].value).ToString()+")";
        }
        else
        {
            percentage[(int)i].text = "(" + (sliders[(int)i].value).ToString() + "%)";
        }
        
    }

    public void SetTerrainType()
    {
        if (currentTerrainType < 3)
        {
            currentTerrainType++;
            extras[0].text = "Terrain type: " + terrainTypes[currentTerrainType];
        }
        else
        {
            currentTerrainType=0;
            extras[0].text = "Terrain type: " + terrainTypes[currentTerrainType];
        }
        

    }

    public void SetAlliesProduction()
    {
        if (alliesProduction==false)
        {
         
            extras[1].text = "Cities can produce allies: Yes";
            alliesProduction = true;
        }
        else
        {

            extras[1].text = "Cities can produce allies: No";
            alliesProduction = false;
        }


    }

    public void ShowLoadGamePanel()
    {
        loadGamePanel.SetActive(true);
        randomWorldPanel.SetActive(false);
        startUpPanel.SetActive(false);
        loadGameButton.GetComponent<Button>().interactable = false;
        randomMapButton.GetComponent<Button>().interactable = true;

    }

    public void HideLoadGamePanel()
    {
        loadGamePanel.SetActive(false);
        startUpPanel.SetActive(true);
        loadGameButton.GetComponent<Button>().interactable = true;

    }

    public void ShowBeginPanel()
    {
        beginPanel.SetActive(true);

    }

    public void ShowMainMenuPanel()
    {
        beginPanel.SetActive(false);

    }

    public void Begin()
    {
        SceneManager.LoadScene(1); 

    }

    public void SelectDifficulty(int value)
    {
        difficulty = value;


        //temporary formula
        if (difficulty == 0)
        {
            difficultyRating.text = "Difficulty Rating 79%";
        }else if (difficulty == 1)
        {
            difficultyRating.text = "Difficulty Rating 91%";
        }
        else
        {
            difficultyRating.text = "Difficulty Rating 100%";
        }

        Debug.Log(difficulty);
        for (int i = 0; i< 3; i++)
        {
            if (i == difficulty)
            {
                difficultySelectors[i].GetComponent<Button>().interactable = false;
            }
            else
            {
                difficultySelectors[i].GetComponent<Button>().interactable = true;
            }
        }

    }

    public void SelectPlayerType(int value)
    {

        if (currentplayerType[value] < 4)
        {
            currentplayerType[value]++;
            
        }
        else
        {
            currentplayerType[value] = 0;
            
        }

        playerTypeText[value].text = playerTypes[currentplayerType[value]];
        playerTypeImage[value].sprite = playerTypeSprites[currentplayerType[value]];
        Debug.Log(currentplayerType[value]);
    }

    public void IAmTheGreatest()
    {

        for(int i = 0; i < 8; i++)
        {
            playerTypeText[i].text = playerTypes[3];
            playerTypeImage[i].sprite = playerTypeSprites[3];
        }
        
    }

    public void ShowGameOptionsPanel()
    {
        gameOptionsPanel.SetActive(true);
        raycastBlocker.SetActive(true);

    }

    public void HideGameOptionsPanel()
    {
        gameOptionsPanel.SetActive(false);
        raycastBlocker.SetActive(false);

    }

}
