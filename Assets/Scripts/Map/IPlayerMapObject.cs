using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerMapObject
{
    public Player owner {get;}
    public Position position {get;}

    public void StartTurn();

    public List<Army> GetSupportingArmies();

    public bool OccupiesPosition(Position position);
}
