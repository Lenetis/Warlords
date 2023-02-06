using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CursorInfo : MonoBehaviour
{
    public GameObject infoPanel;
    public GameObject gui;

    public MouseSelection mouseSelection;

    public TextMeshProUGUI objectName;
    public TextMeshProUGUI objectDescription;

    // Start is called before the first frame update
    void Start()
    {
        mouseSelection = GameObject.Find("Main Camera").GetComponent<MouseSelection>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(1)){
            objectName.text = mouseSelection.highlightedTile.data.name;
            objectDescription.text = mouseSelection.highlightedTile.data.description;
            //Debug.Log(gameObject.GetComponent<RectTransform>().anchoredPosition.x * gui.GetComponent<Canvas>().scaleFactor);
            infoPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(Input.mousePosition.x - Screen.width / 2, Input.mousePosition.y - Screen.height / 2) / gui.GetComponent<Canvas>().scaleFactor;
            Cursor.visible = false;
            infoPanel.SetActive(true);
        }
        else
        {
            infoPanel.SetActive(false);
            Cursor.visible = true;
        }

        

    }
}
