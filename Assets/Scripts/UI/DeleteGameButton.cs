using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeleteGameButton : MonoBehaviour
{
    public GameMenu gameMenu;
    public UltimateMainMenu ultimateMainMenu;

    // Start is called before the first frame update
    void Start()
    {
        gameMenu  = transform.parent.parent.parent.parent.parent.GetComponent<GameMenu>();
        ultimateMainMenu = transform.parent.parent.parent.parent.parent.GetComponent<UltimateMainMenu>();
    }

    public void Delete()
    {
        
        if (SceneManager.GetActiveScene().name == "Game")
        {
            gameMenu.DeleteGame(int.Parse(gameObject.name.Substring(1)));
        }
        else
        {/*
            if (transform.parent.parent.parent.parent.name == "Scenarios")
            {
                ultimateMainMenu.DeleteGame(int.Parse(gameObject.name.Substring(1)));
            }/*
            else if (transform.parent.parent.parent.parent.name == "LoadGamePanel")
            {
                ultimateMainMenu.DeleteGame(int.Parse(gameObject.name.Substring(1)));
            }*/
            ultimateMainMenu.DeleteGame(int.Parse(gameObject.name.Substring(1)));
        }
    }
}
