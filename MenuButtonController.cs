using UnityEngine;
using UnityEngine.SceneManagement;
using Meta.XR; // Использую OVRPlugin и OVRInput

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
    // Чтобы не перезапускать сцену многократно за один сеанс
    private bool hmdWasRemovedInThisInstance = false;

    // Флаг, что сцена уже перезапускалась из‑за снятия шлема
    private static bool sceneHasRestartedDueToHmdRemoval = false;

    void Awake()
    {
        // При старте проверяю, надет ли шлем, и сбрасываю флаг перезапуска
        if (OVRPlugin.userPresent)
        {
            sceneHasRestartedDueToHmdRemoval = false;
        }
    }

    void Update()
    {
        // Кулдаун на кнопку
        if (!canPress)
        {
            cooldownTimer -= Time.unscaledDeltaTime;
            if (cooldownTimer <= 0)
            {
                canPress = true;
            }
        }

        // Кнопка A на правом контроллере
        if (canPress && OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
        {
            // Сбрасываю флаг, если перезапуск делаю вручную
            sceneHasRestartedDueToHmdRemoval = false;
            RestartCurrentScene();

            canPress = false;
            cooldownTimer = buttonCooldown;
        }

        // Отслеживаю снятие шлема
        if (enableHmdDetection)
        {
            bool hmdIsWorn = OVRPlugin.userPresent;

            if (!hmdIsWorn) // шлем снят
            {
                // Первое снятие за сессию — стартую таймер
                if (!hmdWasRemovedInThisInstance && !sceneHasRestartedDueToHmdRemoval)
                {
                    hmdWasRemovedInThisInstance = true;
                    hmdRemovedTimer = hmdRemovalDelay;
                }
                // Тикаю таймер, пока не перезапустили сцену
                else if (hmdWasRemovedInThisInstance && !sceneHasRestartedDueToHmdRemoval)
                {
                    hmdRemovedTimer -= Time.unscaledDeltaTime;

                    if (hmdRemovedTimer <= 0)
                    {
                        // Перезапускаю сцену после задержки
                        sceneHasRestartedDueToHmdRemoval = true;
                        RestartCurrentScene();
                    }
                }
            }
            else // шлем надет
            {
                // Сбрасываю локальные флаги при возврате шлема
                if (hmdWasRemovedInThisInstance)
                {
                    hmdWasRemovedInThisInstance = false;
                    hmdRemovedTimer = 0f;
                }

                // Если сцена уже перезапускалась — снимаю флаг
                if (sceneHasRestartedDueToHmdRemoval)
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