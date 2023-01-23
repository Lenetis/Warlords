using System.Collections;
using System.Collections.Generic;

public class TileContents
{
    public List<Army> armies {get; private set;}
    // items
    // road ??  -- maybe in TileData
    // city
    // port
    // temple/ruins
    // maybe something else

    public TileContents()
    {
        armies = null;
    }

    public TileContents(Army army)
    {
        armies = null;
        AddArmy(army);
    }

    public void AddArmy(Army army)
    {
        if (armies == null) {
            armies = new List<Army>();
        }
        armies.Add(army);        
    }

    public void RemoveArmy(Army army)
    {
        armies.Remove(army);
        if (armies.Count == 0) {
            armies = null;
        }
    }

    public override string ToString()
    {
        string toReturn = "";
        if (armies != null) {
            foreach (Army army in armies){
                toReturn += $"Army: {army}";
            }
        }
        return toReturn;
    }
}