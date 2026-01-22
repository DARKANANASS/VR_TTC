using UnityEngine;
using System.Collections.Generic;
using System.IO;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class VariableConfigReader : MonoBehaviour
{
    [Header("Источник данных")]
    public VariableConfig variableConfig;

    [Header("Текущие значения")]
    [SerializeField] private float currentAllLength;
    [SerializeField] private int currentAllPoints;
    [SerializeField] private float currentMaxDist;
    [SerializeField] private float currentMinDist;
    [SerializeField] private string currentName;
    [SerializeField] private int currentAxis;
    [SerializeField] private float currentTotalDistance;

    [Header("Стартовые точки")]
    [SerializeField] private Vector3[] startPoints = new Vector3[] { Vector3.zero };
    [SerializeField] private int selectedStartPointIndex = 0;

    [Header("Направление траектории")]
    [SerializeField] private TrajectoryDirection trajectoryDirection = TrajectoryDirection.Random;
    private enum TrajectoryDirection
    {
        Random,
        Forward,
        Backward,
        Right,
        Left,
        Up,
        Down
    }

    [Header("Обнуление координат")]
    [SerializeField] private bool resetX = false;
    [SerializeField] private bool resetY = false;
    [SerializeField] private bool resetZ = false;

    [Header("Генерация точек")]
    [SerializeField] private GameObject pointPrefab;
    [SerializeField] private Transform pointsContainer;
    [SerializeField] private Color pointColor = Color.red;
    [SerializeField] private float pointGizmoSize = 0.1f;

    [Header("Данные траектории")]
    [SerializeField] private Vector3[] trajPointsArray;
    [SerializeField] private Dictionary<string, Vector3[]> trajDictionary = new Dictionary<string, Vector3[]>();
    [SerializeField] private List<GameObject> spawnedPoints = new List<GameObject>();

    [Header("UI элементы (TextMeshPro)")]
    [SerializeField] private TMP_Dropdown directionDropdown;
    [SerializeField] private TMP_Dropdown startPointDropdown;
    [SerializeField] private TMP_Text totalDistanceText;
    [SerializeField] private UnityEngine.UI.Button updateDataButton;
    [SerializeField] private UnityEngine.UI.Button generatePointsButton;
    [SerializeField] private UnityEngine.UI.Button saveJsonButton;
    [SerializeField] private UnityEngine.UI.Button loadJsonButton;

    [Header("UI элементы для булевых переменных")]
    [SerializeField] private ToggleBoolUI resetXToggle;
    [SerializeField] private ToggleBoolUI resetYToggle;
    [SerializeField] private ToggleBoolUI resetZToggle;

    [Header("JSON файл")]
    [SerializeField] private string jsonFileName = "trajectories.json";

    private bool isInitialized = false;
    private bool pointsGenerated = false;
    private string jsonSavePath;
    private bool autoUpdateEnabled = true;

    void Start()
    {
        InitializeUI();
        InitializePaths();

        if (pointsContainer == null)
        {
            pointsContainer = new GameObject("TrajectoryPoints").transform;
            pointsContainer.SetParent(transform);
        }

        // Автоматическое обновление данных при старте
        if (autoUpdateEnabled && variableConfig != null)
        {
            UpdateDataFromConfig();
        }
    }

    void OnValidate()
    {
        // Автоматическое обновление данных при изменении VariableConfig в редакторе
        if (autoUpdateEnabled && variableConfig != null)
        {
            UpdateDataFromConfig();
        }
    }

    void Update()
    {
        if (pointsGenerated && spawnedPoints.Count > 0)
        {
            UpdatePointsPositions();
            UpdateDictionary();
            UpdateTotalDistance();
        }
    }

    void OnDrawGizmos()
    {
        if (!pointsGenerated || trajPointsArray == null || trajPointsArray.Length < 2) return;

        Gizmos.color = pointColor;
        foreach (Vector3 point in trajPointsArray)
        {
            Gizmos.DrawSphere(point, pointGizmoSize);
        }

        Gizmos.color = Color.blue;
        for (int i = 0; i < trajPointsArray.Length - 1; i++)
        {
            Gizmos.DrawLine(trajPointsArray[i], trajPointsArray[i + 1]);
        }
    }

    /// <summary>
    /// Инициализация UI элементов
    /// </summary>
    private void InitializeUI()
    {
        if (directionDropdown != null)
        {
            directionDropdown.ClearOptions();
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>
            {
                new TMP_Dropdown.OptionData("Случайное"),
                new TMP_Dropdown.OptionData("Вперед"),
                new TMP_Dropdown.OptionData("Назад"),
                new TMP_Dropdown.OptionData("Вправо"),
                new TMP_Dropdown.OptionData("Влево"),
                new TMP_Dropdown.OptionData("Вверх"),
                new TMP_Dropdown.OptionData("Вниз")
            };
            directionDropdown.AddOptions(options);
            directionDropdown.onValueChanged.AddListener(OnDirectionChanged);
        }

        if (startPointDropdown != null)
        {
            UpdateStartPointDropdown();
            startPointDropdown.onValueChanged.AddListener(OnStartPointChanged);
        }

        if (updateDataButton != null)
            updateDataButton.onClick.AddListener(UpdateDataFromUI);

        if (generatePointsButton != null)
            generatePointsButton.onClick.AddListener(GeneratePointsFromUI);

        if (saveJsonButton != null)
            saveJsonButton.onClick.AddListener(SaveAllTrajectoriesToJson);

        if (loadJsonButton != null)
            loadJsonButton.onClick.AddListener(LoadTrajectoryFromJson);

        // Инициализация UI для булевых переменных
        InitializeBoolUI();
    }

    /// <summary>
    /// Инициализация UI для булевых переменных
    /// </summary>
    private void InitializeBoolUI()
    {
        if (resetXToggle != null)
        {
            resetXToggle.Initialize("Обнулить X", resetX, value => resetX = value);
        }

        if (resetYToggle != null)
        {
            resetYToggle.Initialize("Обнулить Y", resetY, value => resetY = value);
        }

        if (resetZToggle != null)
        {
            resetZToggle.Initialize("Обнулить Z", resetZ, value => resetZ = value);
        }
    }

    /// <summary>
    /// Инициализация путей для сохранения
    /// </summary>
    private void InitializePaths()
    {
        jsonSavePath = Application.dataPath + "/Trajectories/";
        if (!Directory.Exists(jsonSavePath))
        {
            Directory.CreateDirectory(jsonSavePath);
        }
    }

    /// <summary>
    /// Автоматическое обновление данных из VariableConfig
    /// </summary>
    private void UpdateDataFromConfig()
    {
        if (variableConfig == null) return;

        currentAllLength = variableConfig.AllLength;
        currentAllPoints = variableConfig.AllPoint;
        currentMaxDist = variableConfig.cur_MaxDist;
        currentMinDist = variableConfig.cur_MinDist;
        currentName = variableConfig.cur_Name;
        currentAxis = variableConfig.cur_axis;

        isInitialized = true;
        Debug.Log("Данные автоматически обновлены из VariableConfig");
    }

    /// <summary>
    /// Обновляет данные из VariableConfig по кнопке UI
    /// </summary>
    public void UpdateDataFromUI()
    {
        if (variableConfig == null)
        {
            variableConfig = FindObjectOfType<VariableConfig>();
            if (variableConfig == null)
            {
                Debug.LogWarning("VariableConfig не найден!");
                return;
            }
        }

        UpdateDataFromConfig();
    }

    /// <summary>
    /// Генерация точек
    /// </summary>
    public void GeneratePointsFromUI()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("Сначала обновите данные!");
            return;
        }

        if (string.IsNullOrEmpty(currentName))
        {
            Debug.LogWarning("Имя траектории не задано в VariableConfig!");
            return;
        }

        ClearExistingPoints();
        GenerateTrajectoryPoints();
        SpawnPointObjects();
        UpdateDictionary();
        UpdateTotalDistance();
        pointsGenerated = true;
        Debug.Log($"Точки траектории '{currentName}' сгенерированы");
    }

    /// <summary>
    /// Сохранение ВСЕХ траекторий в единый JSON файл
    /// </summary>
    public void SaveAllTrajectoriesToJson()
    {
        if (trajDictionary.Count == 0)
        {
            Debug.LogWarning("Нет траекторий для сохранения!");
            return;
        }

        string filePath = jsonSavePath + jsonFileName;

        // Создаем список всех траекторий
        List<TrajectoryData> allTrajectories = new List<TrajectoryData>();

        foreach (var kvp in trajDictionary)
        {
            TrajectoryData data = new TrajectoryData
            {
                trajectoryName = kvp.Key,
                points = kvp.Value,
                totalDistance = CalculateTrajectoryLength(kvp.Value)
            };
            allTrajectories.Add(data);
        }

        AllTrajectoriesData allData = new AllTrajectoriesData
        {
            trajectories = allTrajectories.ToArray(),
            saveDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            totalTrajectories = allTrajectories.Count
        };

        string json = JsonUtility.ToJson(allData, true);
        File.WriteAllText(filePath, json);
        Debug.Log($"Все траектории ({allTrajectories.Count}) сохранены в: {filePath}");
    }

    /// <summary>
    /// Загрузка конкретной траектории из JSON по имени
    /// </summary>
    public void LoadTrajectoryFromJson()
    {
        if (string.IsNullOrEmpty(currentName))
        {
            Debug.LogWarning("Имя траектории не задано!");
            return;
        }

        string filePath = jsonSavePath + jsonFileName;
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            AllTrajectoriesData allData = JsonUtility.FromJson<AllTrajectoriesData>(json);

            // Ищем траекторию с нужным именем
            TrajectoryData targetTrajectory = null;
            foreach (var trajectory in allData.trajectories)
            {
                if (trajectory.trajectoryName == currentName)
                {
                    targetTrajectory = trajectory;
                    break;
                }
            }

            if (targetTrajectory != null)
            {
                trajPointsArray = targetTrajectory.points;
                currentTotalDistance = targetTrajectory.totalDistance;

                ClearExistingPoints();
                SpawnPointObjects();
                UpdateDictionary();
                UpdateTotalDistance();
                pointsGenerated = true;

                Debug.Log($"Траектория '{currentName}' загружена из файла");
            }
            else
            {
                Debug.LogWarning($"Траектория '{currentName}' не найдена в файле!");
            }
        }
        else
        {
            Debug.LogWarning($"Файл траекторий '{filePath}' не найден!");
        }
    }

    /// <summary>
    /// Генерирует массив точек траектории
    /// </summary>
    private void GenerateTrajectoryPoints()
    {
        trajPointsArray = new Vector3[currentAllPoints];

        // Выбираем стартовую точку
        Vector3 startPoint = GetSelectedStartPoint();
        trajPointsArray[0] = startPoint;

        float remainingLength = currentAllLength;
        int remainingPoints = currentAllPoints - 1;

        for (int i = 1; i < currentAllPoints; i++)
        {
            float maxPossibleDist = Mathf.Min(currentMaxDist, remainingLength);
            float minPossibleDist = Mathf.Max(currentMinDist, remainingLength - (remainingPoints - 1) * currentMaxDist);

            float distance = Random.Range(minPossibleDist, maxPossibleDist);
            Vector3 direction = GetDirectionBasedOnSelection();

            Vector3 newPoint = trajPointsArray[i - 1] + direction.normalized * distance;

            // Применяем обнуление координат
            newPoint = ApplyCoordinateReset(newPoint);

            trajPointsArray[i] = newPoint;

            remainingLength -= distance;
            remainingPoints--;
        }

        float actualLength = CalculateTotalLength();
        if (Mathf.Abs(actualLength - currentAllLength) > 0.001f)
        {
            Debug.LogWarning($"Длина траектории ({actualLength:F2}) не соответствует заданной ({currentAllLength:F2}). Выполняем корректировку...");
            NormalizeTrajectoryLength();
        }
    }

    /// <summary>
    /// Получает выбранную стартовую точку
    /// </summary>
    private Vector3 GetSelectedStartPoint()
    {
        if (startPoints.Length == 0) return Vector3.zero;

        if (selectedStartPointIndex >= 0 && selectedStartPointIndex < startPoints.Length)
        {
            return startPoints[selectedStartPointIndex];
        }

        return startPoints[Random.Range(0, startPoints.Length)];
    }

    /// <summary>
    /// Применяет обнуление координат к точке
    /// </summary>
    private Vector3 ApplyCoordinateReset(Vector3 point)
    {
        if (resetX) point.x = 0;
        if (resetY) point.y = 0;
        if (resetZ) point.z = 0;
        return point;
    }

    /// <summary>
    /// Получает направление на основе выбора
    /// </summary>
    private Vector3 GetDirectionBasedOnSelection()
    {
        switch (trajectoryDirection)
        {
            case TrajectoryDirection.Random:
                return GenerateRandomDirection();
            case TrajectoryDirection.Forward:
                return Vector3.forward;
            case TrajectoryDirection.Backward:
                return Vector3.back;
            case TrajectoryDirection.Right:
                return Vector3.right;
            case TrajectoryDirection.Left:
                return Vector3.left;
            case TrajectoryDirection.Up:
                return Vector3.up;
            case TrajectoryDirection.Down:
                return Vector3.down;
            default:
                return Vector3.forward;
        }
    }

    /// <summary>
    /// Генерирует случайное направление с учетом оси
    /// </summary>
    private Vector3 GenerateRandomDirection()
    {
        Vector3 direction = Random.onUnitSphere;

        switch (currentAxis)
        {
            case 0: // X
                direction = new Vector3(1, Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));
                break;
            case 1: // Y
                direction = new Vector3(Random.Range(-0.5f, 0.5f), 1, Random.Range(-0.5f, 0.5f));
                break;
            case 2: // Z
                direction = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 1);
                break;
        }

        return direction.normalized;
    }

    /// <summary>
    /// Создает GameObject для каждой точки
    /// </summary>
    private void SpawnPointObjects()
    {
        spawnedPoints.Clear();

        for (int i = 0; i < trajPointsArray.Length; i++)
        {
            GameObject pointObj;

            if (pointPrefab != null)
            {
                pointObj = Instantiate(pointPrefab, trajPointsArray[i], Quaternion.identity, pointsContainer);
            }
            else
            {
                pointObj = new GameObject($"Point_{i}");
                pointObj.transform.position = trajPointsArray[i];
                pointObj.transform.SetParent(pointsContainer);

#if UNITY_EDITOR
                var gizmo = pointObj.AddComponent<SceneGizmo>();
                gizmo.gizmoColor = pointColor;
                gizmo.gizmoSize = pointGizmoSize;
#endif
            }

            pointObj.name = $"TrajectoryPoint_{i}";
            spawnedPoints.Add(pointObj);
        }
    }

    /// <summary>
    /// Обновляет позиции точек при их перемещении
    /// </summary>
    private void UpdatePointsPositions()
    {
        for (int i = 0; i < spawnedPoints.Count; i++)
        {
            trajPointsArray[i] = spawnedPoints[i].transform.position;
        }

        AdjustPointDistances();

        for (int i = 0; i < spawnedPoints.Count; i++)
        {
            spawnedPoints[i].transform.position = trajPointsArray[i];
        }
    }

    /// <summary>
    /// Корректирует расстояния между точками
    /// </summary>
    private void AdjustPointDistances()
    {
        for (int i = trajPointsArray.Length - 1; i > 0; i--)
        {
            Vector3 direction = (trajPointsArray[i] - trajPointsArray[i - 1]).normalized;
            float currentDistance = Vector3.Distance(trajPointsArray[i], trajPointsArray[i - 1]);

            if (currentDistance < currentMinDist || currentDistance > currentMaxDist)
            {
                float targetDistance = (currentMinDist + currentMaxDist) / 2f;
                trajPointsArray[i] = trajPointsArray[i - 1] + direction * targetDistance;

                if (i < trajPointsArray.Length - 1)
                {
                    Vector3 offset = trajPointsArray[i + 1] - trajPointsArray[i];
                    trajPointsArray[i + 1] = trajPointsArray[i] + offset.normalized * targetDistance;
                }
            }
        }

        NormalizeTrajectoryLength();
    }

    /// <summary>
    /// Нормализует длину траектории до заданного значения
    /// </summary>
    private void NormalizeTrajectoryLength()
    {
        float currentLength = CalculateTotalLength();

        if (Mathf.Abs(currentLength - currentAllLength) < 0.001f) return;

        float scaleFactor = currentAllLength / currentLength;
        Vector3 center = CalculateTrajectoryCenter();

        for (int i = 0; i < trajPointsArray.Length; i++)
        {
            trajPointsArray[i] = center + (trajPointsArray[i] - center) * scaleFactor;
        }
    }

    /// <summary>
    /// Вычисляет центр траектории
    /// </summary>
    private Vector3 CalculateTrajectoryCenter()
    {
        Vector3 sum = Vector3.zero;
        foreach (Vector3 point in trajPointsArray)
        {
            sum += point;
        }
        return sum / trajPointsArray.Length;
    }

    /// <summary>
    /// Вычисляет общую длину траектории
    /// </summary>
    private float CalculateTotalLength()
    {
        return CalculateTrajectoryLength(trajPointsArray);
    }

    /// <summary>
    /// Вычисляет длину произвольной траектории
    /// </summary>
    private float CalculateTrajectoryLength(Vector3[] points)
    {
        if (points == null || points.Length < 2) return 0f;

        float total = 0f;
        for (int i = 0; i < points.Length - 1; i++)
        {
            total += Vector3.Distance(points[i], points[i + 1]);
        }
        return total;
    }

    /// <summary>
    /// Обновляет сумму расстояний в UI
    /// </summary>
    private void UpdateTotalDistance()
    {
        currentTotalDistance = CalculateTotalLength();

        if (totalDistanceText != null)
        {
            totalDistanceText.text = $"Сумма расстояний: {currentTotalDistance:F2}";
        }
    }

    /// <summary>
    /// Обновляет словарь траекторий
    /// </summary>
    private void UpdateDictionary()
    {
        if (string.IsNullOrEmpty(currentName))
        {
            Debug.LogWarning("Имя траектории не задано!");
            return;
        }

        if (trajDictionary.ContainsKey(currentName))
        {
            trajDictionary[currentName] = (Vector3[])trajPointsArray.Clone();
        }
        else
        {
            trajDictionary.Add(currentName, (Vector3[])trajPointsArray.Clone());
        }
    }

    /// <summary>
    /// Очищает существующие точки
    /// </summary>
    private void ClearExistingPoints()
    {
        foreach (GameObject point in spawnedPoints)
        {
            if (point != null)
                Destroy(point);
        }
        spawnedPoints.Clear();
    }

    /// <summary>
    /// Обработчик изменения направления
    /// </summary>
    private void OnDirectionChanged(int index)
    {
        trajectoryDirection = (TrajectoryDirection)index;
    }

    /// <summary>
    /// Обработчик изменения стартовой точки
    /// </summary>
    private void OnStartPointChanged(int index)
    {
        selectedStartPointIndex = index;
    }

    /// <summary>
    /// Обновляет dropdown стартовых точек
    /// </summary>
    private void UpdateStartPointDropdown()
    {
        if (startPointDropdown == null) return;

        startPointDropdown.ClearOptions();
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

        for (int i = 0; i < startPoints.Length; i++)
        {
            options.Add(new TMP_Dropdown.OptionData($"Точка {i}: {startPoints[i]}"));
        }

        startPointDropdown.AddOptions(options);
    }

    /// <summary>
    /// Возвращает массив точек траектории по имени
    /// </summary>
    public Vector3[] GetTrajectoryPoints(string name)
    {
        if (trajDictionary.ContainsKey(name))
            return trajDictionary[name];
        return null;
    }

    /// <summary>
    /// Возвращает все имена траекторий
    /// </summary>
    public string[] GetAllTrajectoryNames()
    {
        string[] names = new string[trajDictionary.Count];
        trajDictionary.Keys.CopyTo(names, 0);
        return names;
    }

    // Свойства
    public float AllLength => currentAllLength;
    public int AllPoints => currentAllPoints;
    public float MaxDistance => currentMaxDist;
    public float MinDistance => currentMinDist;
    public string ExperimentName => currentName;
    public int Axis => currentAxis;
    public Vector3[] CurrentTrajectoryPoints => trajPointsArray;
    public bool IsPointsGenerated => pointsGenerated;
    public float TotalDistance => currentTotalDistance;

    /// <summary>
    /// Класс для сериализации данных отдельной траектории
    /// </summary>
    [System.Serializable]
    private class TrajectoryData
    {
        public string trajectoryName;
        public Vector3[] points;
        public float totalDistance;
    }

    /// <summary>
    /// Класс для сериализации ВСЕХ траекторий
    /// </summary>
    [System.Serializable]
    private class AllTrajectoriesData
    {
        public TrajectoryData[] trajectories;
        public string saveDate;
        public int totalTrajectories;
    }
}

/// <summary>
/// Класс для управления UI переключателями булевых переменных
/// </summary>
[System.Serializable]
public class ToggleBoolUI
{
    public TMP_Text labelText;
    public UnityEngine.UI.Toggle toggle;

    public void Initialize(string label, bool initialValue, System.Action<bool> onValueChanged)
    {
        if (labelText != null)
            labelText.text = label;

        if (toggle != null)
        {
            toggle.isOn = initialValue;
            toggle.onValueChanged.AddListener((value) => onValueChanged?.Invoke(value));
        }
    }
}

#if UNITY_EDITOR
[ExecuteInEditMode]
public class SceneGizmo : MonoBehaviour
{
    public Color gizmoColor = Color.red;
    public float gizmoSize = 0.1f;

    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, gizmoSize);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, gizmoSize * 1.2f);
    }
}

/// <summary>
/// Custom Editor для VariableConfigReader
/// </summary>
[CustomEditor(typeof(VariableConfigReader))]
public class VariableConfigReaderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        VariableConfigReader reader = (VariableConfigReader)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Действия", EditorStyles.boldLabel);

        if (GUILayout.Button("Обновить данные"))
        {
            reader.UpdateDataFromUI();
        }

        if (GUILayout.Button("Сгенерировать точки"))
        {
            reader.GeneratePointsFromUI();
        }

        if (GUILayout.Button("Сохранить все траектории в JSON"))
        {
            reader.SaveAllTrajectoriesToJson();
        }

        if (GUILayout.Button("Загрузить траекторию из JSON"))
        {
            reader.LoadTrajectoryFromJson();
        }
    }
}
#endif