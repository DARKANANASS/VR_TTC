using System;
using UnityEngine;

public class InputListener : MonoBehaviour
{
    public event Action SpacePressed;
    public event Action PausePressed;
    public event Action EscapePressed;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpacePressed?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            PausePressed?.Invoke();
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            EscapePressed?.Invoke();
        }
    }
}
