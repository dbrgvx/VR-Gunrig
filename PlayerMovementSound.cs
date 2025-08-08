using UnityEngine;
using System.Collections; // Нужен для корутин

public class PlayerMovementSound : MonoBehaviour
{
    [Header("Звук движения")]
    [SerializeField] private AudioClip movementSoundClip;
    [SerializeField][Range(0f, 1f)] private float targetVolume = 0.5f; // Громкость при движении
    [SerializeField] private bool loopSound = true;
    [SerializeField] private float minMovementSpeed = 0.1f; // Порог скорости для запуска звука
    [SerializeField] private float fadeDuration = 0.5f; // Длительность фейда

    private AudioSource audioSource;
    private Vector3 lastPosition;
    private float movementSpeed;
    private Coroutine fadeCoroutine = null; // Текущая корутина фейда
    private bool shouldBePlaying = false; // Должен ли звук играть

    void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.clip = movementSoundClip;
        audioSource.loop = loopSound;
        audioSource.volume = 0f; // Стартую с нуля
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        lastPosition = transform.position;
    }

    void Update()
    {
        // Считаю скорость
        movementSpeed = Vector3.Distance(transform.position, lastPosition) / Time.deltaTime;
        lastPosition = transform.position;

        // Проверяю, нужно ли играть звук
        bool currentShouldBePlaying = movementSpeed > minMovementSpeed;

        // Если состояние изменилось — переключаю фейд
        if (currentShouldBePlaying != shouldBePlaying)
        {
            shouldBePlaying = currentShouldBePlaying;

            // Останавливаю предыдущий фейд
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            // Включаю/выключаю звук с фейдом
            if (shouldBePlaying)
            {
                // Если не играет — запускаю
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

    // Фейд громкости звука
    IEnumerator FadeAudio(float targetVol, float duration)
    {
        float startVolume = audioSource.volume;
        float time = 0;

        while (time < duration)
        {
            // Плавно меняю громкость
            audioSource.volume = Mathf.Lerp(startVolume, targetVol, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        // Фиксирую итоговую громкость
        audioSource.volume = targetVol;

        // Если ушли в ноль — ставлю на паузу
        if (targetVol == 0f)
        {
            audioSource.Pause();
        }

        fadeCoroutine = null;
    }
}