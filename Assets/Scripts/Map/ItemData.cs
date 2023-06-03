using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json.Linq;

public class ItemData
{
    public string name {get; private set;}
    public string description {get; private set;}
    public Texture2D texture {get; private set;}

    public PathfinderData pathfinder {get; private set;}
    public BattleStatsData battleStats {get; private set;}
    public EconomyData economy {get; private set;}

    public ItemData(string name, string description, Texture2D texture, PathfinderData pathfinder, BattleStatsData battleStats, EconomyData economy)
    {
        this.name = name;
        this.description = description;
        this.texture = texture;
        this.pathfinder = pathfinder;
        this.battleStats = battleStats;
        this.economy = economy;
    }

    public static ItemData FromJObject(JObject attributes)
    {
        string name = (string)attributes.GetValue("name");

        string description = (string)attributes.GetValue("description");

        string texturePath = (string)attributes.GetValue("texture");
        Texture2D texture = ResourceManager.LoadTexture(texturePath);

        PathfinderData pathfinder = null;
        if (attributes.ContainsKey("pathfinder")) {
            pathfinder = PathfinderData.FromJObject((JObject)attributes.GetValue("pathfinder"));
        }

        BattleStatsData battleStats = null;
        if (attributes.ContainsKey("battleStats")) {
            battleStats = BattleStatsData.FromJObject((JObject)attributes.GetValue("battleStats"));
        }

        EconomyData economy = null;
        if (attributes.ContainsKey("economy")) {
            economy = EconomyData.FromJObject((JObject)attributes.GetValue("economy"));
        }

        return new ItemData(name, description, texture, pathfinder, battleStats, economy);
    }

    public JObject ToJObject()
    {
        JObject itemJObject = new JObject();

        if (pathfinder != null) {
            itemJObject.Add("pathfinder", pathfinder.ToJObject());
        }
        if (battleStats != null) {
            itemJObject.Add("battleStats", battleStats.ToJObject());
        }
        if (economy != null) {
            itemJObject.Add("economy", economy.ToJObject());
        }
        
        return itemJObject;
    }

    public override string ToString()
    {
        return $"{name} ({description})";
    }
}