using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IExplorable
{
    bool explored {get;}

    void Explore(Unit explorer);
}
