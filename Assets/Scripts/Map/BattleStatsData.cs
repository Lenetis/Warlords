using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json.Linq;

public class BattleStatsData
{
    private int? _strength;
    public int strength
    {
        get {return _strength.GetValueOrDefault(0);}
    }
    
    // todo:
    // command
    // strength bonuses in certain situations

    public BattleStatsData(int? strength)
    {
        _strength = strength;
    }

    public static BattleStatsData FromJObject(JObject attributes)
    {
        int? strength = null;
        if (attributes.ContainsKey("strength")) {
            strength = (int)attributes.GetValue("strength");
        }

        return new BattleStatsData(strength);
    }

    public JObject ToJObject()
    {
        JObject battleStatsJObject = new JObject();
        
        if (_strength != null) {
            battleStatsJObject.Add("strength", _strength);
        }
        
        return battleStatsJObject;
    }
}
