using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
using System;

[Serializable]
public class LessonManager
{
    private CancellationTokenSource lessonCts;
    [SerializeField] private ConditionRunner runner;

    public async Task StartLesson(LessonPlanAsset lessonPlan, DataWriter writer)
    {
        if (lessonCts != null && !lessonCts.IsCancellationRequested)
        {
            lessonCts.Cancel();
        }
        lessonCts = new CancellationTokenSource();
        try
        {
            await RunLessonLoop(lessonPlan, writer, lessonCts.Token);
        }
        catch (OperationCanceledException)
        {
            Debug.Log("остановка");
        }
    }

    private async Task RunLessonLoop(LessonPlanAsset lessonPlan, DataWriter writer, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var lastC = PlayerPrefs.GetInt("c");
        var lastB = PlayerPrefs.GetInt("b");
        for (int b = 0; b < lessonPlan.blocks.Count; b++)
        {
            if (b < lastB) continue;

            ct.ThrowIfCancellationRequested();
            var block = lessonPlan.blocks[b];

            for (int c = 0; c < block.conditions.Count; c++)
            {
                if (b == lastB && c < lastC) continue;

                var cond = block.conditions[c];
                SetProgress(c, b);
                ct.ThrowIfCancellationRequested();

                Results result = new Results();

                result.Values.Add("c", c + 1);
                result.Values.Add("b", b + 1);

                await runner.Run(cond, writer, result, ct);
            }

            await runner.timing.WaitForP(ct);
        }
        ResetProgress();
    }

    public void AbortLesson()
    {
        if (lessonCts == null || lessonCts.IsCancellationRequested) return;

        lessonCts.Cancel();
    }

    public bool HasProgress()
    {
        if (PlayerPrefs.GetInt("c") == -1) return false;
        else return true;
    }

    public void ResetProgress()
    {
        PlayerPrefs.SetInt("c", -1);
        PlayerPrefs.SetInt("b", -1);
        PlayerPrefs.Save();
    }

    private void SetProgress(int c, int b)
    {
        PlayerPrefs.SetInt("c", c);
        PlayerPrefs.SetInt("b", b);
        PlayerPrefs.Save();
    }
}
