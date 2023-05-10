using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public GameObject cityManagement;
    public GameObject gameMenu;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool controllsAvailable()
    {
        if(!gameMenu.activeSelf && !cityManagement.activeSelf)
        {
            return true;
        }

        return false;
    }
}
