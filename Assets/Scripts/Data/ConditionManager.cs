using System;
using UnityEngine;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;

[Serializable]
public class ConditionManager
{
    [SerializedDictionary("Name", "Points")]
    public SerializedDictionary<string, Vector3[]> vectors = Trajectory_Data.TrajectoryDictionary;
    
    [SerializeField] private float fastSpeed;
    [SerializeField] private float slowSpeed;
    [SerializeField] private int shortDur;
    [SerializeField] private int mediumDur;
    [SerializeField] private int longDur;
    [SerializeField] private FirstCondition firstCond;
    [SerializeField] private SecondCondition secondCond;
    [SerializeField] private ThirdCondition thirdCond;
    [SerializeField] private FourthCondition fourthCond;

    private float DefineSpeed(string key)
    {
        switch (key)
        {
            case "S1": return slowSpeed;
            case "S2": return fastSpeed;

            default:
                return 0;
        }
    }

    private int DefineDuration(string key)
    {
        switch (key)
        {
            case "D1": return shortDur;
            case "D2": return mediumDur;
            case "D3": return longDur;

            default:
                return 0;
        }
    }

    private Vector3[] DefineVectors(string key)
    {
        if (key == "")
        {
            return new Vector3[] { };
        }
        return vectors[key];
    }

    private bool isTrain(string key)
    {
        if (key == "1") return true;
        else return false;
    }

    public ConditionClass SetCondition(string Ckey, string Dkey, string Vkey = " ", string Skey = " ", string Tkey = " ")
    {
        var s = DefineSpeed(Skey);
        var d = DefineDuration(Dkey);
        var v = DefineVectors(Vkey);
        var t = isTrain(Tkey);
        
        switch (Ckey)
        {
            case "C1": return firstCond.CloneWithAdd(v, s, d, t);

            case "C2": return secondCond.CloneWithAdd(v, s, d, t);

            case "C3": return thirdCond.CloneWithAdd(v, s, d, t);

            case "C4": return fourthCond.CloneWithAdd(d, t);

            default:
                return null;
        }
    }
}
