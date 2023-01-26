using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public string name {get;}
    public Color color {get;}

    private List<Army> armies;
    private List<int> cities;  // todo

    public Player(string name, Color color)
    {
        this.name = name;
        this.color = color;

        armies = new List<Army>();
        cities = new List<int>();  // todo

        GameController gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        gameController.AddPlayer(this);
    }

    public void AddArmy(Army army)
    {
        armies.Add(army);
    }

    public void StartTurn()
    {
        foreach (Army army in armies) {
            army.StartTurn();
        }
        // do the same thing with cities
    }

    public void MoveAll()
    {
        foreach (Army army in armies) {
            army.Move();
        }
    }

    public override string ToString()
    {
        return $"Player(name={name})";
    }
}
