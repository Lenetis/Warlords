using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using Newtonsoft.Json.Linq;

public class HeroData
{
    public Unit unit {get; set;}
    public int level {get; set;}
    public int experience {get; set;}
    public List<ItemData> items {get; set;}
    
    public HeroData(int level, int experience, List<ItemData> items)
    {
        this.level = level;
        this.experience = experience;
        this.items = items;
        this.unit = unit;
    }

    /// Removes the item from the tilemap and adds it to the list of items
    public void PickUpItem(Item item)
    {
        items.Add(item.itemData);
        if (item.itemData.pathfinder != null) {  // todo do this also for battleStats and economy (and remove the getters from Unit)
            unit.basePathfinder.AddPathfinder(item.itemData.pathfinder);
            unit.army.UpdatePathfindingTypes();
        }
        item.Destroy();
    }

    /// Removes the item from the list of items and spawns a new Item MapObject containing this item at the specified position
    public void DropItem(ItemData itemData, Position position)
    {
        items.Remove(itemData);
        if (itemData.pathfinder != null) {
            unit.basePathfinder.RemovePathfinder(itemData.pathfinder);
            unit.army.UpdatePathfindingTypes();
        }

        Item newItem = new Item(itemData, position);
        newItem.AddToGame();
    }

    public static HeroData FromJObject(JObject attributes)
    {
        int level = (int)attributes.GetValue("level");
        int experience = (int)attributes.GetValue("experience");

        List<ItemData> items = new List<ItemData>();
        foreach (JToken itemJToken in attributes.GetValue("items")) {
            items.Add(ItemData.FromJObject((JObject)itemJToken));
        }

        return new HeroData(level, experience, items);
    }

    public JObject ToJObject()
    {
        JObject heroJObject = new JObject();

        heroJObject.Add("level", level);
        heroJObject.Add("experience", experience);
        heroJObject.Add("items", new JArray(items.Select(item => item.ToJObject())));

        return heroJObject;
    }
}