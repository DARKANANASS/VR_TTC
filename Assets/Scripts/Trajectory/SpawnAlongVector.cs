using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;





#if UNITY_EDITOR
using UnityEditor;
#endif

public class CompactVectorChain : MonoBehaviour
{
    [Header("Суммарная длина")]
    public float targetTotalLength = 10f;
    [SerializeField] private float actualTotalLength;
    [System.Serializable]

    public struct SegmentConfig
    {
        [Range(-90, 90)] public float azimuth;
        [Range(0, 360)] public float elevation;
        [Range(0.35f, 0.65f)] public float length;
    }


    [Header("Начальная точка")]
    public StartPointMode startPointMode = StartPointMode.UseTransform;
    public Vector3 customStartPoint = Vector3.zero;
    public Transform startTransform;

    [Header("Цепочка векторов")]
    public SegmentConfig[] segments = new SegmentConfig[]
    {
        new SegmentConfig { azimuth = 45f, elevation = 30f, length = 2f },
        new SegmentConfig { azimuth = 135f, elevation = 15f, length = 2.5f },
        new SegmentConfig { azimuth = 225f, elevation = -20f, length = 1.8f }
    };

    [Header("Результат")]
    [SerializeField] private Vector3[] points;

    [Header("Сохранение точек")]
    public string saveName = "ChainPoints";
    public Dictionary<string, Vector3[]> savedPoints = new Dictionary<string, Vector3[]>();  

    public enum StartPointMode
    {
        UseTransform,       // Использовать позицию этого Transform
        UseCustomPoint,     // Использовать кастомную точку
        UseOtherTransform   // Использовать другой Transform
    }

    [ContextMenu("Generate Chain")]
    public void GenerateChain()
    {
        if (segments == null || segments.Length == 0)
        {
            Debug.LogError("Нет сегментов для генерации!");
            return;
        }

        // Получаем начальную точку в зависимости от режима
        Vector3 startPoint = GetStartPoint();

        // Вычисляем суммарную длину по настройкам
        float configTotalLength = 0f;
        foreach (var segment in segments)
        {
            configTotalLength += segment.length;
        }

        if (configTotalLength <= 0)
        {
            Debug.LogError("Суммарная длина сегментов должна быть больше 0!");
            return;
        }

        // Масштабирующий коэффициент
        float scaleFactor = targetTotalLength / configTotalLength;

        // Генерируем точки
        points = new Vector3[segments.Length + 1];
        points[0] = startPoint;

        actualTotalLength = 0f;
        Vector3 currentPoint = startPoint;

        for (int i = 0; i < segments.Length; i++)
        {
            SegmentConfig config = segments[i];

            // Вычисляем направление
            float azRad = config.azimuth * Mathf.Deg2Rad;
            float elRad = config.elevation * Mathf.Deg2Rad;
            float cosEl = Mathf.Cos(elRad);

            Vector3 direction = new Vector3(
                cosEl * Mathf.Cos(azRad),
                Mathf.Sin(elRad),
                cosEl * Mathf.Sin(azRad)
            ).normalized;

            // Масштабируем длину
            float scaledLength = config.length * scaleFactor;

            // Вычисляем новую точку
            currentPoint = currentPoint + direction * scaledLength;
            points[i + 1] = currentPoint;

            actualTotalLength += scaledLength;
        }

        Debug.Log($"Сгенерирована цепочка из {points.Length} точек. Длина: {actualTotalLength:F2}");
        Debug.Log($"Начальная точка: {startPoint}");
    }

    /// <summary>
    /// Сохранить текущие точки в словарь
    /// </summary>
    [ContextMenu("Save Current Points")]
    public void SaveCurrentPoints()
    {
        if (points == null || points.Length == 0)
        {
            Debug.LogWarning("Нет точек для сохранения! Сначала сгенерируйте цепочку.");
            return;
        }

        if (string.IsNullOrEmpty(saveName))
        {
            Debug.LogWarning("Имя сохранения не может быть пустым!");
            return;
        }

        // Копируем массив точек
        Vector3[] pointsCopy = new Vector3[points.Length];
        points.CopyTo(pointsCopy, 0);

        // Добавляем или обновляем в словаре
        if (savedPoints.ContainsKey(saveName))
        {
            savedPoints[saveName] = pointsCopy;
            Debug.Log($"Обновлены точки с именем '{saveName}' ({pointsCopy.Length} точек)");
        }
        else
        {
            savedPoints.Add(saveName, pointsCopy);
            Debug.Log($"Сохранены новые точки с именем '{saveName}' ({pointsCopy.Length} точек)");
        }
    }

    /// <summary>
    /// Сохранить точки с указанным именем
    /// </summary>
    public void SavePoints(string name)
    {
        saveName = name;
        SaveCurrentPoints();
    }

    /// <summary>
    /// Загрузить точки по имени
    /// </summary>
    [ContextMenu("Load Last Saved Points")]
    public void LoadPoints()
    {
        if (string.IsNullOrEmpty(saveName))
        {
            Debug.LogWarning("Имя для загрузки не указано!");
            return;
        }

        if (savedPoints.ContainsKey(saveName))
        {
            points = savedPoints[saveName];
            Debug.Log($"Загружены точки '{saveName}' ({points.Length} точек)");
        }
        else
        {
            Debug.LogWarning($"Точки с именем '{saveName}' не найдены!");
        }
    }

    /// <summary>
    /// Загрузить точки по указанному имени
    /// </summary>
    public void LoadPoints(string name)
    {
        saveName = name;
        LoadPoints();
    }

    /// <summary>
    /// Удалить сохраненные точки по имени
    /// </summary>
    [ContextMenu("Delete Saved Points")]
    public void DeleteSavedPoints()
    {
        if (string.IsNullOrEmpty(saveName))
        {
            Debug.LogWarning("Имя для удаления не указано!");
            return;
        }

        if (savedPoints.ContainsKey(saveName))
        {
            savedPoints.Remove(saveName);
            Debug.Log($"Удалены точки с именем '{saveName}'");
        }
        else
        {
            Debug.LogWarning($"Точки с именем '{saveName}' не найдены!");
        }
    }

    /// <summary>
    /// Удалить все сохраненные точки
    /// </summary>
    [ContextMenu("Delete All Saved Points")]
    public void DeleteAllSavedPoints()
    {
        int count = savedPoints.Count;
        savedPoints.Clear();
        Debug.Log($"Удалены все сохраненные точки ({count} записей)");
    }

    /// <summary>
    /// Получить список всех сохраненных имен
    /// </summary>
    public List<string> GetSavedPointNames()
    {
        return new List<string>(savedPoints.Keys);
    }

    /// <summary>
    /// Получить точки по имени
    /// </summary>
    public Vector3[] GetSavedPoints(string name)
    {
        if (savedPoints.ContainsKey(name))
        {
            return savedPoints[name];
        }
        return null;
    }

    /// <summary>
    /// Проверить, есть ли сохраненные точки с указанным именем
    /// </summary>
    public bool HasSavedPoints(string name)
    {
        return savedPoints.ContainsKey(name);
    }

    /// <summary>
    /// Экспортировать все сохраненные точки в JSON
    /// </summary>
    [ContextMenu("Export All Points to JSON")]
    public string ExportToJSON()
    {
        var exportData = new ExportData
        {
            savedPoints = new List<PointCollection>()
        };

        foreach (var kvp in savedPoints)
        {
            exportData.savedPoints.Add(new PointCollection
            {
                name = kvp.Key,
                points = kvp.Value
            });
        }

        string json = JsonUtility.ToJson(exportData, true);
        Debug.Log($"Экспортировано {savedPoints.Count} коллекций точек");
        return json;
    }

    /// <summary>
    /// Импортировать точки из JSON
    /// </summary>
    [ContextMenu("Import Points from JSON")]
    public void ImportFromJSON(string json)
    {
        try
        {
            ExportData importData = JsonUtility.FromJson<ExportData>(json);

            foreach (var collection in importData.savedPoints)
            {
                savedPoints[collection.name] = collection.points;
            }

            Debug.Log($"Импортировано {importData.savedPoints.Count} коллекций точек");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка импорта JSON: {e.Message}");
        }
    }

    [System.Serializable]
    public class ExportData
    {
        public List<PointCollection> savedPoints;
    }

    [System.Serializable]
    public class PointCollection
    {
        public string name;
        public Vector3[] points;
    }

    /// <summary>
    /// Получить начальную точку в зависимости от настроек
    /// </summary>
    private Vector3 GetStartPoint()
    {
        switch (startPointMode)
        {
            case StartPointMode.UseTransform:
                return transform.position;

            case StartPointMode.UseCustomPoint:
                return customStartPoint;

            case StartPointMode.UseOtherTransform:
                if (startTransform != null)
                    return startTransform.position;
                else
                {
                    Debug.LogWarning("Не указан startTransform! Использую позицию этого объекта.");
                    return transform.position;
                }

            default:
                return transform.position;
        }
    }

    [ContextMenu("Set Start Point to Current Position")]
    public void SetStartPointToCurrentPosition()
    {
        startPointMode = StartPointMode.UseTransform;
        Debug.Log("Начальная точка установлена на позицию этого объекта");
    }

    [ContextMenu("Set Start Point to Custom")]
    public void SetStartPointToCustom()
    {
        startPointMode = StartPointMode.UseCustomPoint;
        customStartPoint = transform.position;
        Debug.Log("Начальная точка установлена на кастомную точку");
    }

    [ContextMenu("Capture Current Position as Start Point")]
    public void CaptureCurrentPosition()
    {
        customStartPoint = transform.position;
        Debug.Log($"Текущая позиция сохранена как начальная точка: {customStartPoint}");
    }

    [ContextMenu("Randomize All Angles")]
    public void RandomizeAllAngles()
    {
        for (int i = 0; i < segments.Length; i++)
        {
            segments[i].azimuth = Random.Range(0f, 360f);
            segments[i].elevation = Random.Range(-90f, 90f);
        }
        Debug.Log("Все углы рандомизированы");
    }

    [ContextMenu("Randomize Azimuths")]
    public void RandomizeAzimuths()
    {
        for (int i = 0; i < segments.Length; i++)
        {
            segments[i].azimuth = Random.Range(0f, 360f);
        }
        Debug.Log("Азимуты рандомизированы");
    }

    [ContextMenu("Randomize Elevations")]
    public void RandomizeElevations()
    {
        for (int i = 0; i < segments.Length; i++)
        {
            segments[i].elevation = Random.Range(-90f, 90f);
        }
        Debug.Log("Углы места рандомизированы");
    }

    [ContextMenu("Randomize All")]
    public void RandomizeAll()
    {
        for (int i = 0; i < segments.Length; i++)
        {
            segments[i].azimuth = Random.Range(0f, 360f);
            segments[i].elevation = Random.Range(-90f, 90f);
            segments[i].length = Random.Range(0.5f, 3f);
        }
        Debug.Log("Все параметры рандомизированы");
    }

    [ContextMenu("Add Segment")]
    public void AddSegment()
    {
        var list = new System.Collections.Generic.List<SegmentConfig>(segments);
        list.Add(new SegmentConfig
        {
            azimuth = Random.Range(0f, 360f),
            elevation = Random.Range(-45f, 45f),
            length = 2f
        });
        segments = list.ToArray();
        Debug.Log($"Добавлен сегмент. Всего: {segments.Length}");
    }

    [ContextMenu("Remove Last Segment")]
    public void RemoveLastSegment()
    {
        if (segments.Length > 0)
        {
            var list = new System.Collections.Generic.List<SegmentConfig>(segments);
            list.RemoveAt(list.Count - 1);
            segments = list.ToArray();
            Debug.Log($"Удален последний сегмент. Осталось: {segments.Length}");
        }
    }

    [ContextMenu("Reset to Default")]
    public void ResetToDefault()
    {
        startPointMode = StartPointMode.UseTransform;
        customStartPoint = Vector3.zero;
        startTransform = null;

        segments = new SegmentConfig[]
        {
            new SegmentConfig { azimuth = 45f, elevation = 30f, length = 2f },
            new SegmentConfig { azimuth = 135f, elevation = 15f, length = 2.5f },
            new SegmentConfig { azimuth = 225f, elevation = -20f, length = 1.8f }
        };
        targetTotalLength = 10f;

        Debug.Log("Сброс к значениям по умолчанию");
    }

    void OnDrawGizmosSelected()
    {
        // Рисуем начальную точку в зависимости от режима
        Vector3 actualStartPoint = GetStartPoint();

        if (points != null && points.Length > 1)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(points[0], 0.2f);

            for (int i = 0; i < points.Length - 1; i++)
            {
                float t = (float)i / (points.Length - 1);
                Gizmos.color = Color.Lerp(Color.blue, Color.red, t);

                Gizmos.DrawLine(points[i], points[i + 1]);
                Gizmos.DrawSphere(points[i + 1], 0.15f);

                // Подписи
#if UNITY_EDITOR
                Handles.Label(points[i + 1], $"P{i + 1}");
                if (i < segments.Length)
                {
                    Vector3 midPoint = (points[i] + points[i + 1]) / 2f;
                    Handles.Label(midPoint,
                        $"Az: {segments[i].azimuth:F0}°\nEl: {segments[i].elevation:F0}°");
                }
#endif
            }

#if UNITY_EDITOR
            Handles.Label(actualStartPoint + Vector3.up * 2f,
                $"Длина: {actualTotalLength:F2}/{targetTotalLength}");
#endif
        }
        else if (segments != null && segments.Length > 0)
        {
            // Предварительный просмотр в редакторе
            Vector3 currentPoint = actualStartPoint;

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(currentPoint, 0.2f);

            float configTotalLength = 0f;
            foreach (var segment in segments)
            {
                configTotalLength += segment.length;
            }

            if (configTotalLength > 0)
            {
                float scaleFactor = targetTotalLength / configTotalLength;

                for (int i = 0; i < segments.Length; i++)
                {
                    SegmentConfig config = segments[i];

                    float azRad = config.azimuth * Mathf.Deg2Rad;
                    float elRad = config.elevation * Mathf.Deg2Rad;
                    float cosEl = Mathf.Cos(elRad);

                    Vector3 direction = new Vector3(
                        cosEl * Mathf.Cos(azRad),
                        Mathf.Sin(elRad),
                        cosEl * Mathf.Sin(azRad)
                    ).normalized;

                    float scaledLength = config.length * scaleFactor;
                    Vector3 endPoint = currentPoint + direction * scaledLength;

                    float t = (float)i / segments.Length;
                    Gizmos.color = Color.Lerp(Color.blue, Color.red, t);
                    Gizmos.DrawLine(currentPoint, endPoint);
                    Gizmos.DrawSphere(endPoint, 0.15f);

                    // Подписи
#if UNITY_EDITOR
                    Handles.Label(endPoint, $"P{i + 1}");
                    Vector3 midPoint = (currentPoint + endPoint) / 2f;
                    Handles.Label(midPoint,
                        $"Az: {config.azimuth:F0}°\nEl: {config.elevation:F0}°");
#endif

                    currentPoint = endPoint;
                }

#if UNITY_EDITOR
                Handles.Label(actualStartPoint + Vector3.up * 2f,
                    $"Предпросмотр\nСегментов: {segments.Length}\nРежим: {startPointMode}");
#endif
            }
        }

        // Визуализация начальной точки
        if (startPointMode == StartPointMode.UseOtherTransform && startTransform != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, startTransform.position);
            Gizmos.DrawSphere(startTransform.position, 0.15f);
        }
    }

    /// <summary>
    /// Получить массив точек цепочки
    /// </summary>
    public Vector3[] GetPoints()
    {
        return points != null ? points : new Vector3[0];
    }

    /// <summary>
    /// Получить начальную точку
    /// </summary>
    public Vector3 GetStartPointPosition()
    {
        return GetStartPoint();
    }

    /// <summary>
    /// Установить начальную точку
    /// </summary>
    public void SetStartPoint(Vector3 newStartPoint)
    {
        startPointMode = StartPointMode.UseCustomPoint;
        customStartPoint = newStartPoint;
    }

    /// <summary>
    /// Установить начальную точку как позицию другого Transform
    /// </summary>
    public void SetStartPoint(Transform newStartTransform)
    {
        startPointMode = StartPointMode.UseOtherTransform;
        startTransform = newStartTransform;
    }

    /// <summary>
    /// Переместить цепочку к новой начальной точке
    /// </summary>
    public void MoveChainToStartPoint(Vector3 newStartPoint)
    {
        if (points == null || points.Length == 0) return;

        // Вычисляем смещение
        Vector3 offset = newStartPoint - points[0];

        // Применяем смещение ко всем точкам
        for (int i = 0; i < points.Length; i++)
        {
            points[i] += offset;
        }

        SetStartPoint(newStartPoint);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CompactVectorChain))]
public class CompactVectorChainEditor : Editor
{
    private Vector2 scrollPos;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CompactVectorChain script = (CompactVectorChain)target;

        GUILayout.Space(10);

        // Основные кнопки
        if (GUILayout.Button("Сгенерировать цепочку", GUILayout.Height(30)))
        {
            script.GenerateChain();
            EditorUtility.SetDirty(script);
            SceneView.RepaintAll();
        }

        GUILayout.Space(5);

        EditorGUILayout.LabelField("Управление", EditorStyles.boldLabel);

        // Кнопки рандомизации
        EditorGUILayout.LabelField("Рандомизация углов", EditorStyles.miniBoldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Все углы"))
        {
            script.RandomizeAllAngles();
            EditorUtility.SetDirty(script);
        }

        if (GUILayout.Button("Только азимуты"))
        {
            script.RandomizeAzimuths();
            EditorUtility.SetDirty(script);
        }

        if (GUILayout.Button("Только углы места"))
        {
            script.RandomizeElevations();
            EditorUtility.SetDirty(script);
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Полная рандомизация (все параметры)"))
        {
            script.RandomizeAll();
            EditorUtility.SetDirty(script);
        }

        GUILayout.Space(5);

        // Управление сегментами
        EditorGUILayout.LabelField("Управление сегментами", EditorStyles.miniBoldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Добавить сегмент"))
        {
            script.AddSegment();
            EditorUtility.SetDirty(script);
        }

        if (GUILayout.Button("Удалить последний"))
        {
            script.RemoveLastSegment();
            EditorUtility.SetDirty(script);
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5);
        // Кнопки для начальной точки
        EditorGUILayout.LabelField("Начальная точка", EditorStyles.miniBoldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Исп. позицию объекта"))
        {
            script.SetStartPointToCurrentPosition();
            EditorUtility.SetDirty(script);
        }

        if (GUILayout.Button("Исп. кастомную точку"))
        {
            script.SetStartPointToCustom();
            EditorUtility.SetDirty(script);
        }

        if (GUILayout.Button("Захватить позицию"))
        {
            script.CaptureCurrentPosition();
            EditorUtility.SetDirty(script);
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);
        // Сохранение точек
        EditorGUILayout.LabelField("Сохранение точек", EditorStyles.miniBoldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Сохранить текущие"))
        {
            script.SaveCurrentPoints();
            EditorUtility.SetDirty(script);
        }

        if (GUILayout.Button("Загрузить"))
        {
            script.LoadPoints();
            EditorUtility.SetDirty(script);
            SceneView.RepaintAll();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Удалить сохраненные"))
        {
            script.DeleteSavedPoints();
            EditorUtility.SetDirty(script);
        }

        if (GUILayout.Button("Удалить все"))
        {
            script.DeleteAllSavedPoints();
            EditorUtility.SetDirty(script);
        }
        EditorGUILayout.EndHorizontal();

        // Список сохраненных точек
        if (script.savedPoints.Count > 0)
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Сохраненные точки", EditorStyles.miniBoldLabel);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(100));

            foreach (var kvp in script.savedPoints)
            {
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button(kvp.Key, GUILayout.Width(150)))
                {
                    script.saveName = kvp.Key;
                    script.LoadPoints();
                    EditorUtility.SetDirty(script);
                    SceneView.RepaintAll();
                }

                EditorGUILayout.LabelField($" - {kvp.Value.Length} точек");

                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    script.savedPoints.Remove(kvp.Key);
                    EditorUtility.SetDirty(script);
                    break;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        GUILayout.Space(5);

        // Сброс
        if (GUILayout.Button("Сбросить к значениям по умолчанию"))
        {
            script.ResetToDefault();
            EditorUtility.SetDirty(script);
        }

        // Информация
        if (script.GetPoints() != null && script.GetPoints().Length > 0)
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Информация", EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField($"Текущих точек: {script.GetPoints().Length}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"Сохраненных наборов: {script.savedPoints.Count}", EditorStyles.miniLabel);
           // EditorGUILayout.LabelField($"Общая длина: {script.GetPoints().Length > 0 ?                Vector3.Distance(script.GetPoints()[0], script.GetPoints()[script.GetPoints().Length - 1]) : 0:F2}",                EditorStyles.miniLabel);
        }
    }
}
#endif