using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteGameButton : MonoBehaviour
{
    public GameMenu gameMenu;

    // Start is called before the first frame update
    void Start()
    {
        gameMenu  = transform.parent.parent.parent.parent.parent.GetComponent<GameMenu>();
    }

    public void Delete()
    {
        gameMenu.DeleteGame(int.Parse(gameObject.name.Substring(1)));
    }
}
