using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.IO;

public class CorrectDataContainer : MonoBehaviour
{
    [Header("UI References - Path")]
    [Tooltip("Поле для ввода пути к файлу JSON")]
    public TMP_InputField jsonPathInput;

    [Tooltip("Кнопка сохранения изменений")]
    public UnityEngine.UI.Button saveButton;

    [Tooltip("Кнопка восстановления значений по умолчанию")]
    public UnityEngine.UI.Button resetButton;

    [Header("UI References - Speed Arrays")]
    [Tooltip("Поля для Speed1Dur (3 элемента)")]
    public TMP_InputField[] speed1DurFields;

    [Tooltip("Поля для Speed2Dur (3 элемента)")]
    public TMP_InputField[] speed2DurFields;

    [Header("UI References - Distance Settings")]
    [Tooltip("Поле для MaxDistLowSpeed")]
    public TMP_InputField maxDistLowSpeedField;

    [Tooltip("Поле для MinDistLowSpeed")]
    public TMP_InputField minDistLowSpeedField;

    [Tooltip("Поле для MaxDistHighSpeed")]
    public TMP_InputField maxDistHighSpeedField;

    [Tooltip("Поле для MinDistHighSpeed")]
    public TMP_InputField minDistHighSpeedField;

    [Header("UI References - Occlusion Settings")]
    [Tooltip("Поле для PointsOccl")]
    public TMP_InputField pointsOcclField;

    [Tooltip("Поле для OcclDurLow")]
    public TMP_InputField occlDurLowField;

    [Tooltip("Поле для OcclDurHigh")]
    public TMP_InputField occlDurHighField;

    [Header("UI References - Point Count")]
    [Tooltip("Поля для pointCount (3 элемента)")]
    public TMP_InputField[] pointCountFields;

    [Header("Data Arrays")]
    [Tooltip("Первый набор: 1.0, 3.2, 5.8")]
    public float[] Speed1Dur = new float[] { 1.0f, 3.2f, 5.8f };

    [Tooltip("Второй набор: 2.0, 6.4, 11.6")]
    public float[] Speed2Dur = new float[] { 2.0f, 6.4f, 11.6f };

    [Header("Distance Settings")]
    [Tooltip("Максимальное расстояние для LowSpeed")]
    public float MaxDistLowSpeed = 0.65f;

    [Tooltip("Минимальное расстояние для LowSpeed")]
    public float MinDistLowSpeed = 0.35f;

    [Tooltip("Максимальное расстояние для HighSpeed")]
    public float MaxDistHighSpeed = 1.3f;

    [Tooltip("Минимальное расстояние для HighSpeed")]
    public float MinDistHighSpeed = 0.7f;

    [Header("Occlusion Settings")]
    [Tooltip("Количество точек для окклюзии")]
    public int PointsOccl = 2;

    [Tooltip("Длительность окклюзии для LowSpeed")]
    public float OcclDurLow = 1f;

    [Tooltip("Длительность окклюзии для HighSpeed")]
    public float OcclDurHigh = 2f;

    [Header("Configuration Lists")]
    [Tooltip("Список количества точек: 3, 7, 13")]
    public List<int> pointCount = new List<int> { 3, 7, 13 };

    // Контейнер для хранения исходных значений
    private DefaultValues defaultValues;

    // Текущий путь к файлу JSON
    private string currentJsonPath;

    // Путь к файлу с дефолтными значениями
    private string defaultValuesPath;

    // Внутренний сериализуемый класс
    [System.Serializable]
    public class ContainerData
    {
        public float[] Speed1Dur;
        public float[] Speed2Dur;
        public float MaxDistLowSpeed;
        public float MinDistLowSpeed;
        public float MaxDistHighSpeed;
        public float MinDistHighSpeed;
        public int PointsOccl;
        public float OcclDurLow;
        public float OcclDurHigh;
        public List<int> pointCount;
    }

    // Класс для хранения исходных значений
    [System.Serializable]
    public class DefaultValues
    {
        public float[] Speed1Dur = new float[] { 1.0f, 3.2f, 5.8f };
        public float[] Speed2Dur = new float[] { 2.0f, 6.4f, 11.6f };
        public float MaxDistLowSpeed = 0.65f;
        public float MinDistLowSpeed = 0.35f;
        public float MaxDistHighSpeed = 1.3f;
        public float MinDistHighSpeed = 0.7f;
        public int PointsOccl = 2;
        public float OcclDurLow = 1f;
        public float OcclDurHigh = 2f;
        public List<int> pointCount = new List<int> { 3, 7, 13 };
    }

    void Start()
    {
        // Определяем путь к папке Data рядом с программой
        string programPath = Application.dataPath;
        string dataFolderPath;

        // В редакторе Unity используем папку Assets/Data
#if UNITY_EDITOR
        dataFolderPath = Path.Combine(programPath, "Data");
#else
        // В собранной версии используем папку Data рядом с исполняемым файлом
        dataFolderPath = Path.Combine(Path.GetDirectoryName(programPath), "Data");
#endif

        // Создаем папку Data, если она не существует
        if (!Directory.Exists(dataFolderPath))
        {
            Directory.CreateDirectory(dataFolderPath);
        }

        // Устанавливаем пути к файлам
        currentJsonPath = Path.Combine(dataFolderPath, "config.json");
        defaultValuesPath = Path.Combine(dataFolderPath, "defaultVal.json");

        // Загружаем дефолтные значения из файла или создаем их
        LoadDefaultValues();

        // Загружаем значения из JSON при старте
        if (File.Exists(currentJsonPath))
        {
            LoadFromJson(currentJsonPath);
        }

        // Инициализируем UI элементы
        InitializeUI();

        // Загружаем путь по умолчанию в поле ввода
        if (jsonPathInput != null)
        {
            jsonPathInput.text = currentJsonPath;
        }

        // Обновляем UI значениями из переменных
        UpdateUIFromData();
    }

    // Загрузка дефолтных значений из файла
    private void LoadDefaultValues()
    {
        if (File.Exists(defaultValuesPath))
        {
            try
            {
                string json = File.ReadAllText(defaultValuesPath);
                defaultValues = JsonUtility.FromJson<DefaultValues>(json);
                Debug.Log($"Дефолтные значения загружены из: {defaultValuesPath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Ошибка при загрузке дефолтных значений: {e.Message}");
                CreateDefaultValues();
            }
        }
        else
        {
            Debug.LogWarning($"Файл дефолтных значений не найден: {defaultValuesPath}. Создаются новые значения.");
            CreateDefaultValues();
        }
    }

    // Создание и сохранение дефолтных значений
    private void CreateDefaultValues()
    {
        defaultValues = new DefaultValues();

        // Сохраняем дефолтные значения в файл
        try
        {
            string json = JsonUtility.ToJson(defaultValues, true);
            File.WriteAllText(defaultValuesPath, json);
            Debug.Log($"Дефолтные значения сохранены в: {defaultValuesPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка при сохранении дефолтных значений: {e.Message}");
        }
    }

    // Инициализация UI элементов
    private void InitializeUI()
    {
        if (saveButton != null)
        {
            saveButton.onClick.AddListener(SaveChanges);
        }

        if (resetButton != null)
        {
            resetButton.onClick.AddListener(ResetToDefaults);
        }

        // Добавляем обработчики для полей ввода
        SetupInputFieldListeners();
    }

    // Настройка обработчиков для полей ввода
    private void SetupInputFieldListeners()
    {
        // Обработчики для полей Speed1Dur
        if (speed1DurFields != null)
        {
            for (int i = 0; i < speed1DurFields.Length; i++)
            {
                int index = i; // Захватываем индекс для замыкания
                if (speed1DurFields[i] != null)
                {
                    speed1DurFields[i].onEndEdit.AddListener((value) =>
                        OnSpeed1DurFieldChanged(index, value));
                }
            }
        }

        // Обработчики для полей Speed2Dur
        if (speed2DurFields != null)
        {
            for (int i = 0; i < speed2DurFields.Length; i++)
            {
                int index = i;
                if (speed2DurFields[i] != null)
                {
                    speed2DurFields[i].onEndEdit.AddListener((value) =>
                        OnSpeed2DurFieldChanged(index, value));
                }
            }
        }

        // Обработчики для полей PointCount
        if (pointCountFields != null)
        {
            for (int i = 0; i < pointCountFields.Length; i++)
            {
                int index = i;
                if (pointCountFields[i] != null)
                {
                    pointCountFields[i].onEndEdit.AddListener((value) =>
                        OnPointCountFieldChanged(index, value));
                }
            }
        }

        // Обработчики для полей окклюзии
        if (pointsOcclField != null)
            pointsOcclField.onEndEdit.AddListener(OnPointsOcclChanged);

        if (occlDurLowField != null)
            occlDurLowField.onEndEdit.AddListener(OnOcclDurLowChanged);

        if (occlDurHighField != null)
            occlDurHighField.onEndEdit.AddListener(OnOcclDurHighChanged);

        // Обработчики для остальных полей
        if (maxDistLowSpeedField != null)
            maxDistLowSpeedField.onEndEdit.AddListener(OnMaxDistLowSpeedChanged);

        if (minDistLowSpeedField != null)
            minDistLowSpeedField.onEndEdit.AddListener(OnMinDistLowSpeedChanged);

        if (maxDistHighSpeedField != null)
            maxDistHighSpeedField.onEndEdit.AddListener(OnMaxDistHighSpeedChanged);

        if (minDistHighSpeedField != null)
            minDistHighSpeedField.onEndEdit.AddListener(OnMinDistHighSpeedChanged);

        if (jsonPathInput != null)
            jsonPathInput.onEndEdit.AddListener(OnPathInputChanged);
    }

    // Методы для обработки изменений в полях ввода
    private void OnSpeed1DurFieldChanged(int index, string value)
    {
        if (float.TryParse(value, out float result))
        {
            if (index < Speed1Dur.Length)
            {
                Speed1Dur[index] = result;
            }
        }
    }

    private void OnSpeed2DurFieldChanged(int index, string value)
    {
        if (float.TryParse(value, out float result))
        {
            if (index < Speed2Dur.Length)
            {
                Speed2Dur[index] = result;
            }
        }
    }

    private void OnPointCountFieldChanged(int index, string value)
    {
        if (int.TryParse(value, out int result))
        {
            if (index < pointCount.Count)
            {
                pointCount[index] = result;
            }
        }
    }

    private void OnPointsOcclChanged(string value)
    {
        if (int.TryParse(value, out int result))
        {
            PointsOccl = result;
        }
    }

    private void OnOcclDurLowChanged(string value)
    {
        if (float.TryParse(value, out float result))
        {
            OcclDurLow = result;
        }
    }

    private void OnOcclDurHighChanged(string value)
    {
        if (float.TryParse(value, out float result))
        {
            OcclDurHigh = result;
        }
    }

    private void OnMaxDistLowSpeedChanged(string value)
    {
        if (float.TryParse(value, out float result))
            MaxDistLowSpeed = result;
    }

    private void OnMinDistLowSpeedChanged(string value)
    {
        if (float.TryParse(value, out float result))
            MinDistLowSpeed = result;
    }

    private void OnMaxDistHighSpeedChanged(string value)
    {
        if (float.TryParse(value, out float result))
            MaxDistHighSpeed = result;
    }

    private void OnMinDistHighSpeedChanged(string value)
    {
        if (float.TryParse(value, out float result))
            MinDistHighSpeed = result;
    }

    // Метод для округления float до 2 знаков
    private float RoundFloat(float value)
    {
        return Mathf.Round(value * 100f) / 100f;
    }

    // Метод для создания округленной копии данных для JSON
    private ContainerData CreateRoundedDataForJson()
    {
        // Создаем округленные копии массивов
        float[] roundedSpeed1Dur = new float[Speed1Dur.Length];
        for (int i = 0; i < Speed1Dur.Length; i++)
        {
            roundedSpeed1Dur[i] = RoundFloat(Speed1Dur[i]);
        }

        float[] roundedSpeed2Dur = new float[Speed2Dur.Length];
        for (int i = 0; i < Speed2Dur.Length; i++)
        {
            roundedSpeed2Dur[i] = RoundFloat(Speed2Dur[i]);
        }

        return new ContainerData
        {
            Speed1Dur = roundedSpeed1Dur,
            Speed2Dur = roundedSpeed2Dur,
            MaxDistLowSpeed = RoundFloat(MaxDistLowSpeed),
            MinDistLowSpeed = RoundFloat(MinDistLowSpeed),
            MaxDistHighSpeed = RoundFloat(MaxDistHighSpeed),
            MinDistHighSpeed = RoundFloat(MinDistHighSpeed),
            PointsOccl = PointsOccl,
            OcclDurLow = RoundFloat(OcclDurLow),
            OcclDurHigh = RoundFloat(OcclDurHigh),
            pointCount = new List<int>(pointCount) // pointCount остается без изменений, так как это целые числа
        };
    }

    // Обновление UI значениями из переменных
    private void UpdateUIFromData()
    {
        // Speed1Dur
        if (speed1DurFields != null)
        {
            for (int i = 0; i < Mathf.Min(speed1DurFields.Length, Speed1Dur.Length); i++)
            {
                if (speed1DurFields[i] != null)
                    speed1DurFields[i].text = Speed1Dur[i].ToString();
            }
        }

        // Speed2Dur
        if (speed2DurFields != null)
        {
            for (int i = 0; i < Mathf.Min(speed2DurFields.Length, Speed2Dur.Length); i++)
            {
                if (speed2DurFields[i] != null)
                    speed2DurFields[i].text = Speed2Dur[i].ToString();
            }
        }

        // PointCount
        if (pointCountFields != null)
        {
            for (int i = 0; i < Mathf.Min(pointCountFields.Length, pointCount.Count); i++)
            {
                if (pointCountFields[i] != null)
                    pointCountFields[i].text = pointCount[i].ToString();
            }
        }

        // Distance Settings
        if (maxDistLowSpeedField != null)
            maxDistLowSpeedField.text = MaxDistLowSpeed.ToString();

        if (minDistLowSpeedField != null)
            minDistLowSpeedField.text = MinDistLowSpeed.ToString();

        if (maxDistHighSpeedField != null)
            maxDistHighSpeedField.text = MaxDistHighSpeed.ToString();

        if (minDistHighSpeedField != null)
            minDistHighSpeedField.text = MinDistHighSpeed.ToString();

        // Occlusion Settings
        if (pointsOcclField != null)
            pointsOcclField.text = PointsOccl.ToString();

        if (occlDurLowField != null)
            occlDurLowField.text = OcclDurLow.ToString();

        if (occlDurHighField != null)
            occlDurHighField.text = OcclDurHigh.ToString();
    }

    // Восстановление значений по умолчанию
    public void ResetToDefaults()
    {
        // Восстанавливаем значения из дефолтного контейнера
        Speed1Dur = (float[])defaultValues.Speed1Dur.Clone();
        Speed2Dur = (float[])defaultValues.Speed2Dur.Clone();
        MaxDistLowSpeed = defaultValues.MaxDistLowSpeed;
        MinDistLowSpeed = defaultValues.MinDistLowSpeed;
        MaxDistHighSpeed = defaultValues.MaxDistHighSpeed;
        MinDistHighSpeed = defaultValues.MinDistHighSpeed;
        PointsOccl = defaultValues.PointsOccl;
        OcclDurLow = defaultValues.OcclDurLow;
        OcclDurHigh = defaultValues.OcclDurHigh;
        pointCount = new List<int>(defaultValues.pointCount);

        // Обновляем UI
        UpdateUIFromData();

        Debug.Log("Значения восстановлены по умолчанию");
    }

    // Обновление пути из UI
    public void OnPathInputChanged(string newPath)
    {
        if (!string.IsNullOrEmpty(newPath))
        {
            currentJsonPath = newPath;
        }
    }

    // Сохранение изменений в JSON файл
    public void SaveChanges()
    {
        // Обновляем путь из UI
        if (jsonPathInput != null && !string.IsNullOrEmpty(jsonPathInput.text))
        {
            currentJsonPath = jsonPathInput.text;
        }

        // Проверяем расширение файла
        if (!currentJsonPath.EndsWith(".json"))
        {
            currentJsonPath += ".json";
        }

        try
        {
            string json = ToJson();
            File.WriteAllText(currentJsonPath, json);
            Debug.Log($"Конфигурация сохранена в: {currentJsonPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка при сохранении файла: {e.Message}");
        }
    }

    // Метод для получения данных в формате JSON
    public string ToJson()
    {
        // Создаем округленную копию данных для записи в JSON
        ContainerData data = CreateRoundedDataForJson();

        return JsonUtility.ToJson(data, true);
    }

    // Метод для загрузки данных из JSON
    public void LoadFromJson(string path)
    {
        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                ContainerData data = JsonUtility.FromJson<ContainerData>(json);

                if (data != null)
                {
                    this.Speed1Dur = data.Speed1Dur ?? defaultValues.Speed1Dur;
                    this.Speed2Dur = data.Speed2Dur ?? defaultValues.Speed2Dur;
                    this.MaxDistLowSpeed = data.MaxDistLowSpeed;
                    this.MinDistLowSpeed = data.MinDistLowSpeed;
                    this.MaxDistHighSpeed = data.MaxDistHighSpeed;
                    this.MinDistHighSpeed = data.MinDistHighSpeed;
                    this.PointsOccl = data.PointsOccl;
                    this.OcclDurLow = data.OcclDurLow;
                    this.OcclDurHigh = data.OcclDurHigh;
                    this.pointCount = data.pointCount ?? new List<int>(defaultValues.pointCount);

                    Debug.Log($"Конфигурация загружена из: {path}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Ошибка при загрузке файла: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"Файл не найден: {path}. Используются значения по умолчанию.");
            ResetToDefaults();
        }
    }

    // Метод для быстрой загрузки из текущего пути
    public void LoadFromCurrentPath()
    {
        if (!string.IsNullOrEmpty(currentJsonPath))
        {
            LoadFromJson(currentJsonPath);
        }
    }
}