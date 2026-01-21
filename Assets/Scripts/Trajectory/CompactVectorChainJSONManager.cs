using UnityEngine;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CompactVectorChainJSONManager : MonoBehaviour
{
    [Header("Цепочка векторов")]
    public CompactVectorChain targetChain;
    
    [Header("Файлы JSON")]
    public string jsonFileName = "Trajectory_array.json";
    public string customJsonData = "";
    public string folderPath;

    [Header("Сохранение")]
    public bool autoSaveOnPlay = false;
    public bool autoLoadOnStart = false;
    
    [Header("Информация")]
    [SerializeField] public List<string> loadedCollectionNames = new List<string>();
    [SerializeField] public int totalLoadedPoints = 0;
    
    void Start()
    {
        if (targetChain == null)
            targetChain = GetComponent<CompactVectorChain>();
        
        if (autoLoadOnStart && targetChain != null)
        {
            LoadFromFile();
        }
    }
    
    /// <summary>
    /// Сохранить все точки из targetChain в JSON файл
    /// </summary>
    [ContextMenu("Save All to JSON File")]
    public void SaveToFile()
    {
        if (targetChain == null)
        {
            Debug.LogError("Target chain не назначен!");
            return;
        }
        
        if (string.IsNullOrEmpty(jsonFileName))
        {
            Debug.LogError("Имя файла не может быть пустым!");
            return;
        }
        
        // Получаем JSON из целевой цепочки
        string json = targetChain.ExportToJSON();
        
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogWarning("Нет данных для сохранения!");
            return;
        }
        
        // Определяем путь для сохранения
        string filePath = GetFilePath(jsonFileName);
        
        try
        {
            // Сохраняем в файл
            File.WriteAllText(filePath, json);
            Debug.Log($"Данные сохранены в файл: {filePath}");
            Debug.Log($"Сохранено коллекций: {targetChain.savedPoints.Count}");
            
            // Обновляем поле для просмотра
            customJsonData = json;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка сохранения файла: {e.Message}");
        }
    }
    
    /// <summary>
    /// Загрузить точки из JSON файла в targetChain
    /// </summary>
    [ContextMenu("Load from JSON File")]
    public void LoadFromFile()
    {
        if (targetChain == null)
        {
            Debug.LogError("Target chain не назначен!");
            return;
        }
        
        if (string.IsNullOrEmpty(jsonFileName))
        {
            Debug.LogError("Имя файла не может быть пустым!");
            return;
        }
        
        string filePath = GetFilePath(jsonFileName);
        
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"Файл не найден: {filePath}");
            return;
        }
        
        try
        {
            // Читаем файл
            string json = File.ReadAllText(filePath);
            
            // Импортируем в цепочку
            targetChain.ImportFromJSON(json);
            
            // Обновляем информацию
            UpdateLoadedInfo();
            customJsonData = json;
            
            Debug.Log($"Данные загружены из файла: {filePath}");
            Debug.Log($"Загружено коллекций: {loadedCollectionNames.Count}");
            Debug.Log($"Всего точек: {totalLoadedPoints}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка загрузки файла: {e.Message}");
        }
    }
    
    /// <summary>
    /// Сохранить customJsonData в файл
    /// </summary>
    [ContextMenu("Save Custom JSON to File")]
    public void SaveCustomToFile()
    {
        if (string.IsNullOrEmpty(customJsonData))
        {
            Debug.LogWarning("Нет данных для сохранения!");
            return;
        }
        
        if (string.IsNullOrEmpty(jsonFileName))
        {
            Debug.LogError("Имя файла не может быть пустым!");
            return;
        }
        
        string filePath = GetFilePath(jsonFileName);
        
        try
        {
            File.WriteAllText(filePath, customJsonData);
            Debug.Log($"Custom JSON сохранен в файл: {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка сохранения файла: {e.Message}");
        }
    }
    
    /// <summary>
    /// Загрузить JSON из файла в customJsonData
    /// </summary>
    [ContextMenu("Load JSON to Custom Field")]
    public void LoadToCustomField()
    {
        if (string.IsNullOrEmpty(jsonFileName))
        {
            Debug.LogError("Имя файла не может быть пустым!");
            return;
        }
        
        string filePath = GetFilePath(jsonFileName);
        
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"Файл не найден: {filePath}");
            return;
        }
        
        try
        {
            customJsonData = File.ReadAllText(filePath);
            Debug.Log($"JSON загружен в поле из файла: {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка загрузки файла: {e.Message}");
        }
    }
    
    /// <summary>
    /// Импортировать customJsonData в targetChain
    /// </summary>
    [ContextMenu("Import Custom JSON to Chain")]
    public void ImportCustomJSON()
    {
        if (targetChain == null)
        {
            Debug.LogError("Target chain не назначен!");
            return;
        }
        
        if (string.IsNullOrEmpty(customJsonData))
        {
            Debug.LogWarning("Нет данных для импорта!");
            return;
        }
        
        try
        {
            targetChain.ImportFromJSON(customJsonData);
            UpdateLoadedInfo();
            Debug.Log($"Custom JSON импортирован в цепочку");
            Debug.Log($"Загружено коллекций: {loadedCollectionNames.Count}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка импорта JSON: {e.Message}");
        }
    }
    
    /// <summary>
    /// Экспортировать из targetChain в customJsonData
    /// </summary>
    [ContextMenu("Export Chain to Custom JSON")]
    public void ExportToCustomJSON()
    {
        if (targetChain == null)
        {
            Debug.LogError("Target chain не назначен!");
            return;
        }
        
        customJsonData = targetChain.ExportToJSON();
        Debug.Log($"Данные экспортированы в поле customJsonData");
    }
    
    /// <summary>
    /// Очистить customJsonData
    /// </summary>
    [ContextMenu("Clear Custom JSON")]
    public void ClearCustomJSON()
    {
        customJsonData = "";
        Debug.Log("Custom JSON очищен");
    }
    
    /// <summary>
    /// Очистить все данные в targetChain
    /// </summary>
    [ContextMenu("Clear All in Chain")]
    public void ClearChainData()
    {
        if (targetChain == null)
        {
            Debug.LogError("Target chain не назначен!");
            return;
        }
        
        targetChain.DeleteAllSavedPoints();
        loadedCollectionNames.Clear();
        totalLoadedPoints = 0;
        Debug.Log("Все данные в цепочке очищены");
    }
    
    /// <summary>
    /// Показать список всех сохраненных коллекций в консоли
    /// </summary>
    [ContextMenu("Show Collections Info")]
    public void ShowCollectionsInfo()
    {
        if (targetChain == null)
        {
            Debug.LogError("Target chain не назначен!");
            return;
        }
        
        UpdateLoadedInfo();
        
        Debug.Log("=== Информация о коллекциях ===");
        Debug.Log($"Всего коллекций: {loadedCollectionNames.Count}");
        Debug.Log($"Всего точек: {totalLoadedPoints}");
        
        foreach (var name in loadedCollectionNames)
        {
            var points = targetChain.GetSavedPoints(name);
            if (points != null)
            {
                Debug.Log($"  '{name}': {points.Length} точек");
            }
        }
    }
    
    /// <summary>
    /// Копировать JSON в буфер обмена (только в редакторе)
    /// </summary>
    [ContextMenu("Copy JSON to Clipboard")]
    public void CopyToClipboard()
    {
        #if UNITY_EDITOR
        if (string.IsNullOrEmpty(customJsonData))
        {
            Debug.LogWarning("Нет данных для копирования!");
            return;
        }
        
        GUIUtility.systemCopyBuffer = customJsonData;
        Debug.Log("JSON скопирован в буфер обмена");
        #else
        Debug.LogWarning("Копирование в буфер обмена доступно только в редакторе Unity");
        #endif
    }
    
    /// <summary>
    /// Вставить JSON из буфера обмена (только в редакторе)
    /// </summary>
    [ContextMenu("Paste JSON from Clipboard")]
    public void PasteFromClipboard()
    {
        #if UNITY_EDITOR
        customJsonData = GUIUtility.systemCopyBuffer;
        Debug.Log("JSON вставлен из буфера обмена");
        #else
        Debug.LogWarning("Вставка из буфера обмена доступна только в редакторе Unity");
        #endif
    }
    
    /// <summary>
    /// Получить полный путь к файлу
    /// </summary>
    private string GetFilePath(string fileName)
    {
        // В редакторе сохраняем в папку проекта
        #if UNITY_EDITOR
        folderPath = Application.dataPath + "/../TrajectoryJSON/";
#else
        // В билде используем persistentDataPath
        string folderPath = Application.persistentDataPath + "/TrajectoryJSON/";
#endif

        // Создаем папку если не существует
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        
        return Path.Combine(folderPath, fileName);
    }
    
    /// <summary>
    /// Обновить информацию о загруженных данных
    /// </summary>
    private void UpdateLoadedInfo()
    {
        if (targetChain == null) return;
        
        loadedCollectionNames = targetChain.GetSavedPointNames();
        totalLoadedPoints = 0;
        
        foreach (var name in loadedCollectionNames)
        {
            var points = targetChain.GetSavedPoints(name);
            if (points != null)
            {
                totalLoadedPoints += points.Length;
            }
        }
    }
    
    /// <summary>
    /// Получить путь к папке с JSON файлами
    /// </summary>
    public string GetJsonFolderPath()
    {
        #if UNITY_EDITOR
        return Application.dataPath + "/../TrajectoryJSON/";
#else
        return Application.persistentDataPath + "/TrajectoryJSON/";
#endif
    }

    /// <summary>
    /// Открыть папку с JSON файлами (только в редакторе)
    /// </summary>
    [ContextMenu("Open JSON Folder")]
    public void OpenJsonFolder()
    {
        #if UNITY_EDITOR
        string folderPath = GetJsonFolderPath();
        
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        
        EditorUtility.RevealInFinder(folderPath);
        #else
        Debug.LogWarning("Открытие папки доступно только в редакторе Unity");
        #endif
    }
    
    /// <summary>
    /// Создать тестовый JSON с примерами данных
    /// </summary>
    [ContextMenu("Create Test JSON")]
    public void CreateTestJSON()
    {
        // Создаем тестовые данные
        var testData = new CompactVectorChain.ExportData
        {
            savedPoints = new List<CompactVectorChain.PointCollection>
            {
                new CompactVectorChain.PointCollection
                {
                    name = "TestChain1",
                    points = new Vector3[]
                    {
                        new Vector3(0, 0, 0),
                        new Vector3(2, 1, 0),
                        new Vector3(4, 0, 2),
                        new Vector3(6, -1, 0)
                    }
                },
                new CompactVectorChain.PointCollection
                {
                    name = "TestChain2",
                    points = new Vector3[]
                    {
                        new Vector3(0, 0, 0),
                        new Vector3(0, 3, 0),
                        new Vector3(2, 3, 2),
                        new Vector3(2, 0, 2),
                        new Vector3(0, 0, 0)
                    }
                },
                new CompactVectorChain.PointCollection
                {
                    name = "TestChain3",
                    points = new Vector3[]
                    {
                        new Vector3(0, 0, 0),
                        new Vector3(1, 2, 1),
                        new Vector3(2, 0, 2),
                        new Vector3(3, 2, 3),
                        new Vector3(4, 0, 4)
                    }
                }
            }
        };
        
        customJsonData = JsonUtility.ToJson(testData, true);
        Debug.Log("Создан тестовый JSON с 3 коллекциями точек");
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CompactVectorChainJSONManager))]
public class CompactVectorChainJSONManagerEditor : Editor
{
    private Vector2 scrollPos;
    private bool showJsonPreview = false;
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        CompactVectorChainJSONManager script = (CompactVectorChainJSONManager)target;
        
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Управление JSON", EditorStyles.boldLabel);
        
        // Информация о файле
        EditorGUILayout.LabelField("Файл:", EditorStyles.miniBoldLabel);
        EditorGUILayout.LabelField($"Путь: {script.GetJsonFolderPath()}{script.jsonFileName}", EditorStyles.miniLabel);
        
        GUILayout.Space(5);
        
        // Основные операции с файлами
        EditorGUILayout.LabelField("Файловые операции", EditorStyles.miniBoldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Сохранить в файл", GUILayout.Height(25)))
        {
            script.SaveToFile();
            EditorUtility.SetDirty(script);
        }
        
        if (GUILayout.Button("Загрузить из файла", GUILayout.Height(25)))
        {
            script.LoadFromFile();
            EditorUtility.SetDirty(script);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Открыть папку", GUILayout.Width(120)))
        {
            script.OpenJsonFolder();
        }
        
        if (GUILayout.Button("Тестовый JSON", GUILayout.Width(120)))
        {
            script.CreateTestJSON();
            EditorUtility.SetDirty(script);
        }
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Space(5);
        
        // Операции с customJsonData
        EditorGUILayout.LabelField("Работа с полем JSON", EditorStyles.miniBoldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Экспорт → Поле"))
        {
            script.ExportToCustomJSON();
            EditorUtility.SetDirty(script);
        }
        
        if (GUILayout.Button("Импорт ← Поле"))
        {
            script.ImportCustomJSON();
            EditorUtility.SetDirty(script);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Сохранить поле → Файл"))
        {
            script.SaveCustomToFile();
            EditorUtility.SetDirty(script);
        }
        
        if (GUILayout.Button("Загрузить поле ← Файл"))
        {
            script.LoadToCustomField();
            EditorUtility.SetDirty(script);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Копировать в буфер", GUILayout.Width(150)))
        {
            script.CopyToClipboard();
        }
        
        if (GUILayout.Button("Вставить из буфера", GUILayout.Width(150)))
        {
            script.PasteFromClipboard();
            EditorUtility.SetDirty(script);
        }
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Space(5);
        
        // Очистка
        EditorGUILayout.LabelField("Очистка", EditorStyles.miniBoldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Очистить поле JSON", GUILayout.Width(150)))
        {
            script.ClearCustomJSON();
            EditorUtility.SetDirty(script);
        }
        
        if (GUILayout.Button("Очистить цепочку", GUILayout.Width(150)))
        {
            script.ClearChainData();
            EditorUtility.SetDirty(script);
        }
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Space(5);
        
        // Информация
        if (GUILayout.Button("Показать информацию", GUILayout.Height(25)))
        {
            script.ShowCollectionsInfo();
        }
        
        // Превью JSON
        GUILayout.Space(10);
        showJsonPreview = EditorGUILayout.Foldout(showJsonPreview, "Превью JSON", true);
        
        if (showJsonPreview && !string.IsNullOrEmpty(script.customJsonData))
        {
            GUILayout.Space(5);
            EditorGUILayout.LabelField("Содержимое JSON:", EditorStyles.miniBoldLabel);
            
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(200));
            
            string displayText = script.customJsonData;
            if (displayText.Length > 5000)
            {
                displayText = displayText.Substring(0, 5000) + "\n\n... (еще " + (displayText.Length - 5000) + " символов)";
            }
            
            GUIStyle textAreaStyle = new GUIStyle(EditorStyles.textArea);
            textAreaStyle.wordWrap = true;
            
            EditorGUILayout.TextArea(displayText, textAreaStyle);
            
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.LabelField($"Длина: {script.customJsonData.Length} символов", EditorStyles.miniLabel);
        }
        
        // Статистика
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Статистика", EditorStyles.miniBoldLabel);
        
        if (script.targetChain != null)
        {
            EditorGUILayout.LabelField($"Коллекций в цепочке: {script.targetChain.savedPoints.Count}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"Загружено имен: {script.loadedCollectionNames.Count}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"Всего точек: {script.totalLoadedPoints}", EditorStyles.miniLabel);
        }
        
        // Предупреждение если нет targetChain
        if (script.targetChain == null)
        {
            GUILayout.Space(10);
            EditorGUILayout.HelpBox("Target Chain не назначен! Назначьте CompactVectorChain компонент.", MessageType.Warning);
        }
    }
}
#endif