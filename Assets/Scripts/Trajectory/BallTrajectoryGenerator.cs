using UnityEngine;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class BallTrajectoryGenerator : MonoBehaviour
{
    [System.Serializable]
    public class TrajectorySettings
    {
        public string trajectoryName;
        public Vector3[] points;
        public float totalLength;
        public string speed;
        public string duration;
        public float distanceBetweenPoints;

        public TrajectorySettings(string name, Vector3[] trajectoryPoints, float length,
            string speedType, string durationType, float pointDistance)
        {
            trajectoryName = name;
            points = trajectoryPoints;
            totalLength = length;
            speed = speedType;
            duration = durationType;
            distanceBetweenPoints = pointDistance;
        }
    }

    [Header("Настройки скорости шарика")]
    [Tooltip("Словарь скоростей шарика: '1' = 0.04f, '2' = 0.08f")]
    public Dictionary<string, float> ballSpeeds = new Dictionary<string, float>()
    {
        { "1", 0.04f },
        { "2", 0.08f }
    };

    [Tooltip("Выберите скорость шарика: 1 или 2")]
    public string selectedSpeed = "1";

    [Header("Настройки длительности траекторий")]
    [Tooltip("Словарь длительностей для каждой скорости")]
    public Dictionary<string, Dictionary<string, float>> trajectoryDurationsBySpeed =
        new Dictionary<string, Dictionary<string, float>>();

    [Tooltip("Длительность траектории: 1, 2 или 3")]
    public string trajectoryDuration = "1";

    [Header("Параметры траектории")]
    [Tooltip("Общая длина траектории = значение варианта + (occluder ? 1.0 : 0)")]
    public float totalLength = 0f;

    [Tooltip("Добавляет 1.0f к общей длине если true")]
    public bool occluder = false;

    [Tooltip("Вариант от 0 до 5")]
    [Range(0, 5)]
    public int variant = 0;

    [Tooltip("Условие: 1, 2 или 3")]
    [Range(1, 3)]
    public int condition = 1;

    [Tooltip("Ось: 1, 2 или 3")]
    [Range(1, 3)]
    public int axis = 1;

    [Header("Настройки количества точек")]
    [Tooltip("Количество точек для каждой длительности")]
    public Dictionary<string, int> pointCountByDuration = new Dictionary<string, int>()
    {
        { "1", 5 },   // Короткая: 5 точек
        { "2", 7 },   // Средняя: 7 точек
        { "3", 13 }   // Длинная: 13 точек
    };

    [Tooltip("Дополнительные точки при occluder = true")]
    public int additionalPointsForOccluder = 2;

    [Header("Настройки генерации точек")]
    [Tooltip("Минимальное расстояние между точками для скорости '1'")]
    public float minDistanceSpeed1 = 0.35f;

    [Tooltip("Максимальное расстояние между точками для скорости '1'")]
    public float maxDistanceSpeed1 = 0.65f;

    [Tooltip("Минимальное расстояние между точками для скорости '2'")]
    public float minDistanceSpeed2 = 0.7f;

    [Tooltip("Максимальное расстояние между точками для скорости '2'")]
    public float maxDistanceSpeed2 = 1.3f;

    [Header("Результаты")]
    [Tooltip("Название сгенерированной траектории")]
    public string generatedTrajectoryName = "";

    [Tooltip("Массив точек траектории")]
    public Vector3[] trajectoryPoints;

    [Tooltip("Расстояние между точками")]
    public float pointDistance = 0f;

    [Tooltip("Базовая длина (без occluder)")]
    public float baseTrajectoryLength = 0f;

    [Tooltip("Количество точек в траектории")]
    public int pointCount = 0;

    [Header("Словарь траекторий")]
    [Tooltip("Словарь всех сгенерированных траекторий")]
    public Dictionary<string, Vector3[]> trajectoriesDictionary = new Dictionary<string, Vector3[]>();

    [Tooltip("Список для отображения в инспекторе")]
    public List<TrajectorySettings> trajectoryList = new List<TrajectorySettings>();

    [Header("Визуализация")]
    public bool showTrajectory = true;
    public Color trajectoryColor = Color.green;
    public float pointSize = 0.1f;

    void Awake()
    {
        InitializeData();
    }

    /// <summary>
    /// Инициализация данных
    /// </summary>
    void InitializeData()
    {
        // Инициализация словаря скоростей если пустой
        if (ballSpeeds.Count == 0)
        {
            ballSpeeds = new Dictionary<string, float>()
            {
                { "1", 0.04f },
                { "2", 0.08f }
            };
        }

        // Инициализация словаря длительностей с точными значениями
        InitializeTrajectoryDurations();

        // Инициализация количества точек
        if (pointCountByDuration.Count == 0)
        {
            pointCountByDuration = new Dictionary<string, int>()
            {
                { "1", 5 },   // Короткая: 5 точек
                { "2", 7 },   // Средняя: 7 точек
                { "3", 13 }   // Длинная: 13 точек
            };
        }

        // Генерируем начальную траекторию
        GenerateTrajectory();
    }

    /// <summary>
    /// Инициализация словаря длительностей траекторий
    /// </summary>
    void InitializeTrajectoryDurations()
    {
        // Для скорости "1" (медленно = 0.04f)
        Dictionary<string, float> speed1Durations = new Dictionary<string, float>()
        {
            { "1", 1.0f },    // Короткая = 1.0f
            { "2", 3.2f },    // Средняя = 3.2f
            { "3", 5.8f }     // Длинная = 5.8f
        };

        // Для скорости "2" (быстро = 0.08f)
        Dictionary<string, float> speed2Durations = new Dictionary<string, float>()
        {
            { "1", 2.0f },    // Короткая = 2.0f
            { "2", 6.4f },    // Средняя = 6.4f
            { "3", 11.6f }    // Длинная = 11.6f
        };

        trajectoryDurationsBySpeed = new Dictionary<string, Dictionary<string, float>>()
        {
            { "1", speed1Durations },
            { "2", speed2Durations }
        };
    }

    /// <summary>
    /// Генерация названия траектории
    /// </summary>
    string GenerateTrajectoryName()
    {
        // Формат: A + Q + C + W + S + E + D + R + V + T
        return $"A{axis}C{condition}S{selectedSpeed}D{trajectoryDuration}V{variant}";
    }

    /// <summary>
    /// Получить целевое расстояние между точками
    /// </summary>
    float GetTargetDistance()
    {
        if (selectedSpeed == "1")
        {
            return Random.Range(minDistanceSpeed1, maxDistanceSpeed1);
        }
        else // selectedSpeed == "2"
        {
            return Random.Range(minDistanceSpeed2, maxDistanceSpeed2);
        }
    }

    /// <summary>
    /// Получить базовую длину траектории (без occluder)
    /// </summary>
    public float GetBaseTrajectoryLength()
    {
        if (trajectoryDurationsBySpeed.ContainsKey(selectedSpeed) &&
            trajectoryDurationsBySpeed[selectedSpeed].ContainsKey(trajectoryDuration))
        {
            return trajectoryDurationsBySpeed[selectedSpeed][trajectoryDuration];
        }

        // Возвращаем значения по умолчанию если что-то пошло не так
        return selectedSpeed == "1" ? 1.0f : 2.0f;
    }

    /// <summary>
    /// Получить итоговую длину траектории (с учетом occluder)
    /// </summary>
    public float GetTotalTrajectoryLength()
    {
        float baseLength = GetBaseTrajectoryLength();
        return baseLength + (occluder ? 1.0f : 0f);
    }

    /// <summary>
    /// Получить количество точек для текущей длительности
    /// </summary>
    int GetPointCountForDuration()
    {
        if (pointCountByDuration.ContainsKey(trajectoryDuration))
        {
            int basePointCount = pointCountByDuration[trajectoryDuration];

            // Добавляем дополнительные точки если occluder = true
            return basePointCount + (occluder ? additionalPointsForOccluder : 0);
        }

        // Возвращаем значения по умолчанию
        return trajectoryDuration switch
        {
            "1" => 5 + (occluder ? 2 : 0),
            "2" => 7 + (occluder ? 2 : 0),
            "3" => 13 + (occluder ? 2 : 0),
            _ => 5
        };
    }

    /// <summary>
    /// Сгенерировать траекторию на основе текущих параметров
    /// </summary>
    [ContextMenu("Сгенерировать траекторию")]
    public void GenerateTrajectory()
    {
        // 1. Получаем базовую длину (значение варианта траектории)
        baseTrajectoryLength = GetBaseTrajectoryLength();

        // 2. Получаем итоговую длину (базовая + occluder * 1.0f)
        totalLength = GetTotalTrajectoryLength();

        // 3. Получаем количество точек
        pointCount = GetPointCountForDuration();

        // 4. Получаем целевое расстояние между точками
        pointDistance = GetTargetDistance();

        // 5. Генерируем точки
        trajectoryPoints = GenerateTrajectoryPoints(pointCount, totalLength);

        // 6. Генерируем название
        generatedTrajectoryName = GenerateTrajectoryName();

        // 7. Выводим информацию
        Debug.Log($"Сгенерирована траектория: {generatedTrajectoryName}");
        Debug.Log($"Базовая длина: {baseTrajectoryLength:F1}");
        Debug.Log($"Итоговая длина: {totalLength:F2} (базовая {(occluder ? "+ 1.0" : "")})");
        Debug.Log($"Точек: {pointCount} (базовых: {pointCountByDuration[trajectoryDuration]}{(occluder ? $" + {additionalPointsForOccluder}" : "")})");
        Debug.Log($"Расстояние между точками: {pointDistance:F2}");
        Debug.Log($"Скорость шарика: {selectedSpeed} = {ballSpeeds[selectedSpeed]:F3}");
    }

    /// <summary>
    /// Генерация массива точек траектории
    /// </summary>
    Vector3[] GenerateTrajectoryPoints(int pointCount, float totalLength)
    {
        Vector3[] points = new Vector3[pointCount];

        // Начинаем с начала координат
        points[0] = Vector3.zero;

        // Определяем направление на основе оси с учетом ограничений
        Vector3 direction = GetDirectionFromAxis();

        // Генерируем точки вдоль выбранного направления
        for (int i = 1; i < pointCount; i++)
        {
            // Равномерное распределение вдоль траектории
            float t = (float)i / (pointCount - 1);
            float distanceAlongPath = t * totalLength;

            // Базовая позиция вдоль направления
            Vector3 basePosition = direction * distanceAlongPath;

            // Применяем ограничения по осям
            basePosition = ApplyAxisRestrictions(basePosition);

            // Добавляем отклонения в зависимости от condition
            points[i] = ApplyConditionEffects(basePosition, t, totalLength);
        }

        return points;
    }

    /// <summary>
    /// Получить направление на основе выбранной оси
    /// </summary>
    Vector3 GetDirectionFromAxis()
    {
        switch (axis)
        {
            case 1: return Vector3.right;      // Ось X
            case 2: return Vector3.up;         // Ось Y
            case 3: return Vector3.forward;    // Ось Z
            default: return Vector3.right;
        }
    }

    /// <summary>
    /// Применить ограничения по осям
    /// </summary>
    Vector3 ApplyAxisRestrictions(Vector3 position)
    {
        Vector3 result = position;

        switch (axis)
        {
            case 1: // Axis = 1: Y и Z равны 0 (для оси X)
                result.y = 0f;
                result.z = 0f;
                break;

            case 2: // Axis = 2: Z равна 0 (для оси Y)
                result.z = 0f;
                break;

            case 3: // Axis = 3: нет ограничений (ось Z)
                break;
        }

        return result;
    }

    /// <summary>
    /// Применить эффекты в зависимости от condition
    /// </summary>
    Vector3 ApplyConditionEffects(Vector3 position, float t, float totalLength)
    {
        Vector3 result = position;

        switch (condition)
        {
            case 1: // Condition 1: небольшие случайные отклонения
                float noise1 = 0.05f * totalLength;
                result += new Vector3(
                    Random.Range(-noise1, noise1),
                    Random.Range(-noise1, noise1),
                    Random.Range(-noise1, noise1)
                );
                break;

            case 2: // Condition 2: синусоидальные отклонения
                float amplitude = 0.1f * totalLength;
                float frequency = 4f;
                result.x += Mathf.Sin(t * Mathf.PI * frequency) * amplitude;
                result.y += Mathf.Cos(t * Mathf.PI * frequency) * amplitude * 0.5f;
                break;

            case 3: // Condition 3: спиральные отклонения
                float spiralRadius = 0.15f * totalLength;
                float spiralTurns = 3f;
                float angle = t * Mathf.PI * 2f * spiralTurns;
                result.x += Mathf.Cos(angle) * spiralRadius;
                result.y += Mathf.Sin(angle) * spiralRadius;
                break;
        }

        // Снова применяем ограничения по осям после добавления эффектов
        return ApplyAxisRestrictions(result);
    }

    /// <summary>
    /// Сохранить текущую траекторию в словарь
    /// </summary>
    [ContextMenu("Сохранить траекторию в словарь")]
    public void SaveTrajectoryToDictionary()
    {
        if (trajectoryPoints == null || trajectoryPoints.Length == 0)
        {
            Debug.LogWarning("Нет траектории для сохранения! Сначала сгенерируйте траекторию.");
            return;
        }

        if (string.IsNullOrEmpty(generatedTrajectoryName))
        {
            Debug.LogWarning("Нет названия траектории!");
            return;
        }

        // Копируем массив точек
        Vector3[] pointsCopy = new Vector3[trajectoryPoints.Length];
        System.Array.Copy(trajectoryPoints, pointsCopy, trajectoryPoints.Length);

        // Добавляем или обновляем в словаре
        if (trajectoriesDictionary.ContainsKey(generatedTrajectoryName))
        {
            trajectoriesDictionary[generatedTrajectoryName] = pointsCopy;
            Debug.Log($"Обновлена траектория '{generatedTrajectoryName}' ({pointsCopy.Length} точек)");
        }
        else
        {
            trajectoriesDictionary.Add(generatedTrajectoryName, pointsCopy);
            Debug.Log($"Сохранена траектория '{generatedTrajectoryName}' ({pointsCopy.Length} точек)");
        }

        // Обновляем список для отображения в инспекторе
        UpdateTrajectoryList();
    }

    /// <summary>
    /// Обновить список траекторий для отображения в инспекторе (сортировка по алфавиту)
    /// </summary>
    public void UpdateTrajectoryList()
    {
        trajectoryList.Clear();

        // Создаем отсортированный список названий
        var sortedKeys = trajectoriesDictionary.Keys.OrderBy(key => key).ToList();

        foreach (var key in sortedKeys)
        {
            Vector3[] points = trajectoriesDictionary[key];

            // Рассчитываем длину траектории
            float length = CalculateTrajectoryLength(points);

            // Извлекаем параметры из названия
            ExtractParametersFromName(key, out string speed, out string duration);

            trajectoryList.Add(new TrajectorySettings(
                key,
                points,
                length,
                speed,
                duration,
                pointDistance
            ));
        }

        Debug.Log($"Список траекторий обновлен и отсортирован по алфавиту. Всего: {trajectoryList.Count}");
    }

    /// <summary>
    /// Рассчитать длину траектории
    /// </summary>
    float CalculateTrajectoryLength(Vector3[] points)
    {
        if (points.Length < 2) return 0f;

        float length = 0f;
        for (int i = 0; i < points.Length - 1; i++)
        {
            length += Vector3.Distance(points[i], points[i + 1]);
        }

        return length;
    }

    /// <summary>
    /// Извлечь параметры из названия траектории
    /// </summary>
    void ExtractParametersFromName(string trajectoryName, out string speed, out string duration)
    {
        speed = "1";
        duration = "1";

        try
        {
            // Извлекаем скорость (символ после 'S')
            int sIndex = trajectoryName.IndexOf('S');
            if (sIndex >= 0 && sIndex + 1 < trajectoryName.Length)
            {
                char speedChar = trajectoryName[sIndex + 1];
                if (char.IsDigit(speedChar))
                    speed = speedChar.ToString();
            }

            // Извлекаем длительность (символ после 'D')
            int dIndex = trajectoryName.IndexOf('D');
            if (dIndex >= 0 && dIndex + 1 < trajectoryName.Length)
            {
                char durationChar = trajectoryName[dIndex + 1];
                if (char.IsDigit(durationChar))
                    duration = durationChar.ToString();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Ошибка при извлечении параметров: {e.Message}");
        }
    }

    /// <summary>
    /// Загрузить траекторию из словаря по имени
    /// </summary>
    public void LoadTrajectory(string trajectoryName)
    {
        if (trajectoriesDictionary.ContainsKey(trajectoryName))
        {
            trajectoryPoints = trajectoriesDictionary[trajectoryName];
            generatedTrajectoryName = trajectoryName;

            // Извлекаем параметры из названия
            ExtractAllParametersFromName(trajectoryName);

            // Обновляем длину на основе извлеченных параметров
            UpdateLengthFromParameters();

            // Обновляем количество точек
            pointCount = trajectoryPoints.Length;

            Debug.Log($"Загружена траектория: {trajectoryName}");
        }
        else
        {
            Debug.LogWarning($"Траектория '{trajectoryName}' не найдена!");
        }
    }

    /// <summary>
    /// Обновить длину на основе текущих параметров
    /// </summary>
    void UpdateLengthFromParameters()
    {
        baseTrajectoryLength = GetBaseTrajectoryLength();
        totalLength = GetTotalTrajectoryLength();
    }

    /// <summary>
    /// Извлечь все параметры из названия траектории
    /// </summary>
    void ExtractAllParametersFromName(string trajectoryName)
    {
        try
        {
            // Извлекаем axis (символ после 'A')
            int aIndex = trajectoryName.IndexOf('A');
            if (aIndex >= 0 && aIndex + 1 < trajectoryName.Length)
            {
                char axisChar = trajectoryName[aIndex + 1];
                if (char.IsDigit(axisChar))
                    axis = int.Parse(axisChar.ToString());
            }

            // Извлекаем condition (символ после 'C')
            int cIndex = trajectoryName.IndexOf('C');
            if (cIndex >= 0 && cIndex + 1 < trajectoryName.Length)
            {
                char conditionChar = trajectoryName[cIndex + 1];
                if (char.IsDigit(conditionChar))
                    condition = int.Parse(conditionChar.ToString());
            }

            // Извлекаем скорость (символ после 'S')
            int sIndex = trajectoryName.IndexOf('S');
            if (sIndex >= 0 && sIndex + 1 < trajectoryName.Length)
            {
                char speedChar = trajectoryName[sIndex + 1];
                if (char.IsDigit(speedChar))
                    selectedSpeed = speedChar.ToString();
            }

            // Извлекаем длительность (символ после 'D')
            int dIndex = trajectoryName.IndexOf('D');
            if (dIndex >= 0 && dIndex + 1 < trajectoryName.Length)
            {
                char durationChar = trajectoryName[dIndex + 1];
                if (char.IsDigit(durationChar))
                    trajectoryDuration = durationChar.ToString();
            }

            // Извлекаем variant (символ после 'V')
            int vIndex = trajectoryName.IndexOf('V');
            if (vIndex >= 0 && vIndex + 1 < trajectoryName.Length)
            {
                char variantChar = trajectoryName[vIndex + 1];
                if (char.IsDigit(variantChar))
                    variant = int.Parse(variantChar.ToString());
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Ошибка при извлечении параметров: {e.Message}");
        }
    }

    /// <summary>
    /// Очистить словарь траекторий
    /// </summary>
    [ContextMenu("Очистить словарь траекторий")]
    public void ClearTrajectoriesDictionary()
    {
        trajectoriesDictionary.Clear();
        trajectoryList.Clear();
        Debug.Log("Словарь траекторий очищен");
    }

    /// <summary>
    /// Получить список всех сохраненных имен траекторий (отсортированный)
    /// </summary>
    public List<string> GetTrajectoryNames()
    {
        return trajectoriesDictionary.Keys.OrderBy(key => key).ToList();
    }

    /// <summary>
    /// Получить скорость шарика по выбранному ключу
    /// </summary>
    public float GetBallSpeed()
    {
        if (ballSpeeds.ContainsKey(selectedSpeed))
            return ballSpeeds[selectedSpeed];

        return 0.04f; // Значение по умолчанию
    }

    /// <summary>
    /// Получить итоговую длину траектории по параметрам
    /// </summary>
    public float CalculateTotalLength(string speed, string duration, bool hasOccluder)
    {
        float baseLength = trajectoryDurationsBySpeed.ContainsKey(speed) &&
                          trajectoryDurationsBySpeed[speed].ContainsKey(duration)
            ? trajectoryDurationsBySpeed[speed][duration]
            : (speed == "1" ? 1.0f : 2.0f);

        return baseLength + (hasOccluder ? 1.0f : 0f);
    }

    /// <summary>
    /// Сгенерировать несколько траекторий для тестирования (отсортированные по алфавиту)
    /// </summary>
    [ContextMenu("Сгенерировать тестовые траектории")]
    public void GenerateTestTrajectories()
    {
        ClearTrajectoriesDictionary();

        // Генерируем различные комбинации
        string[] speeds = { "1", "2" };
        string[] durations = { "1", "2", "3" };
        int[] conditions = { 1, 2, 3 };
        int[] axes = { 1, 2, 3 };

        // Создаем список для временного хранения перед сортировкой
        List<(string name, Vector3[] points)> tempTrajectories = new List<(string, Vector3[])>();

        foreach (var speed in speeds)
        {
            foreach (var duration in durations)
            {
                foreach (var cond in conditions)
                {
                    foreach (var ax in axes)
                    {
                        for (int var = 0; var <= 2; var++) // 3 варианта для теста
                        {
                            // Устанавливаем параметры
                            selectedSpeed = speed;
                            trajectoryDuration = duration;
                            condition = cond;
                            axis = ax;
                            variant = var;
                            occluder = Random.value > 0.5f; // случайный occluder

                            // Генерируем
                            GenerateTrajectory();

                            // Сохраняем во временный список
                            Vector3[] pointsCopy = new Vector3[trajectoryPoints.Length];
                            System.Array.Copy(trajectoryPoints, pointsCopy, trajectoryPoints.Length);
                            tempTrajectories.Add((generatedTrajectoryName, pointsCopy));
                        }
                    }
                }
            }
        }

        // Сортируем по алфавиту и добавляем в словарь
        var sortedTrajectories = tempTrajectories.OrderBy(t => t.name).ToList();

        foreach (var trajectory in sortedTrajectories)
        {
            trajectoriesDictionary.Add(trajectory.name, trajectory.points);
        }

        // Обновляем список для отображения
        UpdateTrajectoryList();

        Debug.Log($"Сгенерировано {trajectoriesDictionary.Count} тестовых траекторий (отсортировано по алфавиту)");
    }

    /// <summary>
    /// Получить базовое количество точек (без occluder)
    /// </summary>
    public int GetBasePointCount()
    {
        if (pointCountByDuration.ContainsKey(trajectoryDuration))
        {
            return pointCountByDuration[trajectoryDuration];
        }

        return trajectoryDuration switch
        {
            "1" => 5,
            "2" => 7,
            "3" => 13,
            _ => 5
        };
    }

    void OnDrawGizmosSelected()
    {
        if (!showTrajectory || trajectoryPoints == null || trajectoryPoints.Length < 2)
            return;

        Gizmos.color = trajectoryColor;

        // Рисуем точки
        for (int i = 0; i < trajectoryPoints.Length; i++)
        {
            Gizmos.DrawSphere(trajectoryPoints[i], pointSize);
        }

        // Рисуем линии между точками
        for (int i = 0; i < trajectoryPoints.Length - 1; i++)
        {
            Gizmos.DrawLine(trajectoryPoints[i], trajectoryPoints[i + 1]);
        }

        // Подпись с информацией
#if UNITY_EDITOR
        if (trajectoryPoints.Length > 0)
        {
            Vector3 labelPos = trajectoryPoints[0] + Vector3.up * 0.5f;

            string axisInfo = axis switch
            {
                1 => "Ось X (Y=0, Z=0)",
                2 => "Ось Y (Z=0)",
                3 => "Ось Z",
                _ => "Неизвестно"
            };

            float ballSpeedValue = GetBallSpeed();
            int basePointCount = GetBasePointCount();

            Handles.Label(labelPos,
                $"Траектория: {generatedTrajectoryName}\n" +
                $"Точек: {pointCount} (базовых: {basePointCount}{(occluder ? $" + {additionalPointsForOccluder}" : "")})\n" +
                $"Базовая длина: {baseTrajectoryLength:F1}\n" +
                $"Итоговая длина: {totalLength:F2} {(occluder ? "(+1.0)" : "")}\n" +
                $"Скорость: {selectedSpeed} = {ballSpeedValue:F3}\n" +
                $"Длительность: {trajectoryDuration}\n" +
                $"Condition: {condition}\n" +
                $"Axis: {axis} ({axisInfo})\n" +
                $"Occluder: {occluder}\n" +
                $"Расстояние между точками: {pointDistance:F2}");
        }
#endif
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(BallTrajectoryGenerator))]
public class BallTrajectoryGeneratorEditor : Editor
{
    private Vector2 scrollPos;
    private bool showTrajectories = false;
    private bool showSettings = true;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BallTrajectoryGenerator script = (BallTrajectoryGenerator)target;

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Управление генерацией", EditorStyles.boldLabel);

        // Проверка допустимости скорости
        if (script.selectedSpeed != "1" && script.selectedSpeed != "2")
        {
            EditorGUILayout.HelpBox("Скорость должна быть '1' или '2'", MessageType.Error);
        }

        // Информация о текущих значениях
        EditorGUILayout.LabelField("Текущие значения:", EditorStyles.miniBoldLabel);

        EditorGUI.indentLevel++;

        float speedValue = script.GetBallSpeed();
        string speedName = script.selectedSpeed == "1" ? "Медленно (0.04)" : "Быстро (0.08)";
        EditorGUILayout.LabelField($"Скорость: {script.selectedSpeed} = {speedName}", EditorStyles.miniLabel);

        string durationName = script.trajectoryDuration switch
        {
            "1" => "Короткая",
            "2" => "Средняя",
            "3" => "Длинная",
            _ => "Неизвестно"
        };

        float baseLength = script.GetBaseTrajectoryLength();
        int basePoints = script.GetBasePointCount();
        EditorGUILayout.LabelField($"Длительность: {durationName} = {baseLength:F1} ({basePoints} точек)",
            EditorStyles.miniLabel);

        string axisInfo = script.axis switch
        {
            1 => "Ось X (Y=0, Z=0)",
            2 => "Ось Y (Z=0)",
            3 => "Ось Z",
            _ => "Неизвестно"
        };
        EditorGUILayout.LabelField($"Axis: {axisInfo}", EditorStyles.miniLabel);

        EditorGUILayout.LabelField($"Condition: {script.condition}", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"Variant: {script.variant}", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"Occluder: {script.occluder}", EditorStyles.miniLabel);

        EditorGUILayout.LabelField($"Базовая длина: {baseLength:F1}", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"Итоговая длина: {script.totalLength:F2} {(script.occluder ? "(+1.0)" : "")}",
            EditorStyles.miniLabel);

        int totalPoints = script.pointCount;
        EditorGUILayout.LabelField($"Точек: {totalPoints} (базовых: {basePoints}{(script.occluder ? $" + {script.additionalPointsForOccluder}" : "")})",
            EditorStyles.miniLabel);

        EditorGUI.indentLevel--;

        GUILayout.Space(10);

        // Основные кнопки
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Сгенерировать траекторию", GUILayout.Height(30)))
        {
            script.GenerateTrajectory();
            EditorUtility.SetDirty(script);
            SceneView.RepaintAll();
        }

        if (GUILayout.Button("Сохранить в словарь", GUILayout.Height(30)))
        {
            script.SaveTrajectoryToDictionary();
            EditorUtility.SetDirty(script);
            SceneView.RepaintAll();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Тестовые траектории"))
        {
            script.GenerateTestTrajectories();
            EditorUtility.SetDirty(script);
            SceneView.RepaintAll();
        }

        if (GUILayout.Button("Очистить словарь"))
        {
            script.ClearTrajectoriesDictionary();
            EditorUtility.SetDirty(script);
        }
        EditorGUILayout.EndHorizontal();

        // Кнопка обновления списка
        if (GUILayout.Button("Обновить список траекторий"))
        {
            script.UpdateTrajectoryList();
            EditorUtility.SetDirty(script);
        }

        // Список сохраненных траекторий
        GUILayout.Space(10);
        showTrajectories = EditorGUILayout.Foldout(showTrajectories, "Сохраненные траектории", true);

        if (showTrajectories && script.trajectoryList.Count > 0)
        {
            EditorGUILayout.LabelField($"Всего траекторий: {script.trajectoryList.Count} (отсортировано по алфавиту)",
                EditorStyles.miniLabel);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(200));

            foreach (var trajectory in script.trajectoryList)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button(trajectory.trajectoryName, EditorStyles.miniButton, GUILayout.Width(200)))
                {
                    script.LoadTrajectory(trajectory.trajectoryName);
                    EditorUtility.SetDirty(script);
                    SceneView.RepaintAll();
                }

                EditorGUILayout.LabelField($"Точек: {trajectory.points.Length}", GUILayout.Width(80));
                EditorGUILayout.LabelField($"Длина: {trajectory.totalLength:F2}", GUILayout.Width(80));

                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    if (script.trajectoriesDictionary.ContainsKey(trajectory.trajectoryName))
                    {
                        script.trajectoriesDictionary.Remove(trajectory.trajectoryName);
                        script.UpdateTrajectoryList();
                        EditorUtility.SetDirty(script);
                    }
                    break;
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField($"Скорость: {trajectory.speed}, Длительность: {trajectory.duration}",
                    EditorStyles.miniLabel);

                EditorGUILayout.EndVertical();
#endif
            }
        }
    }
}