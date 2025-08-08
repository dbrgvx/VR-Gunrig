using UnityEngine;
using System.Collections;

public class AudioSwitcherOptimized : MonoBehaviour
{
    public AudioSource audioSource; // ���� ������������ AudioSource
    public AudioClip firstClip;     // ������ ����
    public AudioClip loopingClip;   // ����������� ����

    public float fadeDuration = 2.0f;

    void Start()
    {
      
        // ���������, ��� � ��� ���� AudioSource
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogError("[AudioSwitcherOptimized] AudioSource �� ������!");
                enabled = false;
                return;
            }
        }

        // �������� � ������� �����
        if (firstClip != null)
        {
            audioSource.clip = firstClip;
            audioSource.loop = false;
            audioSource.volume = 1.0f;
            audioSource.Play();

            // ��������� ������������
            StartCoroutine(SwitchToLoopingAfterDelay(firstClip.length * 0.95f)); // ������������� ���� ������ �����
        }
        else if (loopingClip != null)
        {
            // ���� ������� ����� ���, ����� ��������� �����������
            audioSource.clip = loopingClip;
            audioSource.loop = true;
            audioSource.volume = 1.0f;
            audioSource.Play();
        }
    }

    IEnumerator SwitchToLoopingAfterDelay(float delay)
    {
        // ������ ���� ��������� �����
        yield return new WaitForSeconds(delay);

        // ������ ��������� ���������
        float startVolume = audioSource.volume;
        float elapsedTime = 0;

        while (elapsedTime < fadeDuration)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // ��������� ������� ����� � ����� �����
        audioSource.Stop();
        audioSource.clip = null; // ����������� ������ �� ����

        yield return new WaitForSeconds(0.1f); // ����� �����

        // ������������� �� ����������� ���� (��� ��������)
        if (loopingClip != null)
        {
            // ��������� ������ � LateUpdate
            yield return new WaitForEndOfFrame();

            audioSource.clip = loopingClip;
            audioSource.loop = true;
            audioSource.volume = 0; // �������� � ������
            audioSource.Play();

            // ����� ����� ����� ������� ���������� ���������
            yield return new WaitForSeconds(0.1f);

            // ������ ����������� ���������
            elapsedTime = 0;

            while (elapsedTime < fadeDuration)
            {
                audioSource.volume = Mathf.Lerp(0, 1.0f, elapsedTime / fadeDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            audioSource.volume = 1.0f; // ����������, ��� ��������� ����� 1
        }
    }
}