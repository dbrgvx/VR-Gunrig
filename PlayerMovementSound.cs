using UnityEngine;
using System.Collections; // Необходимо для корутин

public class PlayerMovementSound : MonoBehaviour
{
    [Header("Настройки звука")]
    [SerializeField] private AudioClip movementSoundClip;
    [SerializeField][Range(0f, 1f)] private float targetVolume = 0.5f; // Целевая громкость при движении
    [SerializeField] private bool loopSound = true;
    [SerializeField] private float minMovementSpeed = 0.1f; // Порог скорости для активации звука
    [SerializeField] private float fadeDuration = 0.5f; // Длительность плавного перехода

    private AudioSource audioSource;
    private Vector3 lastPosition;
    private float movementSpeed;
    private Coroutine fadeCoroutine = null; // Хранит ссылку на активную корутину затухания/появления
    private bool shouldBePlaying = false; // Флаг, должен ли звук проигрываться

    void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.clip = movementSoundClip;
        audioSource.loop = loopSound;
        audioSource.volume = 0f; // Начинаем с нулевой громкости
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        lastPosition = transform.position;
    }

    void Update()
    {
        // Рассчитываем скорость движения
        movementSpeed = Vector3.Distance(transform.position, lastPosition) / Time.deltaTime;
        lastPosition = transform.position;

        // Определяем, должен ли звук играть
        bool currentShouldBePlaying = movementSpeed > minMovementSpeed;

        // Если состояние изменилось (начали или закончили двигаться)
        if (currentShouldBePlaying != shouldBePlaying)
        {
            shouldBePlaying = currentShouldBePlaying;

            // Останавливаем предыдущую корутину, если она была
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            // Запускаем новую корутину для плавного перехода
            if (shouldBePlaying)
            {
                // Если звук еще не играет, запускаем
                if (!audioSource.isPlaying)
                {
                    audioSource.Play();
                }
                fadeCoroutine = StartCoroutine(FadeAudio(targetVolume, fadeDuration));
            }
            else
            {
                fadeCoroutine = StartCoroutine(FadeAudio(0f, fadeDuration));
            }
        }
    }

    // Корутина для плавного изменения громкости
    IEnumerator FadeAudio(float targetVol, float duration)
    {
        float startVolume = audioSource.volume;
        float time = 0;

        while (time < duration)
        {
            // Изменяем громкость пропорционально времени
            audioSource.volume = Mathf.Lerp(startVolume, targetVol, time / duration);
            time += Time.deltaTime;
            yield return null; // Ждем следующего кадра
        }

        // Устанавливаем точное конечное значение
        audioSource.volume = targetVol;

        // Если громкость стала нулевой, останавливаем проигрывание
        if (targetVol == 0f)
        {
            audioSource.Pause(); // Используем Pause вместо Stop, чтобы сохранить позицию
        }

        fadeCoroutine = null; // Сбрасываем ссылку на корутину
    }
}