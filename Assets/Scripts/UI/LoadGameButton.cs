using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadGameButton : MonoBehaviour
{
    public GameMenu gameMenu;

    // Start is called before the first frame update
    void Start()
    {
        gameMenu  = transform.parent.parent.parent.parent.parent.GetComponent<GameMenu>();
    }

    public void Load()
    {
        gameMenu.LoadGame(int.Parse(gameObject.name));
    }
}
