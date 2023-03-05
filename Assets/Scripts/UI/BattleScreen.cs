using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleScreen : MonoBehaviour
{
    public GameObject battlePanel;

    public GameObject unitImage;
    public GameObject attackerPanel;
    public GameObject defenderPanel;
    public GameObject[] attackerUnits;
    public GameObject[] defenderUnits;

    public TextMeshProUGUI winInfo;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            battlePanel.SetActive(false);

            for (int i = 0; i < attackerUnits.Length; i++)
            {
                Destroy(attackerUnits[i]);
            }
            
            for (int i = 0; i < defenderUnits.Length; i++)
            {
                Destroy(defenderUnits[i]);
            }
        }
    }

    public void UpdateAttacker(List<Unit> units, int mode)
    {
        if (mode == 1)
        {
            for (int i = 0; i < attackerUnits.Length; i++)
            {
                Destroy(attackerUnits[i]);
            }
        }

        attackerUnits = new GameObject[units.Count];
        for (int i = 0; i < units.Count; i++)
        {
            attackerUnits[i] = Instantiate(unitImage, attackerPanel.transform);
            attackerUnits[i].transform.localPosition = new Vector3((i + 1) * ((attackerPanel.GetComponent<RectTransform>().sizeDelta.x / ((units.Count) + 1))) - (attackerPanel.GetComponent<RectTransform>().sizeDelta.x / 2), 0, 0);
            attackerUnits[i].transform.SetParent(attackerPanel.transform);
            attackerUnits[i].name = i.ToString();

            attackerUnits[i].GetComponent<Image>().sprite = Sprite.Create(units[i].texture, new Rect(0.0f, 0.0f, units[i].texture.width, units[i].texture.height), new Vector2(0.5f, 0.5f), 100.0f);

        }
    }

    public void UpdateDefender(List<Unit> units, int mode)
    {
        if (mode == 1)
        {

            for (int i = 0; i < defenderUnits.Length; i++)
            {
                Destroy(defenderUnits[i]);
            }
        }
        defenderUnits = new GameObject[units.Count];
        for (int i = 0; i < units.Count; i++)
        {
            defenderUnits[i] = Instantiate(unitImage, defenderPanel.transform);
            defenderUnits[i].transform.localPosition = new Vector3((i + 1) * ((defenderPanel.GetComponent<RectTransform>().sizeDelta.x / ((units.Count) + 1))) - (defenderPanel.GetComponent<RectTransform>().sizeDelta.x / 2), 0, 0);
            defenderUnits[i].transform.SetParent(defenderPanel.transform);
            defenderUnits[i].name = i.ToString();

            defenderUnits[i].GetComponent<Image>().sprite = Sprite.Create(units[i].texture, new Rect(0.0f, 0.0f, units[i].texture.width, units[i].texture.height), new Vector2(0.5f, 0.5f), 100.0f);

        }
    }

}
