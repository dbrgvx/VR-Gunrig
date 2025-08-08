using System.Collections.Generic;
using UnityEngine;

public class XButtonToggleObjects : MonoBehaviour
{
    [Header("��������� �����")]
    [Tooltip("������� ��� ������������")]
    public bool alreadyUsed = false;
    [SerializeField] private float buttonCooldown = 0.5f;

    [Header("������� ��� ���������")]
    [Tooltip("�������, ������� ����� �������� ��� ������� X")]
    public List<GameObject> objectsToEnable = new List<GameObject>();

    [Header("������� ��� �����������")]
    [Tooltip("�������, ������� ����� ��������� ��� ������� X")]
    public List<GameObject> objectsToDisable = new List<GameObject>();

    [Header("�������")]
    [Tooltip("����� ���������� ��������� � �������")]
    public bool showDebug = true;

    private bool canPress = true;
    private float cooldownTimer = 0f;

    void Update()
    {
        // ���� ������� ��� ������������, �������
        if (alreadyUsed)
            return;

        // ������� ��� ������
        if (!canPress)
        {
            cooldownTimer -= Time.unscaledDeltaTime;
            if (cooldownTimer <= 0)
            {
                canPress = true;
            }
        }

        // �������� ������� ������ X �� ����� �����������
        if (canPress && OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.LTouch))
        {
            // ��������� �������
            foreach (var obj in objectsToDisable)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                    if (showDebug) Debug.Log($"��������: {obj.name}");
                }
            }

            // �������� �������
            foreach (var obj in objectsToEnable)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                    if (showDebug) Debug.Log($"�������: {obj.name}");
                }
            }

            // ��������, ��� ������� ��� ������������
            alreadyUsed = true;
            canPress = false;
            cooldownTimer = buttonCooldown;

            if (showDebug) Debug.Log("������ X ������ - ������� �����������");
        }
    }

    // ����� ��� ������������ ������
    public void ResetToggle()
    {
        alreadyUsed = false;
        canPress = true;
        if (showDebug) Debug.Log("����� ��������� �������������");
    }
}