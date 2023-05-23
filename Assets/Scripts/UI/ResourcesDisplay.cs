using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResourcesDisplay : MonoBehaviour
{
    public TextMeshProUGUI[] resources = new TextMeshProUGUI[4];

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateResources(int cities, int gold, int income, int upkeep)
    {
        resources[0].text = "Noc: "+cities.ToString();
        resources[1].text = "YT: " + gold.ToString();
        resources[2].text = "YI: " + income.ToString();
        resources[3].text = "YU: " + upkeep.ToString();
        Debug.Log("Resources updated");
    }
}
