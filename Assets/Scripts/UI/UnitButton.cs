using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UnitButton : MonoBehaviour, IPointerClickHandler
{
    public ArmyManagement armyManagement;
    public int army;
    public int unit;
    void Start()
    {
        armyManagement= GameObject.Find("Main").GetComponent<ArmyManagement>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            armyManagement.SetUnitActivity(int.Parse(gameObject.name));
        }
    }
}
