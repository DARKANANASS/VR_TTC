using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class BallTrajectoryGeneratorV4 : MonoBehaviour
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
        public bool hasOccluder;

        public TrajectorySettings(string name, Vector3[] trajectoryPoints, float length,
            string speedType, string durationType, float pointDistance, bool occluder)
        {
            trajectoryName = name;
            points = trajectoryPoints;
            totalLength = length;
            speed = speedType;
            duration = durationType;
            distanceBetweenPoints = pointDistance;
            hasOccluder = occluder;
        }
    }

    [System.Serializable]
    public class TrajectoryCollection
    {
        public List<TrajectoryData> trajectories = new List<TrajectoryData>();
    }

    [System.Serializable]
    public class TrajectoryData
    {
        public string name;
        public Vector3Serializable[] points;
        public float totalLength;
        public string speed;
        public string duration;
        public bool hasOccluder;
    }

    [System.Serializable]
    public struct Vector3Serializable
    {
        public float x, y, z;

        public Vector3Serializable(Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public Vector3 ToVector3() => new Vector3(x, y, z);

        public static Vector3Serializable[] FromVector3Array(Vector3[] array)
        {
            if (array == null) return null;

            Vector3Serializable[] result = new Vector3Serializable[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                result[i] = new Vector3Serializable(array[i]);
            }
            return result;
        }

        public static Vector3[] ToVector3Array(Vector3Serializable[] array)
        {
            if (array == null) return null;

            Vector3[] result = new Vector3[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                result[i] = array[i].ToVector3();
            }
            return result;
        }
    }

    [Header("Настройки скорости шарика")]
    [Tooltip("Словарь скоростей шарика: '1' = 0.04f (медленно), '2' = 0.08f (быстро)")]
    public Dictionary<string, float> ballSpeeds = new Dictionary<string, float>()
    {
        { "1", 0.04f },
        { "2", 0.08f }
    };

    [Tooltip("Выберите скорость шарика: 1 (медленно) или 2 (быстро)")]
    public string selectedSpeed = "1";

    [Header("Настройки длительности траекторий")]
    [Tooltip("Словарь длительностей для каждой скорости")]
    public Dictionary<string, Dictionary<string, float>> trajectoryDurationsBySpeed =
        new Dictionary<string, Dictionary<string, float>>();

    [Tooltip("Длительность траектории: 1 (короткая), 2 (средняя) или 3 (длинная)")]
    public string trajectoryDuration = "1";

    [Header("Параметры траектории")]
    [Tooltip("Общая длина траектории = выбранная длительность + (occluder ? 1.0 : 0)")]
    public float totalLength = 0f;

    [Tooltip("Добавляет 1.0f к общей длине если true. Работает только при condition = 1")]
    public bool occluder = false;

    [Tooltip("Вариант от 0 до 4 (5 вариантов)")]
    [Range(0, 4)]
    public int variant = 0;

    [Tooltip("Условие: 1, 2 или 3 (occluder работает только при condition = 1)")]
    [Range(1, 3)]
    public int condition = 1;

    [Tooltip("Ось: 1 (X), 2 (Y) или 3 (Z)")]
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

    [Tooltip("Дополнительные точки при occluder = true (только для condition = 1)")]
    public int additionalPointsForOccluder = 2;

    [Header("Настройки генерации точек")]
    [Tooltip("Минимальное расстояние между точками для скорости '1' (медленно)")]
    public float minDistanceSpeed1 = 0.35f;

    [Tooltip("Максимальное расстояние между точками для скорости '1' (медленно)")]
    public float maxDistanceSpeed1 = 0.65f;

    [Tooltip("Минимальное расстояние между точками для скорости '2' (быстро)")]
    public float minDistanceSpeed2 = 0.7f;

    [Tooltip("Максимальное расстояние между точками для скорости '2' (быстро)")]
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

    // Оптимизация: кэши и пулы
    private Dictionary<int, Vector3> directionCache = new Dictionary<int, Vector3>();
    private Queue<Vector3[]> pointsPool = new Queue<Vector3[]>();

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
        // Формат: A + axis + C + condition + S + speed + D + duration + V + variant
        return $"A{axis}C{condition}S{selectedSpeed}D{trajectoryDuration}V{variant}";
        // Не добавляем O в конец для occluder
    }

    /// <summary>
    /// Получить целевое расстояние между точками
    /// </summary>
    float GetTargetDistance()
    {
        if (selectedSpeed == "1")
        {
            return UnityEngine.Random.Range(minDistanceSpeed1, maxDistanceSpeed1);
        }
        else // selectedSpeed == "2"
        {
            return UnityEngine.Random.Range(minDistanceSpeed2, maxDistanceSpeed2);
        }
    }

    /// <summary>
    /// Безопасное получение скорости шарика
    /// </summary>
    public float GetSafeBallSpeed()
    {
        return ballSpeeds.TryGetValue(selectedSpeed, out float speed)
            ? speed
            : 0.04f;
    }

    /// <summary>
    /// Безопасное получение количества точек с учетом occluder
    /// </summary>
    public int GetSafePointCount()
    {
        int baseCount = pointCountByDuration.TryGetValue(trajectoryDuration, out int count)
            ? count
            : 5;

        // Добавляем дополнительные точки только если occluder=true И condition=1
        if (occluder && condition == 1)
        {
            return baseCount + additionalPointsForOccluder;
        }

        return baseCount;
    }

    /// <summary>
    /// Получить базовую длину траектории (длительность без occluder)
    /// </summary>
    public float GetBaseTrajectoryLength()
    {
        if (trajectoryDurationsBySpeed.TryGetValue(selectedSpeed, out var speedDict) &&
            speedDict.TryGetValue(trajectoryDuration, out float length))
        {
            return length;
        }

        // Возвращаем значения по умолчанию если что-то пошло не так
        return selectedSpeed == "1" ? 1.0f : 2.0f;
    }

    /// <summary>
    /// Получить итоговую длину траектории (длительность + occluder если condition=1)
    /// </summary>
    public float GetTotalTrajectoryLength()
    {
        float baseLength = GetBaseTrajectoryLength();

        // Добавляем 1.0f только если occluder=true И condition=1
        if (occluder && condition == 1)
        {
            return baseLength + 1.0f;
        }

        return baseLength;
    }

    /// <summary>
    /// Получить количество точек для текущей длительности
    /// </summary>
    int GetPointCountForDuration()
    {
        return GetSafePointCount();
    }

    /// <summary>
    /// Проверить, активен ли occluder (только для condition=1)
    /// </summary>
    public bool IsOccluderActive()
    {
        return occluder && condition == 1;
    }

    /// <summary>
    /// Получить массив точек из пула или создать новый
    /// </summary>
    Vector3[] GetPointsArray(int size)
    {
        if (pointsPool.Count > 0)
        {
            foreach (var array in pointsPool)
            {
                if (array.Length == size)
                {
                    pointsPool = new Queue<Vector3[]>(pointsPool.Where(a => a != array));
                    return array;
                }
            }
        }

        return new Vector3[size];
    }

    /// <summary>
    /// Вернуть массив точек в пул
    /// </summary>
    void ReturnPointsArray(Vector3[] array)
    {
        if (array != null && pointsPool.Count < 20) // Ограничиваем размер пула
        {
            // Очищаем массив
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = Vector3.zero;
            }
            pointsPool.Enqueue(array);
        }
    }

    /// <summary>
    /// Сгенерировать траекторию на основе текущих параметров
    /// </summary>
    [ContextMenu("Сгенерировать траекторию")]
    public void GenerateTrajectory()
    {
        // 1. Получаем базовую длину (выбранную длительность)
        baseTrajectoryLength = GetBaseTrajectoryLength();

        // 2. Получаем итоговую длину (длительность + occluder если condition=1)
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
        Debug.Log($"Скорость: {selectedSpeed} ({(selectedSpeed == "1" ? "медленно" : "быстро")})");
        Debug.Log($"Длительность: {trajectoryDuration} ({(trajectoryDuration == "1" ? "короткая" : trajectoryDuration == "2" ? "средняя" : "длинная")})");
        Debug.Log($"Базовая длина (длительность): {baseTrajectoryLength:F1}");
        Debug.Log($"Итоговая длина: {totalLength:F2} {(IsOccluderActive() ? "(длительность +1.0 occluder)" : "(только длительность)")}");
        Debug.Log($"Condition: {condition} {(condition == 1 ? "(occluder разрешен)" : "(occluder не применяется)")}");
        Debug.Log($"Occluder: {occluder} {(IsOccluderActive() ? "(активен, +1.0 к длине)" : "(не активен)")}");
        Debug.Log($"Точек: {pointCount} (базовых: {pointCountByDuration[trajectoryDuration]}{(IsOccluderActive() ? $" + {additionalPointsForOccluder}" : "")})");
        Debug.Log($"Расстояние между точками: {pointDistance:F2}");
        Debug.Log($"Скорость шарика: {selectedSpeed} = {GetSafeBallSpeed():F3}");
    }

    /// <summary>
    /// Генерация массива точек траектории (оптимизированная версия)
    /// </summary>
    Vector3[] GenerateTrajectoryPoints(int pointCount, float totalLength)
    {
        Vector3[] points = GetPointsArray(pointCount);
        Vector3 direction = GetCachedDirectionFromAxis();

        // Предварительно вычисляем значения
        int lastIndex = pointCount - 1;

        for (int i = 0; i < pointCount; i++)
        {
            float t = (float)i / lastIndex;
            float distanceAlongPath = t * totalLength;
            Vector3 basePosition = direction * distanceAlongPath;
            basePosition = ApplyAxisRestrictions(basePosition);
            points[i] = ApplyConditionEffects(basePosition, t, totalLength);
        }

        return points;
    }

    /// <summary>
    /// Получить направление с кэшированием
    /// </summary>
    Vector3 GetCachedDirectionFromAxis()
    {
        if (!directionCache.TryGetValue(axis, out Vector3 direction))
        {
            direction = GetDirectionFromAxis();
            directionCache[axis] = direction;
        }
        return direction;
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
            case 1: // Condition 1: небольшие случайные отклонения (occluder разрешен)
                float noise1 = 0.05f * totalLength;
                result += new Vector3(
                    UnityEngine.Random.Range(-noise1, noise1),
                    UnityEngine.Random.Range(-noise1, noise1),
                    UnityEngine.Random.Range(-noise1, noise1)
                );
                break;

            case 2: // Condition 2: синусоидальные отклонения (occluder не применяется)
                float amplitude = 0.1f * totalLength;
                float frequency = 4f;
                result.x += Mathf.Sin(t * Mathf.PI * frequency) * amplitude;
                result.y += Mathf.Cos(t * Mathf.PI * frequency) * amplitude * 0.5f;
                break;

            case 3: // Condition 3: спиральные отклонения (occluder не применяется)
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
    /// Проверить, является ли траектория дубликатом существующей
    /// </summary>
    public bool IsTrajectoryDuplicate(Vector3[] newPoints, float tolerance = 0.01f)
    {
        foreach (var existing in trajectoriesDictionary.Values)
        {
            if (AreTrajectoriesEqual(existing, newPoints, tolerance))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Сравнить две траектории
    /// </summary>
    bool AreTrajectoriesEqual(Vector3[] a, Vector3[] b, float tolerance)
    {
        if (a.Length != b.Length) return false;

        for (int i = 0; i < a.Length; i++)
        {
            if (Vector3.Distance(a[i], b[i]) > tolerance)
                return false;
        }
        return true;
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

        // Проверяем на дубликат
        if (IsTrajectoryDuplicate(trajectoryPoints))
        {
            Debug.LogWarning($"Траектория '{generatedTrajectoryName}' похожа на существующую! Не сохранена.");
            return;
        }

        // Копируем массив точек
        Vector3[] pointsCopy = new Vector3[trajectoryPoints.Length];
        System.Array.Copy(trajectoryPoints, pointsCopy, trajectoryPoints.Length);

        // Добавляем или обновляем в словаре
        if (trajectoriesDictionary.ContainsKey(generatedTrajectoryName))
        {
            // Возвращаем старый массив в пул
            if (trajectoriesDictionary.TryGetValue(generatedTrajectoryName, out Vector3[] oldArray))
            {
                ReturnPointsArray(oldArray);
            }

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
        // Возвращаем старые массивы в пул
        foreach (var settings in trajectoryList)
        {
            if (settings.points != null)
            {
                ReturnPointsArray(settings.points);
            }
        }

        trajectoryList.Clear();

        // Создаем отсортированный список названий
        var sortedKeys = trajectoriesDictionary.Keys.OrderBy(key => key).ToList();

        foreach (var key in sortedKeys)
        {
            Vector3[] points = trajectoriesDictionary[key];

            // Рассчитываем длину траектории
            float length = CalculateTrajectoryLength(points);

            // Извлекаем параметры из названия
            var parameters = ExtractAllParameters(key);

            // Определяем, есть ли occluder (на основе сохраненных параметров)
            // Поскольку O не в названии, проверяем по базовой длине
            bool hasOccluder = DetermineOccluderFromParameters(key, parameters);

            trajectoryList.Add(new TrajectorySettings(
                key,
                points,
                length,
                parameters.speed,
                parameters.duration,
                pointDistance,
                hasOccluder
            ));
        }

        Debug.Log($"Список траекторий обновлен и отсортирован по алфавиту. Всего: {trajectoryList.Count}");
    }

    /// <summary>
    /// Определить есть ли occluder по параметрам
    /// </summary>
    bool DetermineOccluderFromParameters(string trajectoryName, (int axis, int condition, string speed, string duration, int variant) parameters)
    {
        // Поскольку O не в названии, мы должны определить occluder другими способами
        // В данном случае будем проверять длину траектории

        float expectedBaseLength = GetExpectedBaseLength(parameters.speed, parameters.duration);
        float actualLength = CalculateTrajectoryLength(trajectoriesDictionary[trajectoryName]);

        // Если фактическая длина примерно на 1.0 больше базовой, значит есть occluder
        return Mathf.Abs(actualLength - (expectedBaseLength + 1.0f)) < 0.1f && parameters.condition == 1;
    }

    /// <summary>
    /// Получить ожидаемую базовую длину
    /// </summary>
    float GetExpectedBaseLength(string speed, string duration)
    {
        if (trajectoryDurationsBySpeed.TryGetValue(speed, out var speedDict) &&
            speedDict.TryGetValue(duration, out float length))
        {
            return length;
        }
        return speed == "1" ? 1.0f : 2.0f;
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
    /// Извлечь все параметры из названия траектории
    /// </summary>
    (int axis, int condition, string speed, string duration, int variant) ExtractAllParameters(string trajectoryName)
    {
        int resultAxis = 1;
        int resultCondition = 1;
        string resultSpeed = "1";
        string resultDuration = "1";
        int resultVariant = 0;

        Dictionary<char, Action<string>> paramExtractors = new Dictionary<char, Action<string>>()
        {
            { 'A', (value) => resultAxis = ParseInt(value, 1) },
            { 'C', (value) => resultCondition = ParseInt(value, 1) },
            { 'S', (value) => resultSpeed = value },
            { 'D', (value) => resultDuration = value },
            { 'V', (value) => resultVariant = ParseInt(value, 0) }
        };

        for (int i = 0; i < trajectoryName.Length - 1; i++)
        {
            if (paramExtractors.TryGetValue(trajectoryName[i], out Action<string> extractor))
            {
                string value = trajectoryName[i + 1].ToString();
                extractor(value);
            }
        }

        return (resultAxis, resultCondition, resultSpeed, resultDuration, resultVariant);
    }

    int ParseInt(string value, int defaultValue)
    {
        return int.TryParse(value, out int result) ? result : defaultValue;
    }

    /// <summary>
    /// Загрузить траекторию из словаря по имени
    /// </summary>
    public void LoadTrajectory(string trajectoryName)
    {
        if (trajectoriesDictionary.TryGetValue(trajectoryName, out Vector3[] points))
        {
            // Возвращаем текущие точки в пул
            if (trajectoryPoints != null)
            {
                ReturnPointsArray(trajectoryPoints);
            }

            trajectoryPoints = points;
            generatedTrajectoryName = trajectoryName;

            // Извлекаем параметры из названия
            var parameters = ExtractAllParameters(trajectoryName);
            axis = parameters.axis;
            condition = parameters.condition;
            selectedSpeed = parameters.speed;
            trajectoryDuration = parameters.duration;
            variant = parameters.variant;

            // Определяем occluder по длине траектории
            float expectedBaseLength = GetExpectedBaseLength(parameters.speed, parameters.duration);
            float actualLength = CalculateTrajectoryLength(points);
            occluder = Mathf.Abs(actualLength - (expectedBaseLength + 1.0f)) < 0.1f && parameters.condition == 1;

            // Обновляем длину на основе извлеченных параметров
            UpdateLengthFromParameters();

            // Обновляем количество точек
            pointCount = trajectoryPoints.Length;

            Debug.Log($"Загружена траектория: {trajectoryName}");
            Debug.Log($"Определен occluder: {occluder} ({(occluder && condition == 1 ? "активен" : "не активен")})");
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
    /// Очистить словарь траекторий
    /// </summary>
    [ContextMenu("Очистить словарь траекторий")]
    public void ClearTrajectoriesDictionary()
    {
        // Возвращаем все массивы в пул
        foreach (var array in trajectoriesDictionary.Values)
        {
            ReturnPointsArray(array);
        }

        foreach (var settings in trajectoryList)
        {
            if (settings.points != null)
            {
                ReturnPointsArray(settings.points);
            }
        }

        trajectoriesDictionary.Clear();
        trajectoryList.Clear();

        // Очищаем кэш направлений
        directionCache.Clear();

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
        return GetSafeBallSpeed();
    }

    /// <summary>
    /// Получить итоговую длину траектории по параметрам
    /// </summary>
    public float CalculateTotalLength(string speed, string duration, bool hasOccluder, int conditionValue)
    {
        if (trajectoryDurationsBySpeed.TryGetValue(speed, out var speedDict) &&
            speedDict.TryGetValue(duration, out float baseLength))
        {
            // Добавляем 1.0f только если occluder=true И condition=1
            if (hasOccluder && conditionValue == 1)
            {
                return baseLength + 1.0f;
            }
            return baseLength;
        }

        float defaultLength = speed == "1" ? 1.0f : 2.0f;
        if (hasOccluder && conditionValue == 1)
        {
            return defaultLength + 1.0f;
        }
        return defaultLength;
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
        List<(string name, Vector3[] points, bool hasOccluder)> tempTrajectories =
            new List<(string, Vector3[], bool)>();

        foreach (var speed in speeds)
        {
            foreach (var duration in durations)
            {
                foreach (var cond in conditions)
                {
                    foreach (var ax in axes)
                    {
                        // 5 вариантов (0-4)
                        for (int var = 0; var <= 4; var++)
                        {
                            // Устанавливаем параметры
                            selectedSpeed = speed;
                            trajectoryDuration = duration;
                            condition = cond;
                            axis = ax;
                            variant = var;

                            // Для condition=1 генерируем с occluder и без
                            if (cond == 1)
                            {
                                // Без occluder
                                occluder = false;
                                GenerateTrajectory();
                                Vector3[] pointsCopy = new Vector3[trajectoryPoints.Length];
                                System.Array.Copy(trajectoryPoints, pointsCopy, trajectoryPoints.Length);
                                tempTrajectories.Add((generatedTrajectoryName, pointsCopy, false));

                                // С occluder
                                occluder = true;
                                GenerateTrajectory();
                                pointsCopy = new Vector3[trajectoryPoints.Length];
                                System.Array.Copy(trajectoryPoints, pointsCopy, trajectoryPoints.Length);
                                tempTrajectories.Add((generatedTrajectoryName, pointsCopy, true));
                            }
                            else
                            {
                                // Для condition 2 и 3 только без occluder
                                occluder = false;
                                GenerateTrajectory();
                                Vector3[] pointsCopy = new Vector3[trajectoryPoints.Length];
                                System.Array.Copy(trajectoryPoints, pointsCopy, trajectoryPoints.Length);
                                tempTrajectories.Add((generatedTrajectoryName, pointsCopy, false));
                            }
                        }
                    }
                }
            }
        }

        // Сортируем по алфавиту и добавляем в словарь
        var sortedTrajectories = tempTrajectories.OrderBy(t => t.name).ToList();

        foreach (var trajectory in sortedTrajectories)
        {
            // Проверяем, есть ли уже такая траектория
            if (!trajectoriesDictionary.ContainsKey(trajectory.name))
            {
                trajectoriesDictionary.Add(trajectory.name, trajectory.points);
            }
            else
            {
                // Если уже есть, добавляем с суффиксом для различия
                string newName = $"{trajectory.name}_{(trajectory.hasOccluder ? "O" : "NO")}";
                trajectoriesDictionary.Add(newName, trajectory.points);
            }
        }

        // Обновляем список для отображения
        UpdateTrajectoryList();

        Debug.Log($"Сгенерировано {trajectoriesDictionary.Count} тестовых траекторий (отсортировано по алфавиту)");

        // Выводим статистику
        int withOccluder = trajectoryList.Count(t => t.hasOccluder);
        int withoutOccluder = trajectoryList.Count(t => !t.hasOccluder);
        Debug.Log($"С occluder: {withOccluder}, Без occluder: {withoutOccluder}");
    }

    /// <summary>
    /// Получить базовое количество точек (без occluder)
    /// </summary>
    public int GetBasePointCount()
    {
        return pointCountByDuration.TryGetValue(trajectoryDuration, out int count)
            ? count
            : 5;
    }

    /// <summary>
    /// Сохранить траектории в файл
    /// </summary>
    public void SaveTrajectoriesToFile(string fileName = "trajectories.json")
    {
        TrajectoryCollection collection = new TrajectoryCollection();

        foreach (var kvp in trajectoriesDictionary)
        {
            var parameters = ExtractAllParameters(kvp.Key);

            // Определяем есть ли occluder
            float expectedBaseLength = GetExpectedBaseLength(parameters.speed, parameters.duration);
            float actualLength = CalculateTrajectoryLength(kvp.Value);
            bool hasOccluder = Mathf.Abs(actualLength - (expectedBaseLength + 1.0f)) < 0.1f && parameters.condition == 1;

            TrajectoryData data = new TrajectoryData
            {
                name = kvp.Key,
                points = Vector3Serializable.FromVector3Array(kvp.Value),
                totalLength = actualLength,
                speed = parameters.speed,
                duration = parameters.duration,
                hasOccluder = hasOccluder
            };

            collection.trajectories.Add(data);
        }

        string json = JsonUtility.ToJson(collection, true);
        string path = System.IO.Path.Combine(Application.persistentDataPath, fileName);
        System.IO.File.WriteAllText(path, json);

        Debug.Log($"Сохранено {collection.trajectories.Count} траекторий в файл: {path}");
    }

    /// <summary>
    /// Загрузить траектории из файла
    /// </summary>
    public void LoadTrajectoriesFromFile(string fileName = "trajectories.json")
    {
        string path = System.IO.Path.Combine(Application.persistentDataPath, fileName);

        if (!System.IO.File.Exists(path))
        {
            Debug.LogWarning($"Файл не найден: {path}");
            return;
        }

        string json = System.IO.File.ReadAllText(path);
        TrajectoryCollection collection = JsonUtility.FromJson<TrajectoryCollection>(json);

        ClearTrajectoriesDictionary();

        foreach (var data in collection.trajectories)
        {
            Vector3[] points = Vector3Serializable.ToVector3Array(data.points);
            trajectoriesDictionary.Add(data.name, points);
        }

        UpdateTrajectoryList();

        Debug.Log($"Загружено {collection.trajectories.Count} траекторий из файла: {path}");
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

            string speedInfo = selectedSpeed switch
            {
                "1" => "медленно (0.04)",
                "2" => "быстро (0.08)",
                _ => "неизвестно"
            };

            string durationInfo = trajectoryDuration switch
            {
                "1" => "короткая",
                "2" => "средняя",
                "3" => "длинная",
                _ => "неизвестно"
            };

            string occluderInfo = IsOccluderActive() ? "ДА (+1.0)" : "НЕТ";

            float ballSpeedValue = GetBallSpeed();
            int basePointCount = GetBasePointCount();

            Handles.Label(labelPos,
                $"Траектория: {generatedTrajectoryName}\n" +
                $"Скорость: {selectedSpeed} ({speedInfo})\n" +
                $"Длительность: {trajectoryDuration} ({durationInfo}) = {baseTrajectoryLength:F1}\n" +
                $"Итоговая длина: {totalLength:F2}\n" +
                $"Condition: {condition} {(condition == 1 ? "(occluder разрешен)" : "(occluder не применяется)")}\n" +
                $"Occluder: {occluderInfo}\n" +
                $"Axis: {axis} ({axisInfo})\n" +
                $"Variant: {variant}\n" +
                $"Точек: {pointCount} (базовых: {basePointCount}{(IsOccluderActive() ? $" + {additionalPointsForOccluder}" : "")})\n" +
                $"Расстояние между точками: {pointDistance:F2}");
        }
#endif
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(BallTrajectoryGeneratorV4))]
public class BallTrajectoryGeneratorV4Editor : Editor
{
    private Vector2 scrollPos;
    private bool showTrajectories = false;
    private bool showSettings = true;
    private string saveFileName = "trajectories.json";
    private string loadFileName = "trajectories.json";

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BallTrajectoryGeneratorV4 script = (BallTrajectoryGeneratorV4)target;

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Управление генерацией", EditorStyles.boldLabel);

        // Проверка допустимости скорости
        if (script.selectedSpeed != "1" && script.selectedSpeed != "2")
        {
            EditorGUILayout.HelpBox("Скорость должна быть '1' или '2'", MessageType.Error);
        }

        // Проверка occluder для condition
        if (script.occluder && script.condition != 1)
        {
            EditorGUILayout.HelpBox("Occluder применяется только при Condition = 1", MessageType.Warning);
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

        EditorGUILayout.LabelField($"Condition: {script.condition} {(script.condition == 1 ? "(occluder разрешен)" : "(occluder не применяется)")}",
            EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"Variant: {script.variant} (0-4, 5 вариантов)", EditorStyles.miniLabel);

        bool isOccluderActive = script.IsOccluderActive();
        EditorGUILayout.LabelField($"Occluder: {script.occluder} {(isOccluderActive ? "(активен, +1.0 к длине)" : "(не активен)")}",
            EditorStyles.miniLabel);

        EditorGUILayout.LabelField($"Базовая длина (длительность): {baseLength:F1}", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"Итоговая длина: {script.totalLength:F2} {(isOccluderActive ? "(длительность +1.0)" : "(только длительность)")}",
            EditorStyles.miniLabel);

        int totalPoints = script.pointCount;
        EditorGUILayout.LabelField($"Точек: {totalPoints} (базовых: {basePoints}{(isOccluderActive ? $" + {script.additionalPointsForOccluder}" : "")})",
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
        if (GUILayout.Button("Тестовые траектории (5 вариантов)"))
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

        // Сохранение/загрузка файлов
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Сохранение/Загрузка файлов", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        saveFileName = EditorGUILayout.TextField("Имя файла:", saveFileName);
        if (GUILayout.Button("Сохранить в файл", GUILayout.Width(150)))
        {
            script.SaveTrajectoriesToFile(saveFileName);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        loadFileName = EditorGUILayout.TextField("Имя файла:", loadFileName);
        if (GUILayout.Button("Загрузить из файла", GUILayout.Width(150)))
        {
            script.LoadTrajectoriesFromFile(loadFileName);
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

            int withOccluder = script.trajectoryList.Count(t => t.hasOccluder);
            int withoutOccluder = script.trajectoryList.Count(t => !t.hasOccluder);
            EditorGUILayout.LabelField($"С occluder: {withOccluder}, Без occluder: {withoutOccluder}",
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

                EditorGUILayout.LabelField($"Точек: {trajectory.points.Length}", GUILayout.Width(60));
                EditorGUILayout.LabelField($"Длина: {trajectory.totalLength:F2}", GUILayout.Width(60));

                if (trajectory.hasOccluder)
                {
                    EditorGUILayout.LabelField("O", GUILayout.Width(20));
                }

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

                EditorGUILayout.LabelField($"Скорость: {trajectory.speed}, Длительность: {trajectory.duration}, Occluder: {trajectory.hasOccluder}",
                    EditorStyles.miniLabel);

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndScrollView();
        }
    }
}
#endif