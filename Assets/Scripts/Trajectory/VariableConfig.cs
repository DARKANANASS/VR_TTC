using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class VariableConfig : MonoBehaviour
{
    [Header("Speed Arrays (из JSON)")]
    public float[] Speed1Dur = new float[3];
    public float[] Speed2Dur = new float[3];

    [Header("Distance Settings (из JSON)")]
    public float MaxDistLowSpeed;
    public float MinDistLowSpeed;
    public float MaxDistHighSpeed;
    public float MinDistHighSpeed;

    [Header("Occlusion Settings (из JSON)")]
    public int PointsOccl;
    public float OcclDurLow;
    public float OcclDurHigh;

    [Header("Configuration Lists (из JSON)")]
    public List<int> pointCount = new List<int>();

    [Header("Локальные переменные")]
    [Tooltip("Типы скоростей")]
    public string[] speed = new string[] { "Низкая", "Высокая" };

    [Tooltip("Список условий: TTC, Control, Reverse")]
    public List<string> condition = new List<string> { "TTC", "Control", "Reverse" };

    [Tooltip("Список осей: 1, 2, 3")]
    public List<int> axis = new List<int> { 1, 2, 3 };

    [Tooltip("Список вариантов: 1, 2, 3, 4, 5")]
    public List<int> Variant = new List<int> { 1, 2, 3, 4, 5 };

    [Header("Текущие значения")]
    [Tooltip("Текущая скорость")]
    public string cur_speed;

    [Tooltip("Текущее условие")]
    public string cur_condition;

    [Tooltip("Текущая ось")]
    public int cur_axis;

    [Tooltip("Текущий вариант")]
    public int cur_Variant;

    [Tooltip("Номер скорости (1 - Низкая, 2 - Высокая)")]
    public int num_speed;

    [Tooltip("Номер условия (1 - TTC, 2 - Control, 3 - Reverse)")]
    public int num_condition;

    [Tooltip("Номер длительности (порядковый номер в выпадающем списке + 1)")]
    public int num_duration;

    [Tooltip("Текущая длительность")]
    public float cur_duration;

    [Tooltip("Текущее количество точек")]
    public int cur_points;

    [Tooltip("Сгенерированное имя")]
    public string cur_Name;

    [Tooltip("Флаг окклюзии")]
    public bool occlusion = false;

    [Header("Расчетные значения")]
    [Tooltip("Общая длина (длительность + окклюзия)")]
    public float AllLength;

    [Tooltip("Общее количество точек")]
    public int AllPoint;

    [Tooltip("Текущее максимальное расстояние")]
    public float cur_MaxDist;

    [Tooltip("Текущее минимальное расстояние")]
    public float cur_MinDist;

    [Header("UI References")]
    [Tooltip("Кнопка для загрузки конфигурации")]
    public Button loadConfigButton;

    [Tooltip("Поле ввода пути к JSON файлу")]
    public TMP_InputField jsonPathInput;

    [Tooltip("Выпадающий список для выбора скорости")]
    public TMP_Dropdown speedDropdown;

    [Tooltip("Выпадающий список для выбора условия")]
    public TMP_Dropdown conditionDropdown;

    [Tooltip("Выпадающий список для выбора оси")]
    public TMP_Dropdown axisDropdown;

    [Tooltip("Выпадающий список для выбора варианта")]
    public TMP_Dropdown variantDropdown;

    [Tooltip("Выпадающий список для выбора длительности")]
    public TMP_Dropdown durationDropdown;

    [Tooltip("Переключатель окклюзии")]
    public Toggle occlusionToggle;

    [Tooltip("Поле ввода общей длины")]
    public TMP_InputField allLengthInput;

    [Tooltip("Поле ввода общего количества точек")]
    public TMP_InputField allPointInput;

    [Tooltip("Поле ввода текущего максимального расстояния")]
    public TMP_InputField curMaxDistInput;

    [Tooltip("Поле ввода текущего минимального расстояния")]
    public TMP_InputField curMinDistInput;

    [Tooltip("Поле ввода сгенерированного имени")]
    public TMP_InputField curNameInput;

    // Класс для десериализации JSON
    [System.Serializable]
    private class ContainerData
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

    public void Start()
    {
        // Считывание данных из JSON при старте
        string defaultJsonPath = "config.json";
        if (System.IO.File.Exists(defaultJsonPath))
        {
            AssignVariablesFromJson(defaultJsonPath);
        }

        // Назначаем обработчик нажатия кнопки
        if (loadConfigButton != null)
        {
            loadConfigButton.onClick.AddListener(OnLoadConfigButtonClicked);
        }

        // Назначаем обработчики для выпадающих списков
        if (speedDropdown != null)
        {
            speedDropdown.onValueChanged.AddListener(OnSpeedDropdownChanged);
            InitializeDropdown(speedDropdown, speed);
        }

        if (conditionDropdown != null)
        {
            conditionDropdown.onValueChanged.AddListener(OnConditionDropdownChanged);
            InitializeDropdown(conditionDropdown, condition);
        }

        if (axisDropdown != null)
        {
            axisDropdown.onValueChanged.AddListener(OnAxisDropdownChanged);
            InitializeDropdown(axisDropdown, axis.ConvertAll(x => x.ToString()));
        }

        if (variantDropdown != null)
        {
            variantDropdown.onValueChanged.AddListener(OnVariantDropdownChanged);
            InitializeDropdown(variantDropdown, Variant.ConvertAll(x => x.ToString()));
        }

        if (durationDropdown != null)
        {
            durationDropdown.onValueChanged.AddListener(OnDurationDropdownChanged);
        }

        if (occlusionToggle != null)
        {
            occlusionToggle.onValueChanged.AddListener(OnOcclusionToggleChanged);
        }

        // Инициализируем значения по умолчанию
        InitializeDefaultValues();
    }

    /// <summary>
    /// Инициализация выпадающего списка значениями
    /// </summary>
    private void InitializeDropdown(TMP_Dropdown dropdown, IList<string> options)
    {
        dropdown.ClearOptions();
        List<TMP_Dropdown.OptionData> optionData = new List<TMP_Dropdown.OptionData>();

        foreach (string option in options)
        {
            optionData.Add(new TMP_Dropdown.OptionData(option));
        }

        dropdown.AddOptions(optionData);
    }

    /// <summary>
    /// Инициализация значений по умолчанию
    /// </summary>
    private void InitializeDefaultValues()
    {
        // Устанавливаем начальные значения
        if (speedDropdown != null && speed.Length > 0)
        {
            cur_speed = speed[0];
            UpdateSpeedNumber();
        }

        if (conditionDropdown != null && condition.Count > 0)
        {
            cur_condition = condition[0];
            UpdateCondition();
        }

        if (axisDropdown != null && axis.Count > 0)
        {
            cur_axis = axis[0];
        }

        if (variantDropdown != null && Variant.Count > 0)
        {
            cur_Variant = Variant[0];
        }

        UpdateUIInputFields();
    }

    /// <summary>
    /// Обновление номера скорости на основе выбранной скорости
    /// </summary>
    private void UpdateSpeedNumber()
    {
        if (cur_speed == "Низкая")
        {
            num_speed = 1;
        }
        else if (cur_speed == "Высокая")
        {
            num_speed = 2;
        }

        UpdateDurationDropdown();
        UpdateDistanceAndLength();
        UpdateCurrentPoints();
        UpdateName();
    }

    /// <summary>
    /// Обновление условий
    /// </summary>
    private void UpdateCondition()
    {
        if (cur_condition == "TTC")
        {
            num_condition = 1;
            occlusion = true;
            AllPoint = cur_points + PointsOccl;
        }
        else if (cur_condition == "Control")
        {
            num_condition = 2;
            occlusion = false;
            AllPoint = cur_points;
        }
        else if (cur_condition == "Reverse")
        {
            num_condition = 3;
            occlusion = false;
            AllPoint = cur_points;
        }

        // Обновляем UI переключателя
        if (occlusionToggle != null)
        {
            occlusionToggle.isOn = occlusion;
        }

        UpdateDistanceAndLength();
        UpdateUIInputFields();
        UpdateName();
    }

    /// <summary>
    /// Обновление текущего количества точек на основе выбранной длительности
    /// </summary>
    private void UpdateCurrentPoints()
    {
        if (num_speed == 1 && Speed1Dur != null && pointCount != null && pointCount.Count >= 3)
        {
            for (int i = 0; i < Speed1Dur.Length; i++)
            {
                if (Mathf.Approximately(Speed1Dur[i], cur_duration))
                {
                    cur_points = pointCount[i];
                    break;
                }
            }
        }
        else if (num_speed == 2 && Speed2Dur != null && pointCount != null && pointCount.Count >= 3)
        {
            for (int i = 0; i < Speed2Dur.Length; i++)
            {
                if (Mathf.Approximately(Speed2Dur[i], cur_duration))
                {
                    cur_points = pointCount[i];
                    break;
                }
            }
        }

        UpdateCondition();
    }

    /// <summary>
    /// Обновление расстояний и общей длины
    /// </summary>
    private void UpdateDistanceAndLength()
    {
        if (num_speed == 1)
        {
            cur_MaxDist = MaxDistLowSpeed;
            cur_MinDist = MinDistLowSpeed;
        }
        else if (num_speed == 2)
        {
            cur_MaxDist = MaxDistHighSpeed;
            cur_MinDist = MinDistHighSpeed;
        }

        // Обновление общей длины
        if (cur_condition == "TTC")
        {
            if (num_speed == 1)
            {
                AllLength = cur_duration + OcclDurLow;
            }
            else if (num_speed == 2)
            {
                AllLength = cur_duration + OcclDurHigh;
            }
        }
        else
        {
            AllLength = cur_duration;
        }

        UpdateUIInputFields();
    }

    /// <summary>
    /// Обновление сгенерированного имени
    /// </summary>
    private void UpdateName()
    {
        cur_Name = "A" + cur_axis + "C" + num_condition + "S" + num_speed + "D" + num_duration + "V" + cur_Variant;

        if (curNameInput != null)
        {
            curNameInput.text = cur_Name;
        }
    }

    /// <summary>
    /// Обновление полей ввода UI
    /// </summary>
    private void UpdateUIInputFields()
    {
        if (allLengthInput != null)
            allLengthInput.text = AllLength.ToString("F2");

        if (allPointInput != null)
            allPointInput.text = AllPoint.ToString();

        if (curMaxDistInput != null)
            curMaxDistInput.text = cur_MaxDist.ToString("F2");

        if (curMinDistInput != null)
            curMinDistInput.text = cur_MinDist.ToString("F2");

        if (curNameInput != null)
            curNameInput.text = cur_Name;
    }

    /// <summary>
    /// Обновление выпадающего списка длительностей
    /// </summary>
    private void UpdateDurationDropdown()
    {
        if (durationDropdown == null) return;

        durationDropdown.ClearOptions();
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

        float[] durations = (num_speed == 1) ? Speed1Dur : Speed2Dur;

        if (durations != null)
        {
            foreach (float duration in durations)
            {
                options.Add(new TMP_Dropdown.OptionData(duration.ToString("F1")));
            }
        }

        durationDropdown.AddOptions(options);

        // Устанавливаем первую длительность по умолчанию
        if (options.Count > 0)
        {
            OnDurationDropdownChanged(0);
        }
    }

    // Обработчики изменений UI элементов
    private void OnSpeedDropdownChanged(int index)
    {
        if (index < speed.Length)
        {
            cur_speed = speed[index];
            UpdateSpeedNumber();
        }
    }

    private void OnConditionDropdownChanged(int index)
    {
        if (index < condition.Count)
        {
            cur_condition = condition[index];
            UpdateCondition();
        }
    }

    private void OnAxisDropdownChanged(int index)
    {
        if (index < axis.Count)
        {
            cur_axis = axis[index];
            UpdateName();
        }
    }

    private void OnVariantDropdownChanged(int index)
    {
        if (index < Variant.Count)
        {
            cur_Variant = Variant[index];
            UpdateName();
        }
    }

    private void OnDurationDropdownChanged(int index)
    {
        if (durationDropdown != null && durationDropdown.options.Count > index)
        {
            string durationText = durationDropdown.options[index].text;
            if (float.TryParse(durationText, out float duration))
            {
                cur_duration = duration;
                num_duration = index + 1; // Порядковый номер + 1
                UpdateDistanceAndLength();
                UpdateCurrentPoints();
                UpdateName();
            }
        }
    }

    private void OnOcclusionToggleChanged(bool value)
    {
        occlusion = value;
    }

    /// <summary>
    /// Обработчик нажатия кнопки загрузки конфигурации
    /// </summary>
    private void OnLoadConfigButtonClicked()
    {
        string jsonPath = "config.json"; // Путь по умолчанию

        // Если есть поле ввода, берем путь из него
        if (jsonPathInput != null && !string.IsNullOrEmpty(jsonPathInput.text))
        {
            jsonPath = jsonPathInput.text;
        }

        // Вызываем функцию присваивания переменных
        AssignVariablesFromJson(jsonPath);
    }

    /// <summary>
    /// Присваивает переменным значения из JSON файла
    /// </summary>
    /// <param name="jsonPath">Путь к JSON файлу</param>
    public void AssignVariablesFromJson(string jsonPath)
    {
        if (!System.IO.File.Exists(jsonPath))
        {
            Debug.LogError($"JSON файл не найден: {jsonPath}");
            return;
        }

        string jsonContent = System.IO.File.ReadAllText(jsonPath);
        ContainerData data = JsonUtility.FromJson<ContainerData>(jsonContent);

        if (data != null)
        {
            // Присваивание значений из JSON переменным скрипта
            Speed1Dur = data.Speed1Dur;
            Speed2Dur = data.Speed2Dur;
            MaxDistLowSpeed = data.MaxDistLowSpeed;
            MinDistLowSpeed = data.MinDistLowSpeed;
            MaxDistHighSpeed = data.MaxDistHighSpeed;
            MinDistHighSpeed = data.MinDistHighSpeed;
            PointsOccl = data.PointsOccl;
            OcclDurLow = data.OcclDurLow;
            OcclDurHigh = data.OcclDurHigh;
            pointCount = data.pointCount;

            // Обновляем выпадающий список длительностей после загрузки данных
            UpdateDurationDropdown();

            // Обновляем расчетные значения
            UpdateCondition();
            UpdateDistanceAndLength();
            UpdateCurrentPoints();
            UpdateName();

            Debug.Log("Конфигурация успешно загружена из JSON файла");
        }
        else
        {
            Debug.LogError("Не удалось десериализовать JSON файл");
        }
    }
}