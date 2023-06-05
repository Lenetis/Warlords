using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json.Linq;

public class EconomyData
{
    int? _income;
    public int income
    {
        get { return _income.GetValueOrDefault(0); }
        set { _income = value; }
    }

    int? _upkeep;
    public int upkeep
    {
        get { return _upkeep.GetValueOrDefault(0); }
        set { _upkeep = value; }
    }

    public EconomyData(int? income, int? upkeep)
    {
        this._income = income;
        this._upkeep = upkeep;
    }

    public static EconomyData FromJObject(JObject attributes)
    {
        int? income = null;
        int? upkeep = null;
        if (attributes.ContainsKey("income")) {
            income = (int)attributes.GetValue("income");
        }
        if (attributes.ContainsKey("upkeep")) {
            upkeep = (int)attributes.GetValue("upkeep");
        }

        return new EconomyData(income, upkeep);
    }

    public JObject ToJObject()
    {
        JObject economyJObject = new JObject();
        if (_income != null) {
            economyJObject.Add("income", _income);
        }
        if (_upkeep != null) {
            economyJObject.Add("upkeep", _upkeep);
        }
        
        return economyJObject;
    }
}