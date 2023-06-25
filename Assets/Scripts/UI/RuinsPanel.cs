using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RuinsPanel : MonoBehaviour
{
    public GameObject ruinsPanel;
    public List<string> info;
    public TextMeshProUGUI[] infoString;
    public GameObject doneButton;

    public int infoRemaining=2;

    // Start is called before the first frame update
    void Start()
    {
        EventManager.RuinsInfoEvent += ShowRuinsPanel;
    }

    private void OnDestroy()
    {
        EventManager.RuinsInfoEvent -= ShowRuinsPanel;
    }

    // Update is called once per frame
    void Update()
    {
        if (info.Count != 1)
        {
            if (Input.GetMouseButtonUp(0) && ruinsPanel.activeSelf)
            {
                infoRemaining--;
            }

            if (infoRemaining == 1)
            {
                if (infoString[1].text == "")
                {
                    infoString[1].text = info[1];
                    if (info.Count == 2)
                    {
                        doneButton.SetActive(true);
                    }
                }
            }
            else if (infoRemaining == 0 && info.Count == 3)
            {
                if (infoString[2].text == "")
                {
                    infoString[2].text = info[2];
                    doneButton.SetActive(true);
                }
            }
        }
        
    }

    public void ShowRuinsPanel(object sender, RuinsInfoData eventData)
    {
        infoRemaining = 2;
        for (int i=0; i < infoString.Length; i++)
        {
            infoString[i].text = "";
        }

        info = eventData.explorationInfo;
        infoString[0].text = info[0];

        if (info.Count == 1)
        {
            doneButton.SetActive(true);
        }
        else
        {
            doneButton.SetActive(false);
        }
        ruinsPanel.SetActive(true);
    }

    public void ClosePanel()
    {
        ruinsPanel.SetActive(false);
    }
}
