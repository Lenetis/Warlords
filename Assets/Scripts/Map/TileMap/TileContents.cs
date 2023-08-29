using System.Collections;
using System.Collections.Generic;

public class TileContents  // todo change to a struct and remove ALL methods
{
    public List<Army> armies {get; private set;}
    public List<Item> items {get; private set;}
    public Structure structure {get; set;}
    

    /// Deactivates all armies' mapSprites and activates mapSprite of the first army
    private void UpdateArmySprites ()
    {
        foreach(Army army in armies) {
            army.mapSprite.SetActive(false);
        }
        armies[0].mapSprite.SetActive(true);
    }

    /// Adds army to tile contents
    public void AddArmy(Army army, int index = 0)
    {
        if (armies == null) {
            armies = new List<Army>();
        }

        armies.Insert(index, army);
        UpdateArmySprites();
    }

    /// Removes army from tile contents
    public void RemoveArmy(Army army)
    {
        armies.Remove(army);
        if (armies.Count == 0) {
            armies = null;
        }
        else {
            UpdateArmySprites();
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