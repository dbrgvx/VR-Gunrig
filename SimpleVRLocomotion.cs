using UnityEngine;

public class SimpleVRLocomotion : MonoBehaviour
{
    [Header("Ссылки")]
    public OVRCameraRig cameraRig;
    public float moveSpeed = 2.0f;

    [Header("Гравитация и высота")]
    public float normalGravity = -9.81f;
    public float initialGravity = -0.01f;
    public float groundCheckDistance = 0.3f; // Дистанция луча до земли
    public float terrainFollowSpeed = 5.0f;
    public float terrainOffsetY = 1.0f;

    [Header("Параметры поверхности")]
    public string targetTerrainTag = "Terrain";
    public LayerMask terrainLayerMask;
    public bool showDebugRays = true; // Рисовать лучи

    [Header("Ограничения по коллайдерам")]
    public bool preventClimbingSphereColliders = true;

    private CharacterController characterController;
    private float verticalVelocity;
    private bool isGrounded;
    private bool hasLandedOnTerrain = false;
    private float currentGravity;
    private bool autoUpdateHeight = true;

    // Смещения для мульти‑рейкаста
    private Vector3[] raycastOffsets = new Vector3[] {
        Vector3.zero,                          // центр
        new Vector3(0.2f, 0, 0.2f),           // вперёд‑право
        new Vector3(-0.2f, 0, 0.2f),          // вперёд‑лево
        new Vector3(0.2f, 0, -0.2f),          // назад‑право
        new Vector3(-0.2f, 0, -0.2f)          // назад‑лево
    };

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            characterController = gameObject.AddComponent<CharacterController>();
            characterController.height = 4f;
            characterController.radius = 0.3f;
            characterController.center = new Vector3(0, 1f, 0);
        }

        if (cameraRig == null)
        {
            cameraRig = FindObjectOfType<OVRCameraRig>();
            if (cameraRig == null)
                Debug.LogError("OVRCameraRig не найден!");
        }

        // Сброс состояния при старте сцены
        currentGravity = initialGravity;
        hasLandedOnTerrain = false;

        characterController.minMoveDistance = 0.001f;
    }

    void Update()
    {
        MultiRaycastGroundCheck();
        HandleMovement();
        ApplyGravity();

        if (autoUpdateHeight && isGrounded && hasLandedOnTerrain)
        {
            UpdateHeightBasedOnTerrain();
        }
    }

    // Проверка земли несколькими лучами
    void MultiRaycastGroundCheck()
    {
        isGrounded = false;
        RaycastHit hit;

        foreach (Vector3 offset in raycastOffsets)
        {
            Vector3 rayOrigin = transform.position + offset + Vector3.up * 0.1f;

            if (Physics.Raycast(rayOrigin, Vector3.down, out hit, groundCheckDistance + 0.2f))
            {
                isGrounded = true;

                // Отрисовка лучей
                if (showDebugRays)
                {
                    Debug.DrawRay(rayOrigin, Vector3.down * (groundCheckDistance + 0.2f), Color.green, 0.1f);
                }

                // Первый касание поверхности
                if (!hasLandedOnTerrain)
                {
                    if (hit.collider.CompareTag(targetTerrainTag) ||
                        ((1 << hit.collider.gameObject.layer) & terrainLayerMask) != 0)
                    {
                        hasLandedOnTerrain = true;
                        currentGravity = normalGravity;
                        Debug.Log("Приземлились на основную поверхность. Включаю нормальную гравитацию.");
                    }
                }

                // Дальше лучи не нужны
                break;
            }
            else if (showDebugRays)
            {
                Debug.DrawRay(rayOrigin, Vector3.down * (groundCheckDistance + 0.2f), Color.red, 0.1f);
            }
        }
    }

    void HandleMovement()
    {
        Vector2 leftStickInput = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        Vector2 rightStickInput = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
        Vector2 input = (leftStickInput.magnitude > rightStickInput.magnitude) ? leftStickInput : rightStickInput;

        Vector3 headDirection = cameraRig.centerEyeAnchor.transform.forward;
        headDirection.y = 0;
        headDirection.Normalize();

        Vector3 rightDirection = cameraRig.centerEyeAnchor.transform.right;
        rightDirection.y = 0;
        rightDirection.Normalize();

        Vector3 movement = (headDirection * input.y + rightDirection * input.x) * moveSpeed;
        movement.y = verticalVelocity;

        characterController.Move(movement * Time.deltaTime);
    }

    void ApplyGravity()
    {
        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += currentGravity * Time.deltaTime;
        }
    }

    void UpdateHeightBasedOnTerrain()
    {
        RaycastHit hit;
        if (Physics.Raycast(
            transform.position + Vector3.up * 10f,
            Vector3.down,
            out hit,
            100f,
            terrainLayerMask))
        {
            Vector3 targetPosition = transform.position;
            targetPosition.y = hit.point.y + terrainOffsetY + characterController.height / 2;

            Vector3 newPosition = Vector3.Lerp(
                transform.position,
                targetPosition,
                Time.deltaTime * terrainFollowSpeed
            );

            Vector3 moveVector = newPosition - transform.position;
            characterController.Move(new Vector3(0, moveVector.y, 0));
        }
    }

    // ��������� ��������� ������������ ��� ��������� ����������� ������� ��������
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!hasLandedOnTerrain)
        {
            if (hit.collider.CompareTag(targetTerrainTag) ||
                ((1 << hit.collider.gameObject.layer) & terrainLayerMask) != 0)
            {
                hasLandedOnTerrain = true;
                currentGravity = normalGravity;
                Debug.Log("Физика столкновения: Игрок приземлился на базовую поверхность.");
            }
        }
        
        // Предотвращение подъема на SphereCollider
        if (preventClimbingSphereColliders && hit.collider is SphereCollider)
        {
            // Получаем вектор направления от центра сферы к игроку
            Vector3 directionFromSphere = (transform.position - hit.collider.transform.position).normalized;
            // Устанавливаем позицию игрока чуть дальше от сферы
            Vector3 pushDirection = new Vector3(directionFromSphere.x, 0, directionFromSphere.z).normalized;
            characterController.Move(pushDirection * 0.1f);
            // Сбрасываем вертикальную скорость, если она положительная
            if (verticalVelocity > 0)
                verticalVelocity = 0;
        }
    }

    // Форс включения нормальной гравитации (если нужно вручную)
    public void ForceEnableNormalGravity()
    {
        hasLandedOnTerrain = true;
        currentGravity = normalGravity;
        Debug.Log("Принудительно включена нормальная гравитация.");
    }

    // Вкл/выкл отладочных лучей
    public void SetDebugRays(bool enabled)
    {
        showDebugRays = enabled;
    }

    public void SetAutoHeightUpdate(bool enabled)
    {
        autoUpdateHeight = enabled;
    }

    public void SetGravity(float gravity)
    {
        currentGravity = gravity;
    }

    public void ResetLandingState()
    {
        hasLandedOnTerrain = false;
        currentGravity = initialGravity;
    }
}