using UnityEngine;
using System.Collections.Generic;

public class SculptureInfoPanel : MonoBehaviour
{
    [Tooltip("������ ������ (��� �������� ����������)")]
    public Transform player;

    [Tooltip("�������� �������� ��������� ��������")]
    public float animationSpeed = 5.0f;

    [Tooltip("��������� ���������� � ���������� (�������)")]
    public float checkInterval = 0.1f;

    [Tooltip("�������� ����� ����������� (�������)")]
    public float deactivationDelay = 2.0f;

    [Tooltip("�������� �������")]
    public bool debugMode = false;

    [System.Serializable]
    public class SculpturePanel
    {
        public Transform sculpture; // ����������
        public GameObject infoPanel; // �������������� ������
        [Tooltip("������ ��������� (� ������)")]
        public float activationDistance = 2.5f;
        [HideInInspector]
        public bool isActive = false;
        [HideInInspector]
        public bool isInRadius = false;
        [HideInInspector]
        public float deactivationTimer = 0f;
        [Tooltip("�������������� �������, ������� ����� �������������� (����, ������� � ��.)")]
        public List<GameObject> additionalObjects = new List<GameObject>();
    }

    [Header("���������� � ��������� �������")]
    public List<SculpturePanel> sculpturePanels = new List<SculpturePanel>();

    private float timer = 0f;
    private Dictionary<GameObject, Vector3> originalScale = new Dictionary<GameObject, Vector3>();
    private Dictionary<GameObject, float> currentScaleFactor = new Dictionary<GameObject, float>();
    private Dictionary<GameObject, bool> objectActivationState = new Dictionary<GameObject, bool>();
    private Dictionary<GameObject, float> originalLightIntensity = new Dictionary<GameObject, float>();

    // ��������� �������� ��� ��������
    private const float ACTIVATION_THRESHOLD = 0.05f;
    private const float DEACTIVATION_THRESHOLD = 0.01f;

    void Start()
    {
        if (player == null)
        {
            Debug.LogError("�� �������� ������ player!");
            enabled = false;
            return;
        }

        // ������������� ���� ��������
        foreach (var item in sculpturePanels)
        {
            InitializeItem(item);
        }

        // ���������� ��������� ���������� �� ��������� ��� ������
        CheckSculptureProximity();

        // ��������� ��������� �������� �� ������ ��������� ��������
        UpdateObjectVisibility();

        if (debugMode)
            Debug.Log("SculptureInfoPanel: ������������� ���������. ������� " + sculpturePanels.Count + " ���������.");
    }

    void InitializeItem(SculpturePanel item)
    {
        // ��������� ������
        if (item.infoPanel != null)
        {
            // ��������� ������������ �������
            originalScale[item.infoPanel] = item.infoPanel.transform.localScale;
            currentScaleFactor[item.infoPanel] = 0f;
            objectActivationState[item.infoPanel] = false;

            // �������� ������
            item.infoPanel.transform.localScale = Vector3.zero;
            item.infoPanel.SetActive(false);
        }

        // ��������� �������������� ��������
        foreach (var obj in item.additionalObjects)
        {
            if (obj != null)
            {
                // ���������� �������� ��������� ���������
                objectActivationState[obj] = false;

                // ��������� ������������ �������
                originalScale[obj] = obj.transform.localScale;
                currentScaleFactor[obj] = 0f;

                // ��� ���������� ����� ���������� �������� �������������
                Light light = obj.GetComponent<Light>();
                if (light != null)
                {
                    originalLightIntensity[obj] = light.intensity;
                    light.intensity = 0f;
                }

                // ���������� �������� ������
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
        // ��������� ���������� � ����������
        timer += Time.deltaTime;
        if (timer >= checkInterval)
        {
            CheckSculptureProximity();
            timer = 0f;
        }

        // ��������� �������� �����������
        ProcessDeactivationTimers();

        // ��������� ��������� ��������
        UpdateObjectVisibility();
    }

    void CheckSculptureProximity()
    {
        foreach (var item in sculpturePanels)
        {
            if (item.sculpture == null) continue;

            // ��������� ���������� �� ����������
            float distance = Vector3.Distance(player.position, item.sculpture.position);

            // ��������� ���������� ���������
            bool wasInRadius = item.isInRadius;

            // ��������� ��������� ���������� � �������
            item.isInRadius = distance <= item.activationDistance;

            // ���� ����� � ������
            if (!wasInRadius && item.isInRadius)
            {
                item.isActive = true;
                item.deactivationTimer = 0f; // ���������� ������

                if (debugMode)
                    Debug.Log("����� ����� � ������ ����������: " + item.sculpture.name);
            }
            // ���� ����� �� �������
            else if (wasInRadius && !item.isInRadius)
            {
                // ��������� ������ �����������, �� ���� �� ������������
                item.deactivationTimer = deactivationDelay;

                if (debugMode)
                    Debug.Log("����� ����� �� ������� ����������: " + item.sculpture.name + ". ������ ������� �������.");
            }
        }
    }

    void ProcessDeactivationTimers()
    {
        foreach (var item in sculpturePanels)
        {
            // ���� ������� ������ �����������
            if (item.deactivationTimer > 0)
            {
                // ��������� ������
                item.deactivationTimer -= Time.deltaTime;

                // ���� ������ �����
                if (item.deactivationTimer <= 0)
                {
                    item.isActive = false;

                    if (debugMode)
                        Debug.Log("������ �����, ����������� �������� ��� ����������: " + item.sculpture.name);
                }
            }
        }
    }

    void UpdateObjectVisibility()
    {
        foreach (var item in sculpturePanels)
        {
            float targetScale = item.isActive ? 1f : 0f;

            // ��������� ������
            AnimateObject(item.infoPanel, targetScale);

            // ��������� ��� �������������� �������
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

        // ��������� ������� ��������� ���������
        bool isCurrentlyActive = objectActivationState.ContainsKey(obj) ?
                                objectActivationState[obj] : obj.activeSelf;

        // �������� ������� �������
        float currentScale = currentScaleFactor.ContainsKey(obj) ? currentScaleFactor[obj] : 0f;

        // ���������� �������� �������
        float newScale = Mathf.Lerp(currentScale, targetScale, Time.deltaTime * animationSpeed);
        currentScaleFactor[obj] = newScale;

        // ������ ��������� � ������������
        if (!isCurrentlyActive && newScale > ACTIVATION_THRESHOLD)
        {
            // ���������� ������ ������ ����� ������� ���������� �������
            obj.SetActive(true);
            objectActivationState[obj] = true;

            if (debugMode)
                Debug.Log("��������� �������: " + obj.name + " (scale: " + newScale + ")");
        }
        else if (isCurrentlyActive && newScale < DEACTIVATION_THRESHOLD)
        {
            // ������������ ������ ������ ����� ������� ����� �������
            obj.SetActive(false);
            objectActivationState[obj] = false;

            if (debugMode)
                Debug.Log("����������� �������: " + obj.name + " (scale: " + newScale + ")");
        }

        // ��������� ������� � �������, ���� �� �������
        if (obj.activeSelf)
        {
            // ��������� ����� ������������ ��������
            Light light = obj.GetComponent<Light>();
            if (light != null)
            {
                // ��� ����� ���������� ������������� ������ ��������
                float originalIntensity = originalLightIntensity.ContainsKey(obj) ?
                                         originalLightIntensity[obj] : 1.0f;
                light.intensity = originalIntensity * newScale;
            }
            else
            {
                // ��� ��������� �������� �������� �������
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
                // ������ ����� ������� ���������
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(item.sculpture.position, item.activationDistance);

#if UNITY_EDITOR
                UnityEditor.Handles.color = Color.white;
                UnityEditor.Handles.Label(item.sculpture.position + Vector3.up * 0.5f,
                    item.sculpture.name + " (������: " + item.activationDistance + "�)");
#endif
            }
        }
    }
}