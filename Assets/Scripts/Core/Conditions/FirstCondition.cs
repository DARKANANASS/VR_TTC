using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
using System;

[Serializable]
public class FirstCondition : ConditionClass
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

            Task<float> space = timing.WaitForSpace(ct);
            Task occluder = Task.Delay(500, ct);

            Task first = await Task.WhenAny(space, occluder);

            if (first == occluder)
            {
                control.BallMesh(false);
                await space;
            }
            control.BallMesh(true);
            control.StopBall();
            await Task.Delay(900, ct);
            control.DropAll();

            result.Values.Add("C", "TTC");
            result.Values.Add("S", speed);
            result.Values.Add("D", duration);
            EventHelper.AddOcc(result, duration);

            EventHelper.AddMove(result, duration);
            result.Events.Add(new TimeEvent("actual_move_duration", space.Result * 1000));

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
        FirstCondition clone = new FirstCondition();
        clone.focusePrefab = this.focusePrefab;
        clone.speed = speed;
        clone.duration = duration;
        clone.vertices = vertices;
        clone.startText = this.startText;
        clone.train = train;
        return clone;
    }
}
