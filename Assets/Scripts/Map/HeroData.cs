using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using Newtonsoft.Json.Linq;

public class HeroData
{
    int level;
    int experience;
    List<ItemData> items;
    
    public HeroData(int level, int experience, List<ItemData> items)
    {
        this.level = level;
        this.experience = experience;
        this.items = items;
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