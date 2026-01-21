using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class FourthCondition : ConditionClass
{
    [SerializeField] private Sprite stimul;

    public override async Task RunCondition(SceneController control, Timing timing, LessonUI ui, Results result, CancellationToken ct)
    {
        try
        {
            await Task.Delay(900, ct);

            ui.SetImage(stimul);

            await Task.Delay(duration, ct);

            ui.SetImage(focusePrefab);

            await Task.Delay(900, ct);

            ui.SetImage(stimul);

            var spaceTask = await timing.WaitForSpace(ct);

            ui.Image(false);

            result.Values.Add("C", "Воспроизведение");
            result.Values.Add("D", duration);

            EventHelper.AddStim(result, duration);
            EventHelper.AddIsi(result, duration);
            result.Events.Add(new TimeEvent("ref_stim", duration));
            result.Events.Add(new TimeEvent("actual_stim_to_response", spaceTask * 1000));

            result.accuracy = result.Accuracy("ref_stim", "actual_stim_to_response");
        }
        catch (OperationCanceledException)
        {
            control.ClearScene();
            ui.HideAll();
            throw;
        } 
    }

    public ConditionClass CloneWithAdd(int duration, bool train)
    {
        FourthCondition clone = new FourthCondition();
        clone.focusePrefab = this.focusePrefab;
        clone.stimul = this.stimul;
        clone.duration = duration;
        clone.startText = this.startText;
        clone.train = train;
        return clone;
    }
}
