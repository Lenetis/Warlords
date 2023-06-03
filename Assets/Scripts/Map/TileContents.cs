using System.Collections;
using System.Collections.Generic;

public class TileContents  // todo change to a struct and remove ALL methods
{
    public List<Army> armies {get; private set;}
    public List<Item> items {get; private set;}
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

    /// Adds item to tile contents
    public void AddItem(Item item)
    {
        if (items == null) {
            items = new List<Item>();
        }
        items.Add(item);
    }

    /// Removes item from tile contents
    public void RemoveItem(Item item)
    {
        items.Remove(item);
        if (items.Count == 0) {
            items = null;
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
        if (items != null) {
            toReturn += $"Items: {string.Join(", ", items)}";
        }
        if (structure != null) {
            toReturn += $"Structure: {structure}";
        }
        return toReturn;
    }
}