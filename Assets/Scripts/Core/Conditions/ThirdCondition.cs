using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class ThirdCondition : ConditionClass
{
    private Vector3[] vertices;
    private float speed;
    [SerializeField] private Sprite stimul;

    public override async Task RunCondition(SceneController control, Timing timing, LessonUI ui, Results result, CancellationToken ct)
    {
        try
        {
            await Task.Delay(900, ct);

            control.InstantLine(vertices);
            control.ShowEndPos(vertices[vertices.Length - 1]);
            control.InstantBall(speed, vertices);

            var ball = control.GetBall();
            
            var tcs = new TaskCompletionSource<bool>();
            using (ct.Register(() => tcs.TrySetCanceled(ct)))
            {
            ball.Finish += () => tcs.TrySetResult(true);

            await tcs.Task;
            }

            control.StopBall();
            await Task.Delay(900, ct);

            control.DropAll();
            ui.SetImage(stimul);

            var spaceTask = await timing.WaitForSpace(ct);

            ui.Image(false);

            result.Values.Add("C", "Обратный ход");
            result.Values.Add("S", speed);
            result.Values.Add("D", duration);

            EventHelper.AddMove(result, duration);
            EventHelper.AddIsi(result, duration);
            result.Events.Add(new TimeEvent("ref_path", duration));
            result.Events.Add(new TimeEvent("actual_stim_to_response", spaceTask * 1000));

            result.accuracy = result.Accuracy("ref_path", "actual_stim_to_response");
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
        ThirdCondition clone = new ThirdCondition();
        clone.focusePrefab = this.focusePrefab;
        clone.stimul = this.stimul;
        clone.train = train;
        clone.speed = speed;
        clone.startText = this.startText;
        clone.duration = duration;
        clone.vertices = vertices;
        return clone;
    }
}