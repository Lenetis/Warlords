using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonRightClick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public CursorInfo cursorInfo;
    public string buttonName;
    public string buttonDescription;
    public int mode=1;

    void Start()
    {
        cursorInfo = GameObject.Find("Main").GetComponent<CursorInfo>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            //Debug.Log("Right click");
            cursorInfo.mode = mode;
            cursorInfo.buttonName = buttonName;
            cursorInfo.buttonDescription = buttonDescription;
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
