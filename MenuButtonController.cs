using UnityEngine;
using UnityEngine.SceneManagement;
using Meta.XR; // ���������, ��� ��� ���������� ��� OVRPlugin � OVRInput

public class MenuButtonController : MonoBehaviour
{
    [Header("Button Settings")]
    [SerializeField] private float buttonCooldown = 0.5f;

    [Header("HMD Detection")]
    [SerializeField] private float hmdRemovalDelay = 5.0f;
    [SerializeField] private bool enableHmdDetection = true;

    private bool canPress = true;
    private float cooldownTimer = 0f;
    private float hmdRemovedTimer = 0f;
    // ��� ���������� �����������, ��� �� ���� ���� � ������� ��������� ����� ���������� �������
    private bool hmdWasRemovedInThisInstance = false;

    // ����������� ���������� ��� ������������ ����������� ����� ���������� ����
    private static bool sceneHasRestartedDueToHmdRemoval = false;

    void Awake()
    {
        // ���� ���� ����� ��� �������� �����, ��� ��������, ��� ���������� "����� ������ �����" ��������.
        // ���������� ����������� ����, ����� ��������� ����� ���������� ��� ��������� ������.
        if (OVRPlugin.userPresent)
        {
            sceneHasRestartedDueToHmdRemoval = false;
        }
    }

    void Update()
    {
        // ������� ��� ������
        if (!canPress)
        {
            cooldownTimer -= Time.unscaledDeltaTime;
            if (cooldownTimer <= 0)
            {
                canPress = true;
            }
        }

        // �������� ������� ������ A �� ������ �����������
        if (canPress && OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
        {
            // ���� ������������� ����� �������, ����� ������� ����������� ���� HMD,
            // ����� �������� ������ ����� �������� ��������� ����� ������� �����������.
            sceneHasRestartedDueToHmdRemoval = false;
            RestartCurrentScene();

            canPress = false;
            cooldownTimer = buttonCooldown;
        }

        // �������� ����� �� ����
        if (enableHmdDetection)
        {
            bool hmdIsWorn = OVRPlugin.userPresent;

            if (!hmdIsWorn) // ���� ����
            {
                // ���� ���� ������ ��� ��� ������������ ��� ������ (� ���� ���������� �������)
                // � ����� ��� �� ���� ������������ ��-�� ������ ����� � ������� "������ ��� �����"
                if (!hmdWasRemovedInThisInstance && !sceneHasRestartedDueToHmdRemoval)
                {
                    hmdWasRemovedInThisInstance = true;
                    hmdRemovedTimer = hmdRemovalDelay;
                }
                // �����, ���� ���� ���������� ���� ������, ������ �������,
                // � �� ��� �� ��������������� ��-�� ������ ����� � ���� "������ ��� �����"
                else if (hmdWasRemovedInThisInstance && !sceneHasRestartedDueToHmdRemoval)
                {
                    hmdRemovedTimer -= Time.unscaledDeltaTime;

                    if (hmdRemovedTimer <= 0)
                    {
                        // ������������� ����������� ���� ����� ������������.
                        // ���� ���� ���������� ����� ������������ �����.
                        sceneHasRestartedDueToHmdRemoval = true;
                        RestartCurrentScene();
                        // ���������� ���������� hmdWasRemovedInThisInstance � hmdRemovedTimer
                        // ��������� ��� �������� �����. ����������� ���� ������������ ��������� ������ �������.
                    }
                }
            }
            else // ���� �����
            {
                // ���� ���� ��� ����� ������� ��� ������ (���� ����������� �������) � ������ �����
                if (hmdWasRemovedInThisInstance)
                {
                    hmdWasRemovedInThisInstance = false; // ���������� ���� ����������
                    hmdRemovedTimer = 0f; // ���������� ������ �� ������ ������
                }

                // �����: ���� ���� �����, ����� "������ ��� �����", ������� ����� ������� ����������, ��������.
                // ������� ���������� ����������� ����. ��� ����� �������������� � Awake, �� ����� ��� ����������.
                if (sceneHasRestartedDueToHmdRemoval) // ���� ���� ��� true, � ���� ������ �����
                {
                    sceneHasRestartedDueToHmdRemoval = false;
                }
            }
        }
    }

    private void RestartCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}