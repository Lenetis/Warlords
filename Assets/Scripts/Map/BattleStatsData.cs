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

    private int? _command;
    public int command
    {
        get {return _command.GetValueOrDefault(0);}
    }

    private int? _bonus;
    public int bonus
    {
        get {return _bonus.GetValueOrDefault(0);}
    }
    
    // todo:
    // strength bonuses in certain situations

    public BattleStatsData(int? strength, int? command, int? bonus)
    {
        _strength = strength;
        _command = command;
        _bonus = bonus;
    }

    public static BattleStatsData FromJObject(JObject attributes)
    {
        int? strength = null;
        if (attributes.ContainsKey("strength")) {
            strength = (int)attributes.GetValue("strength");
        }

        int? command = null;
        if (attributes.ContainsKey("command")) {
            command = (int)attributes.GetValue("command");
        }

        int? bonus = null;
        if (attributes.ContainsKey("bonus")) {
            bonus = (int)attributes.GetValue("bonus");
        }

        return new BattleStatsData(strength, command, bonus);
    }

    public JObject ToJObject()
    {
        JObject battleStatsJObject = new JObject();
        
        if (_strength != null) {
            battleStatsJObject.Add("strength", _strength);
        }

        if (_command != null) {
            battleStatsJObject.Add("command", _command);
        }

        if (_bonus != null) {
            battleStatsJObject.Add("bonus", _bonus);
        }
        
        return battleStatsJObject;
    }
}
