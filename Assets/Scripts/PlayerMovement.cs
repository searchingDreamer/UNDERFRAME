using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement requirements")]
    public Camera playerCamera;
    public AudioSource footstepSource;
    public AudioClip[] footstepClips;
    public Transform cameraTransform;
    public Transform groundCheckObj;
    public LayerMask groundMask;
    public float normalFOV = 60f;

    // Camera properties (Don't change anything!)
    readonly float ratioFOV = 10f;
    readonly float fovTransitionSpeed = 5f;
    private float targetFOV;

    // Audio properties (Don't change anything!)
    private float footstepRate = 0.5f;
    private float footstepTimer = 0f;

    // Run properties (Don't change anything!)
    readonly float staminaDrainRate = 7f;
    readonly float staminaRegenRate = 2f;
    readonly float staminaRegenDelay = 1f;
    private float lastRunTime;

    // Crouch properties (Don't change anything!)
    readonly float crouchHeight = 0.5f;
    readonly float crouchTransitionSpeed = 6f;
    private float originalHeight;
    private bool isCrouching;
    private Vector3 originalCenter;
    private Vector3 targetCamLocalPos;

    // Bobbing properties (Don't change anything!)
    readonly float bobFrequencyWalk = 13f;
    readonly float bobAmplitudeWalk = 0.05f;
    readonly float bobFrequencyRun = 20f;
    readonly float bobAmplitudeRun = 0.1f;
    private float bobTimer = 0f;
    private Vector3 initialCamLocalPos;
    private Quaternion initialCamLocalRot;

    // Gravity properties (Don't change anything!)
    readonly float groundDistance = 0.4f;
    readonly float gravity = -20f;
    readonly float jumpHeight = 1.5f;
    private bool isGrounded;
    private Vector3 velocity;

    // Other properties (Don't change anything!)
    private float currentSpeed;
    private CharacterController controller;
    private PlayerStats playerStats;
    private Vector3 move;

    void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        controller = GetComponent<CharacterController>();
        initialCamLocalPos = cameraTransform.localPosition;
        initialCamLocalRot = cameraTransform.localRotation;
        originalHeight = controller.height;
        originalCenter = controller.center;
        playerStats.currentStamina = playerStats.maxStamina;

        targetCamLocalPos = initialCamLocalPos;

        if (playerCamera != null)
            normalFOV = playerCamera.fieldOfView;

        targetFOV = normalFOV;
    }

    void Update()
    {
        HandleGroundCheck();
        HandleMovementInput();
        ApplyGravity();
        HandleStamina();
        HandleHeadBobbing();
    }

    void HandleGroundCheck()
    {
        isGrounded = Physics.CheckSphere(groundCheckObj.position, groundDistance, groundMask);
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;
    }

    void HandleMovementInput()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        move = transform.right * x + transform.forward * z;

        if (move.magnitude > 1f)
            move = move.normalized;

        // Ввод на присед (удержание)
        bool wantsToCrouch = Input.GetKey(KeyCode.LeftControl);

        // Проверка: можно ли встать?
        bool canStand = true;
        if (!wantsToCrouch)
        {
            float radius = controller.radius * 0.95f;
            float headCheckStart = transform.position.y + controller.height / 2f;
            float headCheckEnd = transform.position.y + originalHeight;
            Vector3 p1 = new Vector3(transform.position.x, headCheckStart, transform.position.z);
            Vector3 p2 = new Vector3(transform.position.x, headCheckEnd, transform.position.z);
            canStand = !Physics.CheckCapsule(p1, p2, radius, groundMask);
        }

        // Изменяем флаг приседа
        if (wantsToCrouch) isCrouching = true;
        else if (canStand) isCrouching = false;

        // Плавное изменение height и center
        float targetHeight = isCrouching ? crouchHeight : originalHeight;
        Vector3 targetCenter = isCrouching
            ? new Vector3(originalCenter.x, crouchHeight / 2f, originalCenter.z)
            : originalCenter;

        float prevHeight = controller.height;
        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);
        controller.center = Vector3.Lerp(controller.center, targetCenter, Time.deltaTime * crouchTransitionSpeed);

        // Компенсация рывков при смене высоты
        float heightDelta = controller.height - prevHeight;
        if (Mathf.Abs(heightDelta) > 0.001f)
        {
            Vector3 correction = new Vector3(0f, heightDelta / 2f, 0f);
            controller.Move(correction);
        }

        // Скорость
        bool wantsToRun = Input.GetKey(KeyCode.LeftShift) && !isCrouching && move.magnitude > 0.1f;
        if (wantsToRun && playerStats.currentStamina > 0f)
        {
            currentSpeed = playerStats.runSpeed;
            lastRunTime = Time.time;
        }
        else currentSpeed = isCrouching ? 1.25f : 2.5f;

        if (Input.GetButtonDown("Jump") && isGrounded && !isCrouching)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        // Движение
        controller.Move(currentSpeed * Time.deltaTime * move);

        // Звук шагов
        if (isGrounded && move.magnitude > 0.1f)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f && footstepClips.Length > 0 && footstepSource != null)
            {
                int index = Random.Range(0, footstepClips.Length);
                AudioClip clip = footstepClips[index];

                footstepSource.pitch = Random.Range(0.9f, 1.1f);
                float volume = Random.Range(0.8f, 1f);
                footstepSource.PlayOneShot(clip, volume);

                float stepSpeedModifier = currentSpeed == playerStats.runSpeed ? 0.5f : (currentSpeed == 1.25f ? 1.2f : 1f);
                footstepTimer = footstepRate * stepSpeedModifier;
            }
        }
        else
        {
            footstepTimer = 0f;
        }

        // Обновление FOV камеры
        bool isRunningNow = Input.GetKey(KeyCode.LeftShift) && !isCrouching && move.magnitude > 0.1f;
        if (isRunningNow && playerStats.currentStamina > 0f) targetFOV = normalFOV + ratioFOV;
        else if (isCrouching) targetFOV = normalFOV - ratioFOV;
        else targetFOV = normalFOV;

        if (playerCamera != null)
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * fovTransitionSpeed);
    }

    void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleStamina()
    {
        bool isRunningNow = Input.GetKey(KeyCode.LeftShift) && !isCrouching && move.magnitude > 0.1f;

        if (isRunningNow && playerStats.currentStamina > 0)
        {
            playerStats.currentStamina -= staminaDrainRate * Time.deltaTime;
            playerStats.currentStamina = Mathf.Clamp(playerStats.currentStamina, 0f, playerStats.maxStamina);
            lastRunTime = Time.time;
        }
        else if (Time.time - lastRunTime >= staminaRegenDelay && playerStats.currentStamina < playerStats.maxStamina)
        {
            playerStats.currentStamina += staminaRegenRate * Time.deltaTime;
            playerStats.currentStamina = Mathf.Clamp(playerStats.currentStamina, 0f, playerStats.maxStamina);
        }
    }

    void HandleHeadBobbing()
    {
        bool isMovingInput = (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0) && isGrounded;

        if (!isMovingInput)
        {
            bobTimer = 0f;
            cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, targetCamLocalPos, Time.deltaTime * 5f);
            cameraTransform.localRotation = Quaternion.Lerp(cameraTransform.localRotation, initialCamLocalRot, Time.deltaTime * 5f);
            return;
        }

        float freq;
        float amp;

        if (currentSpeed == playerStats.runSpeed)
        {
            freq = bobFrequencyRun;
            amp = bobAmplitudeRun;
        }
        else
        {
            freq = bobFrequencyWalk;
            amp = bobAmplitudeWalk;
        }

        bobTimer += Time.deltaTime * freq;

        float bobY = Mathf.Sin(bobTimer) * amp;
        float bobZTilt = Mathf.Sin(bobTimer * 0.5f) * amp * 8f;

        cameraTransform.localPosition = targetCamLocalPos + new Vector3(0f, bobY, 0f);
        Quaternion targetRot = Quaternion.Euler(0f, 0f, bobZTilt);
        cameraTransform.localRotation = Quaternion.Slerp(cameraTransform.localRotation, targetRot, Time.deltaTime * 6f);
    }
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;

        // Проверяем, есть ли Rigidbody и он не кинематический
        if (body == null || body.isKinematic)
            return;

        // Не толкаем предметы, которые лежат на земле (чтобы не "прыгал" вверх)
        if (hit.moveDirection.y < -0.3f)
            return;

        // Рассчитываем силу толчка
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

        // Примерная сила, можно подстроить под свои скорости
        float pushPower = 3.0f;

        body.AddForce(pushDir * pushPower, ForceMode.Impulse);
    }
    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 200, 20), "Health: " + Mathf.FloorToInt(playerStats.currentHealth) + " / " + playerStats.maxHealth);
        GUI.Label(new Rect(10, 30, 200, 40), "Stamina: " + Mathf.FloorToInt(playerStats.currentStamina) + " / " + playerStats.maxStamina);
        GUI.Label(new Rect(10, 50, 200, 60), $"Strength: {playerStats.strength}");
        GUI.Label(new Rect(10, 70, 200, 80), $"Speed: {playerStats.runSpeed}");
        GUI.Label(new Rect(10, 90, 200, 100), $"Range: {playerStats.range}");
    }
}