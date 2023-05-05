using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public bool dispAreaAvailable;
    public bool minMapAreaAvailable;

    // Start is called before the first frame update
    void Start()
    {
        dispAreaAvailable = true;
        minMapAreaAvailable = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setDispAreaAvailability(bool status)
    {
        dispAreaAvailable = status;
    }

    public void setMinMapAreaAvailability(bool status)
    {
        minMapAreaAvailable = status;
    }
}
