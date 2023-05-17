using System.Collections;
using System.Collections.Generic;

public class TileContents  // todo change to a struct and remove ALL methods
{
    public List<Army> armies {get; private set;}
    public Structure structure {get; set;}
    // todo add items, maybe something else

    /// Adds army to tile contents
    public void AddArmy(Army army)
    {
        if (armies == null) {
            armies = new List<Army>();
        }
        armies.Add(army);        
    }

    /// Removes army from tile contents
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
            foreach (Army army in armies) {
                toReturn += $"Army: {army} ";
            }
        }
        if (structure != null) {
            toReturn += $"Structure: {structure}";
        }
        return toReturn;
    }
}