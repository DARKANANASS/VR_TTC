using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
using System;

[Serializable]
public class SecondCondition : ConditionClass
{
    private Vector3[] vertices;
    private float speed;

    public override async Task RunCondition(SceneController control, Timing timing, LessonUI ui, Results result, CancellationToken ct)
    {
        try
        {
            await Task.Delay(900, ct);

            ui.Image(false);
            control.InstantLine(vertices);
            control.ShowEndPos(vertices[vertices.Length - 1]);
            control.InstantBall(speed, vertices);

            var space = await timing.WaitForSpace(ct);

            control.StopBall();
            await Task.Delay(900, ct);

            control.DropAll();

            result.Values.Add("C", "Контрольный");
            result.Values.Add("S", speed);
            result.Values.Add("D", duration);

            EventHelper.AddMove(result, duration);
            result.Events.Add(new TimeEvent("ref_move_to_target", duration + 900));
            result.Events.Add(new TimeEvent("actual_move_duration", space * 1000));

            result.accuracy = result.Accuracy("ref_move_to_target", "actual_move_duration");
        }
        catch (OperationCanceledException)
        {
            control.ClearScene();
            ui.HideAll();
            throw;
        }
    }

    public ConditionClass CloneWithAdd(Vector3[] vertices, float speed, int duration, bool train)
    {
        SecondCondition clone = new SecondCondition();
        clone.focusePrefab = this.focusePrefab;
        clone.speed = speed;
        clone.train = train;
        clone.startText = this.startText;
        clone.duration = duration;
        clone.vertices = vertices;
        return clone;
    }
}