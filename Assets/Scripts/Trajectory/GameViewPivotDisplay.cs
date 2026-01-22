using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameViewPivotDisplay : MonoBehaviour
{
    public static GameViewPivotDisplay Instance { get; private set; }
    
    public Color pivotColor = Color.cyan;
    public float pivotSize = 10f;
    public Vector3 pivotPosition;
    public bool showPivot = false;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    #if UNITY_EDITOR
    void OnGUI()
    {
        if (showPivot && Event.current.type == EventType.Repaint)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(pivotPosition);
            if (screenPos.z > 0)
            {
                Rect pivotRect = new Rect(
                    screenPos.x - pivotSize / 2,
                    Screen.height - screenPos.y - pivotSize / 2,
                    pivotSize,
                    pivotSize
                );
                
                Handles.BeginGUI();
                Handles.color = pivotColor;
                Handles.DrawSolidDisc(pivotRect.center, Vector3.forward, pivotSize / 2);
                Handles.EndGUI();
            }
        }
    }
    #endif
    
    public static void ShowPivot(Vector3 position)
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("GameViewPivotDisplay");
            Instance = go.AddComponent<GameViewPivotDisplay>();
        }
        
        Instance.pivotPosition = position;
        Instance.showPivot = true;
    }
    
    public static void HidePivot()
    {
        if (Instance != null)
        {
            Instance.showPivot = false;
        }
    }
}