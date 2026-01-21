using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class Timing : MonoBehaviour
{
    [SerializeField] private InputListener input;
    private TaskCompletionSource<float> spaceWaiter;
    private TaskCompletionSource<float> pauseWaiter;

    private float startSpace;
    private float startPause;

    void Start()
    {
        input.SpacePressed += OnSpacePressed;
        input.PausePressed += OnPausePressed;
    }

    public void OnSpacePressed()
    {
        spaceWaiter?.TrySetResult(Time.realtimeSinceStartup - startSpace);
        spaceWaiter = null;
    }

    public void OnPausePressed()
    {
        pauseWaiter?.TrySetResult(Time.realtimeSinceStartup - startPause);
        pauseWaiter = null;
    }

    public Task<float> WaitForSpace(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        spaceWaiter?.TrySetCanceled();
        startSpace = Time.realtimeSinceStartup;
        spaceWaiter = new TaskCompletionSource<float>();

        var registration = ct.Register(() =>
        {
            spaceWaiter.TrySetCanceled(ct);
        });

        spaceWaiter.Task.ContinueWith((t) => registration.Dispose());
        return spaceWaiter.Task;
    }

    public Task<float> WaitForP(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        pauseWaiter?.TrySetCanceled();
        startPause = Time.realtimeSinceStartup;
        pauseWaiter = new TaskCompletionSource<float>();

        var registration = ct.Register(() =>
        {
            pauseWaiter.TrySetCanceled(ct);
        });

        pauseWaiter.Task.ContinueWith((t) => registration.Dispose());

        return pauseWaiter.Task;
    }
}
