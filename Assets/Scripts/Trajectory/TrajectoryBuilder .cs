using UnityEngine;
using System.Collections.Generic;

public class TrajectoryBuilder : MonoBehaviour
{
    public Vector3 startPoint = Vector3.zero;
    public float segmentLength = 1f;
    public int iterations = 5;
    
    public List<Vector3> trajectoryPoints = new List<Vector3>();
    
    void Start()
    {
        BuildTrajectory(startPoint, segmentLength, iterations);
        DrawTrajectory();
    }
    
    void BuildTrajectory(Vector3 start, float length, int t)
    {
        trajectoryPoints.Clear();
        trajectoryPoints.Add(start);
        
        Vector3 currentPoint = start;
        
        for (int i = 0; i < t; i++)
        {
            // Здесь можно изменить направление отрезка
            // Например, всегда вперед по оси X
            Vector3 direction = Vector3.right;
            
            // Создаем новый отрезок
            Vector3 nextPoint = currentPoint + (direction * length);
            
            trajectoryPoints.Add(nextPoint);
            currentPoint = nextPoint;
        }
    }
    
    void DrawTrajectory()
    {
        for (int i = 0; i < trajectoryPoints.Count - 1; i++)
        {
            Debug.DrawLine(trajectoryPoints[i], trajectoryPoints[i + 1], Color.green, 10f);
        }
    }
}