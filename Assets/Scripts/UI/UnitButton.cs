using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UnitButton : MonoBehaviour, IPointerClickHandler
{
    public ArmyManagement armyManagement;
    void Start()
    {
        armyManagement= GameObject.Find("Main").GetComponent<ArmyManagement>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            armyManagement.setUnitActivity(int.Parse(gameObject.name));
        }
    }
}
