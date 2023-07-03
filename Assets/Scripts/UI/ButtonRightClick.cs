using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonRightClick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public CursorInfo cursorInfo;
    public string buttonName;
    public string[] buttonDescription=new string[4];
    public int mode=1;
    public int dispMode;

    void Start()
    {
        cursorInfo = GameObject.Find("Main").GetComponent<CursorInfo>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Right && (transform.parent.name=="BuildableUnitsPanel" || transform.parent.name == "BuyableUnitsPanel" || transform.parent.name== "UnitsField"))
        {
            cursorInfo.mode = mode;
            cursorInfo.dispMode = 0;
            cursorInfo.buttonName = buttonName;
            cursorInfo.buttonDescription[0] = buttonDescription[0];
            cursorInfo.buttonDescription[1] = buttonDescription[1];
            cursorInfo.buttonDescription[2] = buttonDescription[2];
            cursorInfo.buttonDescription[3] = buttonDescription[3];
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            //Debug.Log("Right click");
            cursorInfo.mode = mode;
            cursorInfo.dispMode = dispMode;
            cursorInfo.buttonName = buttonName;
            cursorInfo.buttonDescription[0] = buttonDescription[0];
            cursorInfo.buttonDescription[1] = buttonDescription[1];
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            //Debug.Log("Right click");
            cursorInfo.mode = 0;
        }
    }
}
