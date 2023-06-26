using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadGameButton : MonoBehaviour
{
    public GameMenu gameMenu;
    public UltimateMainMenu ultimateMainMenu;

    // Start is called before the first frame update
    void Start()
    {
        gameMenu  = transform.parent.parent.parent.parent.parent.GetComponent<GameMenu>();
        ultimateMainMenu = transform.parent.parent.parent.parent.parent.GetComponent<UltimateMainMenu>();
    }

    public void Load()
    {
        if (SceneManager.GetActiveScene().name == "Game")
        {
            gameMenu.LoadGame(int.Parse(gameObject.name));
        }
        else
        {
            ultimateMainMenu.SendData(int.Parse(gameObject.name));
        }
    }
}
