using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMenu : MonoBehaviour
{
    public GameObject cityManagementPanel;
    public GameObject armyManagementPanel;
    public GameObject mainUI;
    public UIController uiController;

    // Start is called before the first frame update
    void Start()
    {
        uiController = GameObject.Find("UIController").GetComponent<UIController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadGame()
    {
        ResourceManager.LoadGame("save.json");
        if(cityManagementPanel.activeSelf)
        {
            mainUI.GetComponent<CityManagement>().HideCityManagementPanel();
        }
        if(armyManagementPanel.activeSelf)
        {
            armyManagementPanel.SetActive(false);
        }
        uiController.setDispAreaAvailability(true);
        gameObject.SetActive(false);
    }
    public void SaveGame()
    {
        ResourceManager.SaveGame("save.json");
        uiController.setDispAreaAvailability(true);
        gameObject.SetActive(false);
    }
}
