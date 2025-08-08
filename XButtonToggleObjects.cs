using System.Collections.Generic;
using UnityEngine;

public class XButtonToggleObjects : MonoBehaviour
{
    [Header("Настройка ввода")]
    [Tooltip("Нажатие уже использовано")]
    public bool alreadyUsed = false;
    [SerializeField] private float buttonCooldown = 0.5f;

    [Header("Объекты для активации")]
    [Tooltip("Объекты, которые нужно ВКЛЮЧИТЬ при нажатии X")]
    public List<GameObject> objectsToEnable = new List<GameObject>();

    [Header("Объекты для деактивации")]
    [Tooltip("Объекты, которые нужно ВЫКЛЮЧИТЬ при нажатии X")]
    public List<GameObject> objectsToDisable = new List<GameObject>();

    [Header("Отладка")]
    [Tooltip("Вывод отладочных сообщений в консоль")]
    public bool showDebug = true;

    private bool canPress = true;
    private float cooldownTimer = 0f;

    void Update()
    {
        // Если функция уже использована, выходим
        if (alreadyUsed)
            return;

        // Кулдаун для кнопки
        if (!canPress)
        {
            cooldownTimer -= Time.unscaledDeltaTime;
            if (cooldownTimer <= 0)
            {
                canPress = true;
            }
        }

        // Проверка нажатия кнопки X на левом контроллере
        if (canPress && OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.LTouch))
        {
            // Выключаем объекты
            foreach (var obj in objectsToDisable)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                    if (showDebug) Debug.Log($"Выключен: {obj.name}");
                }
            }

            // Включаем объекты
            foreach (var obj in objectsToEnable)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                    if (showDebug) Debug.Log($"Включен: {obj.name}");
                }
            }

            // Отмечаем, что функция уже использована
            alreadyUsed = true;
            canPress = false;
            cooldownTimer = buttonCooldown;

            if (showDebug) Debug.Log("Кнопка X нажата - объекты переключены");
        }
    }

    // Метод для программного сброса
    public void ResetToggle()
    {
        alreadyUsed = false;
        canPress = true;
        if (showDebug) Debug.Log("Сброс состояния переключателя");
    }
}