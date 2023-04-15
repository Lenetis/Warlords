using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public bool dispAreaAvailable;

    // Start is called before the first frame update
    void Start()
    {
        dispAreaAvailable = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setDispAreaAvailability(bool status)
    {
        dispAreaAvailable = status;
    }
}
