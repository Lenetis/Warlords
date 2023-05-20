using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UnitCityButton : MonoBehaviour, IPointerClickHandler
{
    public CityManagement cityManagement;
    void Start()
    {
        cityManagement = GameObject.Find("Main").GetComponent<CityManagement>();

        if(transform.parent.name == "BuyableUnitsPanel")
        {
            transform.GetChild(1).gameObject.SetActive(true);
        }
        
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if(transform.parent.name== "BuildableUnitsPanel")
            {
                cityManagement.SetUnitProduction(int.Parse(gameObject.name));
            }
            if (transform.parent.name == "BuyableUnitsPanel")
            {
                cityManagement.BuyUnit(int.Parse(gameObject.name));
            }
            else if (transform.parent.name == "ReplaceUnitsPanel")
            {
                cityManagement.ReplaceUnit(int.Parse(gameObject.name));
            }


        }
    }
}
