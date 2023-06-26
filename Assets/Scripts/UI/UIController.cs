using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public GameObject cityManagement;
    public GameObject gameMenu;
    public GameObject battleScreen;
    public GameObject turnInfo;
    public GameObject heroPanel;
    public GameObject inventoryPanel;
    public GameObject ruinsPanel;
    public GameObject gameOverPanel;
    public bool isOverMinimap;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool controllsAvailable()
    {
        if(!gameMenu.activeSelf && !cityManagement.activeSelf && !battleScreen.activeSelf && !turnInfo.activeSelf && !heroPanel.activeSelf && !inventoryPanel.activeSelf && !ruinsPanel.activeSelf &&gameOverPanel.activeSelf)
        {
            return true;
        }

        return false;
    }
}
