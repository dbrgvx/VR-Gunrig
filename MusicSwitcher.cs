using UnityEngine;
using System.Collections;

public class AudioSwitcherOptimized : MonoBehaviour
{
    public AudioSource audioSource; // Источник звука
    public AudioClip firstClip;     // Первый трек
    public AudioClip loopingClip;   // Цикличный трек

    public float fadeDuration = 2.0f;

    void Start()
    {
      
        // Проверяю, что есть AudioSource
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

        // Стартую с первого трека
        if (firstClip != null)
        {
            audioSource.clip = firstClip;
            audioSource.loop = false;
            audioSource.volume = 1.0f;
            audioSource.Play();

            // Почти в конце плавно переключаюсь на луп
            StartCoroutine(SwitchToLoopingAfterDelay(firstClip.length * 0.95f));
        }
        else if (loopingClip != null)
        {
            // Если первого нет — сразу луп
            audioSource.clip = loopingClip;
            audioSource.loop = true;
            audioSource.volume = 1.0f;
            audioSource.Play();
        }
    }

    IEnumerator SwitchToLoopingAfterDelay(float delay)
    {
        // Жду перед переключением
        yield return new WaitForSeconds(delay);

        // Фейд-аут
        float startVolume = audioSource.volume;
        float elapsedTime = 0;

        while (elapsedTime < fadeDuration)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Останавливаю и очищаю
        audioSource.Stop();
        audioSource.clip = null;

        yield return new WaitForSeconds(0.1f);

        // Включаю луп с фейд‑ином
        if (loopingClip != null)
        {
            // Кадр на подготовку
            yield return new WaitForEndOfFrame();

            audioSource.clip = loopingClip;
            audioSource.loop = true;
            audioSource.volume = 0;
            audioSource.Play();

            // Небольшая пауза
            yield return new WaitForSeconds(0.1f);

            // Фейд‑ин
            elapsedTime = 0;

            while (elapsedTime < fadeDuration)
            {
                audioSource.volume = Mathf.Lerp(0, 1.0f, elapsedTime / fadeDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            audioSource.volume = 1.0f;
        }
    }
}