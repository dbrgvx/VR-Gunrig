using UnityEngine;
using UnityEngine.SceneManagement;
using Meta.XR; // Убедитесь, что это подключено для OVRPlugin и OVRInput

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
    // Эта переменная отслеживает, был ли шлем снят в текущем жизненном цикле экземпляра скрипта
    private bool hmdWasRemovedInThisInstance = false;

    // Статическая переменная для отслеживания перезапуска между загрузками сцен
    private static bool sceneHasRestartedDueToHmdRemoval = false;

    void Awake()
    {
        // Если шлем надет при загрузке сцены, это означает, что предыдущий "сеанс снятия шлема" завершен.
        // Сбрасываем статический флаг, чтобы разрешить новый перезапуск при следующем снятии.
        if (OVRPlugin.userPresent)
        {
            sceneHasRestartedDueToHmdRemoval = false;
        }
    }

    void Update()
    {
        // Кулдаун для кнопки
        if (!canPress)
        {
            cooldownTimer -= Time.unscaledDeltaTime;
            if (cooldownTimer <= 0)
            {
                canPress = true;
            }
        }

        // Проверка нажатия кнопки A на правом контроллере
        if (canPress && OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
        {
            // Если перезапускаем сцену кнопкой, также сбросим статический флаг HMD,
            // чтобы детектор снятия шлема сработал корректно после ручного перезапуска.
            sceneHasRestartedDueToHmdRemoval = false;
            RestartCurrentScene();

            canPress = false;
            cooldownTimer = buttonCooldown;
        }

        // Проверка надет ли шлем
        if (enableHmdDetection)
        {
            bool hmdIsWorn = OVRPlugin.userPresent;

            if (!hmdIsWorn) // Шлем СНЯТ
            {
                // Если шлем только что был зафиксирован как снятый (в этом экземпляре скрипта)
                // И сцена ЕЩЕ НЕ была перезапущена из-за снятия шлема в текущей "сессии без шлема"
                if (!hmdWasRemovedInThisInstance && !sceneHasRestartedDueToHmdRemoval)
                {
                    hmdWasRemovedInThisInstance = true;
                    hmdRemovedTimer = hmdRemovalDelay;
                }
                // Иначе, если шлем продолжает быть снятым, таймер активен,
                // и мы еще не перезапускались из-за снятия шлема в этой "сессии без шлема"
                else if (hmdWasRemovedInThisInstance && !sceneHasRestartedDueToHmdRemoval)
                {
                    hmdRemovedTimer -= Time.unscaledDeltaTime;

                    if (hmdRemovedTimer <= 0)
                    {
                        // Устанавливаем статический флаг ПЕРЕД перезапуском.
                        // Этот флаг сохранится после перезагрузки сцены.
                        sceneHasRestartedDueToHmdRemoval = true;
                        RestartCurrentScene();
                        // Переменные экземпляра hmdWasRemovedInThisInstance и hmdRemovedTimer
                        // сбросятся при загрузке сцены. Статический флаг предотвратит повторный запуск таймера.
                    }
                }
            }
            else // Шлем НАДЕТ
            {
                // Если шлем был ранее помечен как снятый (этим экземпляром скрипта) и теперь надет
                if (hmdWasRemovedInThisInstance)
                {
                    hmdWasRemovedInThisInstance = false; // Сбрасываем флаг экземпляра
                    hmdRemovedTimer = 0f; // Сбрасываем таймер на всякий случай
                }

                // Важно: если шлем надет, любая "сессия без шлема", которая могла вызвать перезапуск, окончена.
                // Поэтому сбрасываем статический флаг. Это также обрабатывается в Awake, но здесь для надежности.
                if (sceneHasRestartedDueToHmdRemoval) // Если флаг был true, а шлем теперь надет
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