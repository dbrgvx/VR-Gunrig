using UnityEngine;
using System.Collections.Generic;

public class SculptureInfoPanel : MonoBehaviour
{
    [Tooltip("Объект игрока (для проверки расстояния)")]
    public Transform player;

    [Tooltip("Скорость анимации появления объектов")]
    public float animationSpeed = 5.0f;

    [Tooltip("Проверять расстояние с интервалом (секунды)")]
    public float checkInterval = 0.1f;

    [Tooltip("Задержка перед выключением (секунды)")]
    public float deactivationDelay = 2.0f;

    [Tooltip("Включить отладку")]
    public bool debugMode = false;

    [System.Serializable]
    public class SculpturePanel
    {
        public Transform sculpture; // Скульптура
        public GameObject infoPanel; // Информационная панель
        [Tooltip("Радиус активации (в метрах)")]
        public float activationDistance = 2.5f;
        [HideInInspector]
        public bool isActive = false;
        [HideInInspector]
        public bool isInRadius = false;
        [HideInInspector]
        public float deactivationTimer = 0f;
        [Tooltip("Дополнительные объекты, которые будут активироваться (свет, эффекты и др.)")]
        public List<GameObject> additionalObjects = new List<GameObject>();
    }

    [Header("Скульптуры и связанные объекты")]
    public List<SculpturePanel> sculpturePanels = new List<SculpturePanel>();

    private float timer = 0f;
    private Dictionary<GameObject, Vector3> originalScale = new Dictionary<GameObject, Vector3>();
    private Dictionary<GameObject, float> currentScaleFactor = new Dictionary<GameObject, float>();
    private Dictionary<GameObject, bool> objectActivationState = new Dictionary<GameObject, bool>();
    private Dictionary<GameObject, float> originalLightIntensity = new Dictionary<GameObject, float>();

    // Пороговые значения для масштаба
    private const float ACTIVATION_THRESHOLD = 0.05f;
    private const float DEACTIVATION_THRESHOLD = 0.01f;

    void Start()
    {
        if (player == null)
        {
            Debug.LogError("Не назначен объект player!");
            enabled = false;
            return;
        }

        // Инициализация всех объектов
        foreach (var item in sculpturePanels)
        {
            InitializeItem(item);
        }

        // Немедленно проверяем расстояние до скульптур при старте
        CheckSculptureProximity();

        // Обновляем видимость объектов на основе начальной проверки
        UpdateObjectVisibility();

        if (debugMode)
            Debug.Log("SculptureInfoPanel: инициализация завершена. Найдено " + sculpturePanels.Count + " скульптур.");
    }

    void InitializeItem(SculpturePanel item)
    {
        // Настройка панели
        if (item.infoPanel != null)
        {
            // Сохраняем оригинальный масштаб
            originalScale[item.infoPanel] = item.infoPanel.transform.localScale;
            currentScaleFactor[item.infoPanel] = 0f;
            objectActivationState[item.infoPanel] = false;

            // Скрываем объект
            item.infoPanel.transform.localScale = Vector3.zero;
            item.infoPanel.SetActive(false);
        }

        // Настройка дополнительных объектов
        foreach (var obj in item.additionalObjects)
        {
            if (obj != null)
            {
                // Запоминаем исходное состояние активации
                objectActivationState[obj] = false;

                // Сохраняем оригинальный масштаб
                originalScale[obj] = obj.transform.localScale;
                currentScaleFactor[obj] = 0f;

                // Для источников света запоминаем исходную интенсивность
                Light light = obj.GetComponent<Light>();
                if (light != null)
                {
                    originalLightIntensity[obj] = light.intensity;
                    light.intensity = 0f;
                }

                // Изначально скрываем объект
                obj.transform.localScale = Vector3.zero;
                obj.SetActive(false);
            }
        }

        // Изначально неактивны
        item.isActive = false;
        item.isInRadius = false;
        item.deactivationTimer = 0f;
    }

    void Update()
    {
        // Проверяем расстояние с интервалом
        timer += Time.deltaTime;
        if (timer >= checkInterval)
        {
            CheckSculptureProximity();
            timer = 0f;
        }

        // Обработка таймеров деактивации
        ProcessDeactivationTimers();

        // Обновляем видимость объектов
        UpdateObjectVisibility();
    }

    void CheckSculptureProximity()
    {
        foreach (var item in sculpturePanels)
        {
            if (item.sculpture == null) continue;

            // Проверяем расстояние до скульптуры
            float distance = Vector3.Distance(player.position, item.sculpture.position);

            // Сохраняем предыдущее состояние
            bool wasInRadius = item.isInRadius;

            // Обновляем состояние нахождения в радиусе
            item.isInRadius = distance <= item.activationDistance;

            // Если вошел в радиус
            if (!wasInRadius && item.isInRadius)
            {
                item.isActive = true;
                item.deactivationTimer = 0f; // Сбрасываем таймер

                if (debugMode)
                    Debug.Log("Игрок вошел в радиус скульптуры: " + item.sculpture.name);
            }
            // Если вышел из радиуса
            else if (wasInRadius && !item.isInRadius)
            {
                // Запускаем таймер деактивации, но пока не деактивируем
                item.deactivationTimer = deactivationDelay;

                if (debugMode)
                    Debug.Log("Игрок вышел из радиуса скульптуры: " + item.sculpture.name + ". Начало отсчета таймера.");
            }
        }
    }

    void ProcessDeactivationTimers()
    {
        foreach (var item in sculpturePanels)
        {
            // Если запущен таймер деактивации
            if (item.deactivationTimer > 0)
            {
                // Уменьшаем таймер
                item.deactivationTimer -= Time.deltaTime;

                // Если таймер истек
                if (item.deactivationTimer <= 0)
                {
                    item.isActive = false;

                    if (debugMode)
                        Debug.Log("Таймер истек, деактивация объектов для скульптуры: " + item.sculpture.name);
                }
            }
        }
    }

    void UpdateObjectVisibility()
    {
        foreach (var item in sculpturePanels)
        {
            float targetScale = item.isActive ? 1f : 0f;

            // Обновляем панель
            AnimateObject(item.infoPanel, targetScale);

            // Обновляем все дополнительные объекты
            foreach (var obj in item.additionalObjects)
            {
                if (obj != null)
                {
                    AnimateObject(obj, targetScale);
                }
            }
        }
    }

    void AnimateObject(GameObject obj, float targetScale)
    {
        if (obj == null) return;

        // Проверяем текущее состояние активации
        bool isCurrentlyActive = objectActivationState.ContainsKey(obj) ?
                                objectActivationState[obj] : obj.activeSelf;

        // Получаем текущий масштаб
        float currentScale = currentScaleFactor.ContainsKey(obj) ? currentScaleFactor[obj] : 0f;

        // Постепенно изменяем масштаб
        float newScale = Mathf.Lerp(currentScale, targetScale, Time.deltaTime * animationSpeed);
        currentScaleFactor[obj] = newScale;

        // Логика активации с гистерезисом
        if (!isCurrentlyActive && newScale > ACTIVATION_THRESHOLD)
        {
            // Активируем объект только когда масштаб достаточно большой
            obj.SetActive(true);
            objectActivationState[obj] = true;

            if (debugMode)
                Debug.Log("Активация объекта: " + obj.name + " (scale: " + newScale + ")");
        }
        else if (isCurrentlyActive && newScale < DEACTIVATION_THRESHOLD)
        {
            // Деактивируем объект только когда масштаб почти нулевой
            obj.SetActive(false);
            objectActivationState[obj] = false;

            if (debugMode)
                Debug.Log("Деактивация объекта: " + obj.name + " (scale: " + newScale + ")");
        }

        // Применяем масштаб к объекту, если он активен
        if (obj.activeSelf)
        {
            // Источники света обрабатываем отдельно
            Light light = obj.GetComponent<Light>();
            if (light != null)
            {
                // Для света используем интенсивность вместо масштаба
                float originalIntensity = originalLightIntensity.ContainsKey(obj) ?
                                         originalLightIntensity[obj] : 1.0f;
                light.intensity = originalIntensity * newScale;
            }
            else
            {
                // Для остальных объектов изменяем масштаб
                Vector3 origScale = originalScale.ContainsKey(obj) ? originalScale[obj] : Vector3.one;
                obj.transform.localScale = origScale * newScale;
            }
        }
    }

    // Визуализация в редакторе
    void OnDrawGizmosSelected()
    {
        foreach (var item in sculpturePanels)
        {
            if (item.sculpture != null)
            {
                // Рисуем сферу радиуса активации
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(item.sculpture.position, item.activationDistance);

#if UNITY_EDITOR
                UnityEditor.Handles.color = Color.white;
                UnityEditor.Handles.Label(item.sculpture.position + Vector3.up * 0.5f,
                    item.sculpture.name + " (Радиус: " + item.activationDistance + "м)");
#endif
            }
        }
    }
}