using UnityEngine;
using System.Collections; // ���������� ��� �������

public class PlayerMovementSound : MonoBehaviour
{
    [Header("��������� �����")]
    [SerializeField] private AudioClip movementSoundClip;
    [SerializeField][Range(0f, 1f)] private float targetVolume = 0.5f; // ������� ��������� ��� ��������
    [SerializeField] private bool loopSound = true;
    [SerializeField] private float minMovementSpeed = 0.1f; // ����� �������� ��� ��������� �����
    [SerializeField] private float fadeDuration = 0.5f; // ������������ �������� ��������

    private AudioSource audioSource;
    private Vector3 lastPosition;
    private float movementSpeed;
    private Coroutine fadeCoroutine = null; // ������ ������ �� �������� �������� ���������/���������
    private bool shouldBePlaying = false; // ����, ������ �� ���� �������������

    void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.clip = movementSoundClip;
        audioSource.loop = loopSound;
        audioSource.volume = 0f; // �������� � ������� ���������
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        lastPosition = transform.position;
    }

    void Update()
    {
        // ������������ �������� ��������
        movementSpeed = Vector3.Distance(transform.position, lastPosition) / Time.deltaTime;
        lastPosition = transform.position;

        // ����������, ������ �� ���� ������
        bool currentShouldBePlaying = movementSpeed > minMovementSpeed;

        // ���� ��������� ���������� (������ ��� ��������� ���������)
        if (currentShouldBePlaying != shouldBePlaying)
        {
            shouldBePlaying = currentShouldBePlaying;

            // ������������� ���������� ��������, ���� ��� ����
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            // ��������� ����� �������� ��� �������� ��������
            if (shouldBePlaying)
            {
                // ���� ���� ��� �� ������, ���������
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

    // �������� ��� �������� ��������� ���������
    IEnumerator FadeAudio(float targetVol, float duration)
    {
        float startVolume = audioSource.volume;
        float time = 0;

        while (time < duration)
        {
            // �������� ��������� ��������������� �������
            audioSource.volume = Mathf.Lerp(startVolume, targetVol, time / duration);
            time += Time.deltaTime;
            yield return null; // ���� ���������� �����
        }

        // ������������� ������ �������� ��������
        audioSource.volume = targetVol;

        // ���� ��������� ����� �������, ������������� ������������
        if (targetVol == 0f)
        {
            audioSource.Pause(); // ���������� Pause ������ Stop, ����� ��������� �������
        }

        fadeCoroutine = null; // ���������� ������ �� ��������
    }
}