using System.Collections.Generic;
using UnityEngine;

public class XButtonToggleObjects : MonoBehaviour
{
    [Header("Состояние кнопки")]
    [Tooltip("Одноразовый режим")]
    public bool alreadyUsed = false;
    [SerializeField] private float buttonCooldown = 0.5f;

    [Header("Включить объекты")]
    [Tooltip("Список, который включаем по X")]
    public List<GameObject> objectsToEnable = new List<GameObject>();

    [Header("Выключить объекты")]
    [Tooltip("Список, который выключаем по X")]
    public List<GameObject> objectsToDisable = new List<GameObject>();

    [Header("Отладка")]
    [Tooltip("Печатать логи в консоль")]
    public bool showDebug = true;

    private bool canPress = true;
    private float cooldownTimer = 0f;

    void Update()
    {
        // Если уже отработали — выходим
        if (alreadyUsed)
            return;

        // Кулдаун
        if (!canPress)
        {
            cooldownTimer -= Time.unscaledDeltaTime;
            if (cooldownTimer <= 0)
            {
                canPress = true;
            }
        }

        // Кнопка X на левом контроллере
        if (canPress && OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.LTouch))
        {
            // Выключаю список
            foreach (var obj in objectsToDisable)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                    if (showDebug) Debug.Log($"Выключен: {obj.name}");
                }
            }

            // Включаю список
            foreach (var obj in objectsToEnable)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                    if (showDebug) Debug.Log($"Включен: {obj.name}");
                }
            }

            // Одноразовый режим
            alreadyUsed = true;
            canPress = false;
            cooldownTimer = buttonCooldown;

            if (showDebug) Debug.Log("Нажатие X — переключение выполнено");
        }
    }

    // Сброс одноразового режима
    public void ResetToggle()
    {
        alreadyUsed = false;
        canPress = true;
        if (showDebug) Debug.Log("Сброс переключателя выполнен");
    }
}