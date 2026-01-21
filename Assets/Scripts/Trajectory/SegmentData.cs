using UnityEngine;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class SegmentData : MonoBehaviour
{
    [Range(0, 360)] public float azimuth = 0f;      // Горизонтальный угол (0-360°)
    [Range(-90, 90)] public float elevation = 0f;   // Вертикальный угол (-90°...+90°)
    [Min(0.1f)] public float length = 1f;           // Длина отрезка
    
    [HideInInspector] public Vector3 direction;     // Направление отрезка (вычисляется)
    [HideInInspector] public float actualLength;    // Фактическая длина (с учетом вариации)
    
    public SegmentData() { }
    
    public SegmentData(float az, float el, float len)
    {
        azimuth = az;
        elevation = el;
        length = len;
        CalculateDirection();
    }
    
    public void CalculateDirection()
    {
        float azRad = azimuth * Mathf.Deg2Rad;
        float elRad = elevation * Mathf.Deg2Rad;
        float cosEl = Mathf.Cos(elRad);
        
        direction = new Vector3(
            cosEl * Mathf.Cos(azRad),
            Mathf.Sin(elRad),
            cosEl * Mathf.Sin(azRad)
        ).normalized;
    }
    
    public Vector3 CalculateEndPoint(Vector3 startPoint)
    {
        CalculateDirection();
        actualLength = length;
        return startPoint + direction * actualLength;
    }
}