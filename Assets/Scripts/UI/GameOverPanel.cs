using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverPanel : MonoBehaviour
{
    public GameObject panel;

    // Start is called before the first frame update
    void Start()
    {
        EventManager.GameWonEvent += ShowPanel;
    }

    private void OnDestroy()
    {
        EventManager.GameWonEvent -= ShowPanel;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowPanel(object sender, System.EventArgs args)
    {
        panel.SetActive(true);
    }

    public void Exit()
    {
        //todo
    }
}
