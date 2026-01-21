using UnityEngine;

public class SimpleMoveTowards : MonoBehaviour
{
    [Header("Настройки движения")]
    [Tooltip("Расстояние для перемещения в единицах Unity")]
    public float distance = 10f;

    [Tooltip("Время перемещения в секундах")]
    public float moveTime = 3f;

    [Tooltip("Направление движения (нормализуется автоматически)")]
    public Vector3 direction = Vector3.forward;

    [Header("Опции")]
    [Tooltip("Автоматически начать движение при старте")]
    public bool startOnAwake = true;

    [Tooltip("Вращать объект в направлении движения")]
    public bool rotateTowardsDirection = false;

    // Приватные переменные состояния
    private Vector3 startPosition;
    private Vector3 targetPosition;
    public float currentTime = 0f;
    private bool isMoving = false;
    private Vector3 moveDirection;

    void Start()
    {
        // Нормализуем направление (делаем длину вектора = 1)
        moveDirection = direction.normalized;

        // Запоминаем стартовую позицию
        startPosition = transform.position;

        // Вычисляем целевую позицию
        targetPosition = startPosition + moveDirection * distance;

        // Поворачиваем объект в направлении движения, если нужно
        if (rotateTowardsDirection && moveDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }

        // Начинаем движение, если установлена опция
        if (startOnAwake)
        {
            StartMoving();
        }
    }

    void Update()
    {
        // Если движение не активно - выходим
        if (!isMoving) return;

        // Увеличиваем счетчик времени
        currentTime += Time.deltaTime;

        // Вычисляем прогресс от 0 до 1
        float progress = Mathf.Clamp01(currentTime / moveTime);

        // Вычисляем целевую позицию на основе прогресса
        // (для плавного линейного движения)
        Vector3 currentTarget = Vector3.Lerp(startPosition, targetPosition, progress);

        // Вычисляем необходимую скорость для прохождения расстояния за указанное время
        float requiredSpeed = distance / moveTime;

        // Вычисляем максимальное расстояние, которое можно пройти за этот кадр
        float maxStep = requiredSpeed * Time.deltaTime;

        // Перемещаем объект с помощью MoveTowards
        // Это обеспечивает точное достижение конечной точки без "проскакивания"
        transform.position = Vector3.MoveTowards(
            transform.position,    // Текущая позиция
            currentTarget,         // Целевая позиция на текущий момент
            maxStep                // Максимальный шаг за кадр
        );

        // Если достигли конца времени движения
        if (progress >= 1f)
        {
            // Гарантируем точное позиционирование в конечной точке
            transform.position = targetPosition;

            // Останавливаем движение
            isMoving = false;

            // Вызываем метод завершения
            OnMovementComplete();
        }
    }

    /// <summary>
    /// Начать движение объекта
    /// </summary>
    public void StartMoving()
    {
        // Сбрасываем таймер
        currentTime = 0f;

        // Запоминаем текущую позицию как стартовую
        startPosition = transform.position;

        // Пересчитываем целевую позицию
        targetPosition = startPosition + moveDirection * distance;

        // Активируем движение
        isMoving = true;

        Debug.Log($"Начато движение на расстояние {distance} за {moveTime} секунд");
    }

    /// <summary>
    /// Остановить движение объекта
    /// </summary>
    public void StopMoving()
    {
        isMoving = false;
        Debug.Log("Движение остановлено");
    }

    /// <summary>
    /// Перезапустить движение с текущей позиции
    /// </summary>
    public void RestartMovement()
    {
        StartMoving();
    }

    /// <summary>
    /// Метод вызывается при завершении движения
    /// </summary>
    private void OnMovementComplete()
    {
        Debug.Log($"Движение завершено! Пройдено {distance} единиц за {moveTime} секунд");

        // Здесь можно добавить дополнительные действия:
        // - Включить/выключить другие компоненты
        // - Отправить событие
        // - Запустить анимацию и т.д.
    }

    /// <summary>
    /// Изменить направление движения во время работы
    /// </summary>
    public void ChangeDirection(Vector3 newDirection)
    {
        moveDirection = newDirection.normalized;

        // Если движение активно, пересчитываем цель
        if (isMoving)
        {
            targetPosition = startPosition + moveDirection * distance;
        }
    }

    /// <summary>
    /// Визуализация в редакторе Unity
    /// </summary>
    void OnDrawGizmosSelected()
    {
        // Рисуем линию пути в редакторе
        Gizmos.color = Color.green;

        Vector3 start;
        Vector3 end;

        if (Application.isPlaying)
        {
            // В режиме игры показываем актуальные позиции
            start = startPosition;
            end = targetPosition;
        }
        else
        {
            // В редакторе показываем относительные позиции
            start = transform.position;
            end = transform.position + direction.normalized * distance;
        }

        // Рисуем линию пути
        Gizmos.DrawLine(start, end);

        // Рисуем сферы в начале и конце
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(start, 0.2f);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(end, 0.3f);
    }
}
   