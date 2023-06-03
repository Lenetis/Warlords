using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CityIconInMenu : MonoBehaviour, IPointerDownHandler
{
    public CityManagement cityManagement;
    public GameController gameController;

    // Start is called before the first frame update
    void Start()
    {
        cityManagement= GameObject.Find("Main").GetComponent<CityManagement>();
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            cityManagement.SelectCity(gameController.cities[int.Parse(gameObject.transform.parent.name)]);
            
        }
    }
}
