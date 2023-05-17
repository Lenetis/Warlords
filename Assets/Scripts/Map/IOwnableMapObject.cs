using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this does not neccessarily require it to be a MapObject, but it does require a position, which every MapObject has
//    I don't know how to make this more explicit.
//    probably a //todo
public interface IOwnableMapObject
{
    Player owner {get;}
    Position position {get;}

    void StartTurn();

    List<Army> GetSupportingArmies();
}
