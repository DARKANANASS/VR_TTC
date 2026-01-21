using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
using System;
using TMPro;

[Serializable]
public abstract class ConditionClass
{
    public TextMeshProUGUI startText;
    public Sprite focusePrefab;
    [HideInInspector] public bool train;
    protected int duration;

    public abstract Task RunCondition(SceneController control, Timing timing, LessonUI ui, Results result, CancellationToken ct);

}