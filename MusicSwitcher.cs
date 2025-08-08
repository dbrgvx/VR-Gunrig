using UnityEngine;
using System.Collections;

public class AudioSwitcherOptimized : MonoBehaviour
{
    public AudioSource audioSource; // Один единственный AudioSource
    public AudioClip firstClip;     // Первый клип
    public AudioClip loopingClip;   // Зацикленный клип

    public float fadeDuration = 2.0f;

    void Start()
    {
      
        // Проверяем, что у нас есть AudioSource
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogError("[AudioSwitcherOptimized] AudioSource не найден!");
                enabled = false;
                return;
            }
        }

        // Начинаем с первого клипа
        if (firstClip != null)
        {
            audioSource.clip = firstClip;
            audioSource.loop = false;
            audioSource.volume = 1.0f;
            audioSource.Play();

            // Планируем переключение
            StartCoroutine(SwitchToLoopingAfterDelay(firstClip.length * 0.95f)); // Переключаемся чуть раньше конца
        }
        else if (loopingClip != null)
        {
            // Если первого клипа нет, сразу запускаем зацикленный
            audioSource.clip = loopingClip;
            audioSource.loop = true;
            audioSource.volume = 1.0f;
            audioSource.Play();
        }
    }

    IEnumerator SwitchToLoopingAfterDelay(float delay)
    {
        // Просто ждем указанное время
        yield return new WaitForSeconds(delay);

        // Плавно уменьшаем громкость
        float startVolume = audioSource.volume;
        float elapsedTime = 0;

        while (elapsedTime < fadeDuration)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Остановка первого клипа и явная пауза
        audioSource.Stop();
        audioSource.clip = null; // Освобождаем ссылку на клип

        yield return new WaitForSeconds(0.1f); // Явная пауза

        // Переключаемся на зацикленный клип (без проверок)
        if (loopingClip != null)
        {
            // Выполняем только в LateUpdate
            yield return new WaitForEndOfFrame();

            audioSource.clip = loopingClip;
            audioSource.loop = true;
            audioSource.volume = 0; // Начинаем с тишины
            audioSource.Play();

            // Явная пауза перед началом нарастания громкости
            yield return new WaitForSeconds(0.1f);

            // Плавно увеличиваем громкость
            elapsedTime = 0;

            while (elapsedTime < fadeDuration)
            {
                audioSource.volume = Mathf.Lerp(0, 1.0f, elapsedTime / fadeDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            audioSource.volume = 1.0f; // Убеждаемся, что громкость стала 1
        }
    }
}