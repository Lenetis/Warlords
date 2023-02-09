using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CityManagement : MonoBehaviour
{
    public MouseSelection mouseSelection;

    public GameObject cityManagementPanel;
    public GameObject armyManagementPanel;

    public TextMeshProUGUI cityName;

    // Start is called before the first frame update
    void Start()
    {
        mouseSelection = GameObject.Find("Main Camera").GetComponent<MouseSelection>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (mouseSelection.highlightedTile.contents.armies != null)
            {
                Debug.Log("Army");
                armyManagementPanel.SetActive(true);
            }
            else if (mouseSelection.highlightedTile.contents.city != null)
            {
                cityName.text = "City";
                cityManagementPanel.SetActive(true);
            }
            else
            {
                Debug.Log("Terrain");
            }
        }
    }

    public void HideCityManagementPanel()
    {
        cityManagementPanel.SetActive(false);
    }
    public void HideArmyManagementPanel()
    {
        armyManagementPanel.SetActive(false);
    }

}
