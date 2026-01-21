using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Results
{
    public float accuracy;
    public Dictionary<string, object> Values = new();
    public List<TimeEvent> Events = new();

    public float GetEvent(string name)
    {
        return Events.FirstOrDefault(e => e.Name == name)?.Time ?? 0;
    }

    public float Accuracy(string refName, string actualName)
    {
        float reff = GetEvent(refName);
        float act = GetEvent(actualName);

        float error = Mathf.Abs(reff - act);

        if (error >= reff) return 0;
        else return 100f * (1f - error / reff);
    }
}

public class TimeEvent
{
    public string Name;
    public float Time;

    public TimeEvent(string name, float time)
    {
        Name = name;
        Time = time;
    }   
}