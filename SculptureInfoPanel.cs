using UnityEngine;
using System.Collections.Generic;

public class SculptureInfoPanel : MonoBehaviour
{
    [Tooltip("Ссылка на игрока (камера/риги)")]
    public Transform player;

    [Tooltip("Скорость анимации появления/исчезновения")]
    public float animationSpeed = 5.0f;

    [Tooltip("Как часто проверять дистанцию (сек)")]
    public float checkInterval = 0.1f;

    [Tooltip("Задержка до скрытия (сек)")]
    public float deactivationDelay = 2.0f;

    [Tooltip("Отладочные логи")]
    public bool debugMode = false;

    [System.Serializable]
    public class SculpturePanel
    {
        public Transform sculpture; // ����������
        public GameObject infoPanel; // Панель с инфо
        [Tooltip("Дистанция активации (в метрах)")]
        public float activationDistance = 2.5f;
        [HideInInspector]
        public bool isActive = false;
        [HideInInspector]
        public bool isInRadius = false;
        [HideInInspector]
        public float deactivationTimer = 0f;
        [Tooltip("Доп. объекты, которые тоже показывать (иконки, свет и т.п.)")]
        public List<GameObject> additionalObjects = new List<GameObject>();
    }

    [Header("Скульптуры и их панели")]
    public List<SculpturePanel> sculpturePanels = new List<SculpturePanel>();

    private float timer = 0f;
    private Dictionary<GameObject, Vector3> originalScale = new Dictionary<GameObject, Vector3>();
    private Dictionary<GameObject, float> currentScaleFactor = new Dictionary<GameObject, float>();
    private Dictionary<GameObject, bool> objectActivationState = new Dictionary<GameObject, bool>();
    private Dictionary<GameObject, float> originalLightIntensity = new Dictionary<GameObject, float>();

    // Порог активации/скрытия
    private const float ACTIVATION_THRESHOLD = 0.05f;
    private const float DEACTIVATION_THRESHOLD = 0.01f;

    void Start()
    {
        if (player == null)
        {
            Debug.LogError("Не задан player!");
            enabled = false;
            return;
        }

        // Инициализирую элементы
        foreach (var item in sculpturePanels)
        {
            InitializeItem(item);
        }

        // Первичная проверка дистанции
        CheckSculptureProximity();

        // Синхронизирую видимость объектов
        UpdateObjectVisibility();

        if (debugMode)
            Debug.Log("SculptureInfoPanel: инициализация завершена. Элементов: " + sculpturePanels.Count);
    }

    void InitializeItem(SculpturePanel item)
    {
        // Панель
        if (item.infoPanel != null)
        {
            // Сохраняю исходный масштаб
            originalScale[item.infoPanel] = item.infoPanel.transform.localScale;
            currentScaleFactor[item.infoPanel] = 0f;
            objectActivationState[item.infoPanel] = false;

            // Прячу
            item.infoPanel.transform.localScale = Vector3.zero;
            item.infoPanel.SetActive(false);
        }

        // Дополнительные объекты
        foreach (var obj in item.additionalObjects)
        {
            if (obj != null)
            {
                // Сохраняю состояние
                objectActivationState[obj] = false;

                // Сохраняю масштаб
                originalScale[obj] = obj.transform.localScale;
                currentScaleFactor[obj] = 0f;

                // Если это свет — заношу исходную интенсивность
                Light light = obj.GetComponent<Light>();
                if (light != null)
                {
                    originalLightIntensity[obj] = light.intensity;
                    light.intensity = 0f;
                }

                // Прячу
                obj.transform.localScale = Vector3.zero;
                obj.SetActive(false);
            }
        }

        // ���������� ���������
        item.isActive = false;
        item.isInRadius = false;
        item.deactivationTimer = 0f;
    }

    void Update()
    {
        // Таймер проверки дистанции
        timer += Time.deltaTime;
        if (timer >= checkInterval)
        {
            CheckSculptureProximity();
            timer = 0f;
        }

        // Обрабатываю таймер скрытия
        ProcessDeactivationTimers();

        // Обновляю видимость
        UpdateObjectVisibility();
    }

    void CheckSculptureProximity()
    {
        foreach (var item in sculpturePanels)
        {
            if (item.sculpture == null) continue;

            // Дистанция до скульптуры
            float distance = Vector3.Distance(player.position, item.sculpture.position);

            // Сохраняю старое состояние
            bool wasInRadius = item.isInRadius;

            // В радиусе или нет
            item.isInRadius = distance <= item.activationDistance;

            // Вошёл в радиус
            if (!wasInRadius && item.isInRadius)
            {
                item.isActive = true;
                item.deactivationTimer = 0f;

                if (debugMode)
                    Debug.Log("В радиусе скульптуры: " + item.sculpture.name);
            }
            // Вышел из радиуса
            else if (wasInRadius && !item.isInRadius)
            {
                // Запускаю таймер скрытия
                item.deactivationTimer = deactivationDelay;

                if (debugMode)
                    Debug.Log("Покинул радиус: " + item.sculpture.name + ". Запущена задержка скрытия.");
            }
        }
    }

    void ProcessDeactivationTimers()
    {
        foreach (var item in sculpturePanels)
        {
            // Если идёт отсчёт до скрытия
            if (item.deactivationTimer > 0)
            {
                // Тикаю таймер
                item.deactivationTimer -= Time.deltaTime;

                // Истёк — скрываю
                if (item.deactivationTimer <= 0)
                {
                    item.isActive = false;

                    if (debugMode)
                        Debug.Log("Спрятал элементы для: " + item.sculpture.name);
                }
            }
        }
    }

    void UpdateObjectVisibility()
    {
        foreach (var item in sculpturePanels)
        {
            float targetScale = item.isActive ? 1f : 0f;

            // Панель
            AnimateObject(item.infoPanel, targetScale);

            // Доп. объекты
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

        // Текущее состояние активности
        bool isCurrentlyActive = objectActivationState.ContainsKey(obj) ?
                                objectActivationState[obj] : obj.activeSelf;

        // Текущий масштаб
        float currentScale = currentScaleFactor.ContainsKey(obj) ? currentScaleFactor[obj] : 0f;

        // Новый масштаб по плавной интерполяции
        float newScale = Mathf.Lerp(currentScale, targetScale, Time.deltaTime * animationSpeed);
        currentScaleFactor[obj] = newScale;

        // Включение/выключение по порогам
        if (!isCurrentlyActive && newScale > ACTIVATION_THRESHOLD)
        {
            // Включаю когда достаточно распахнулся
            obj.SetActive(true);
            objectActivationState[obj] = true;

            if (debugMode)
                Debug.Log("Включил объект: " + obj.name + " (scale: " + newScale + ")");
        }
        else if (isCurrentlyActive && newScale < DEACTIVATION_THRESHOLD)
        {
            // Выключаю когда почти схлопнулся
            obj.SetActive(false);
            objectActivationState[obj] = false;

            if (debugMode)
                Debug.Log("Выключил объект: " + obj.name + " (scale: " + newScale + ")");
        }

        // Обновляю визуал, если объект включён
        if (obj.activeSelf)
        {
            // Поддерживаю яркость света
            Light light = obj.GetComponent<Light>();
            if (light != null)
            {
                // Масштабирую интенсивность
                float originalIntensity = originalLightIntensity.ContainsKey(obj) ?
                                         originalLightIntensity[obj] : 1.0f;
                light.intensity = originalIntensity * newScale;
            }
            else
            {
                // Масштаб для обычных объектов
                Vector3 origScale = originalScale.ContainsKey(obj) ? originalScale[obj] : Vector3.one;
                obj.transform.localScale = origScale * newScale;
            }
        }
    }

    // ������������ � ���������
    void OnDrawGizmosSelected()
    {
        foreach (var item in sculpturePanels)
        {
            if (item.sculpture != null)
            {
                // Радиус активации
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(item.sculpture.position, item.activationDistance);

#if UNITY_EDITOR
                UnityEditor.Handles.color = Color.white;
                UnityEditor.Handles.Label(item.sculpture.position + Vector3.up * 0.5f,
                    item.sculpture.name + " (радиус: " + item.activationDistance + "м)");
#endif
            }
        }
    }
}