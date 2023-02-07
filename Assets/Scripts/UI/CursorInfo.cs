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

    public GameObject displayArea;
    public bool isOverDispArea;

    public Vector3 mousePosTmp;
    public bool isMoved;
    public bool isSaved;

    // Start is called before the first frame update
    void Start()
    {
        mouseSelection = GameObject.Find("Main Camera").GetComponent<MouseSelection>();
    }

    // Update is called once per frame
    void Update()
    {
        //can be done once if using fixed size window
        float dispAreaWidth = (displayArea.GetComponent<RectTransform>().anchorMax.x - displayArea.GetComponent<RectTransform>().anchorMin.x) * Screen.width + displayArea.GetComponent<RectTransform>().sizeDelta.x * gui.GetComponent<Canvas>().scaleFactor;
        float dispAreaHeight = (displayArea.GetComponent<RectTransform>().anchorMax.y - displayArea.GetComponent<RectTransform>().anchorMin.y) * Screen.height + displayArea.GetComponent<RectTransform>().sizeDelta.y * gui.GetComponent<Canvas>().scaleFactor;
        float dispAreaPosX = displayArea.GetComponent<RectTransform>().position.x;
        float dispAreaPosY = displayArea.GetComponent<RectTransform>().position.y;
        float dispAreaOriginX = dispAreaPosX - (dispAreaWidth / 2);
        float dispAreaOriginY = dispAreaPosY - (dispAreaHeight / 2);
        float dispAreaEndX = dispAreaPosX + (dispAreaWidth / 2);
        float dispAreaEndY = dispAreaPosY + (dispAreaHeight / 2);


        if(Input.mousePosition.x>= dispAreaOriginX&& Input.mousePosition.x<= dispAreaEndX)
        {
            if (Input.mousePosition.y >= dispAreaOriginY && Input.mousePosition.y <= dispAreaEndY)
            {
                isOverDispArea = true;
            }
            else
            {
                isOverDispArea = false;
            }
        }
        else
        {
            isOverDispArea = false;
        }

        //Debug.Log(dispAreaOriginX+" "+Input.mousePosition.x);

        if (Input.GetMouseButton(1) && isOverDispArea)
        {
            if (!isSaved)
            {
                mousePosTmp = Input.mousePosition;
                isSaved = true;
            }

            if (Input.mousePosition != mousePosTmp)
            {
                isMoved = true;
            }

            if (!isMoved)
            {
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
        else
        {
            infoPanel.SetActive(false);
            Cursor.visible = true; 
            isSaved = false;
            isMoved = false;
        }
    }
}
