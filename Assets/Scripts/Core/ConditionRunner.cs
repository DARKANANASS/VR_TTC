using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class ConditionRunner
{
    [SerializeField] private SceneController controller;
    public Timing timing;
    [SerializeField] private LessonUI ui;

    public async Task Run(ConditionClass cond, DataWriter writer, Results result, CancellationToken ct)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            if (cond.train)
            {
                ui.Description(cond.startText, true);
                await timing.WaitForSpace(ct);
                ui.Description(cond.startText, false);
            }

            ui.SetImage(cond.focusePrefab);
            ui.Image(true);

            await cond.RunCondition(controller, timing, ui, result, ct);

            await Task.Yield();
            ui.EndScreen(true);
            if (cond.train)
            {
                ui.ShowAccuracy("Точность: " + result.accuracy.ToString() + "%");
            }

            writer.CreateRowAndFlush(result);

            await timing.WaitForSpace(ct);

            ui.EndScreen(false);
            ui.HideAccuracy();
            await Task.Yield();
            
        } catch (OperationCanceledException){

            controller.ClearScene();
            ui.HideAll();
            throw;
        }
       
    }
}
