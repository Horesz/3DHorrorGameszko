using UnityEngine;

public class HorrorPlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 5f;
    [SerializeField] private float crouchSpeed = 1.5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 1f;

    [Header("Camera Settings")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float cameraShakeIntensity = 0.1f;

    [Header("Horror Effects")]
    [SerializeField] private Light flashlight;
    [SerializeField] private float stamina = 100f;
    [SerializeField] private float staminaDecreaseRate = 10f;
    [SerializeField] private float staminaRecoveryRate = 5f;
    [SerializeField] private AudioSource breathingSound;
    [SerializeField] private float fearLevel = 0f; // 0-100

    // Private variables
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isCrouching;
    private float originalHeight;
    private float xRotation = 0f;
    private Vector3 originalCameraPos;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Save original values
        originalHeight = controller.height;
        if (playerCamera != null)
            originalCameraPos = playerCamera.localPosition;

        // Setup flashlight
        if (flashlight != null)
            flashlight.enabled = false;
    }

    void Update()
    {
        HandleMovement();
        HandleCamera();
        HandleFlashlight();
        HandleStamina();
        HandleHorrorEffects();
    }

    void HandleMovement()
    {
        // Ground check
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Get input
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Crouch
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isCrouching = !isCrouching;
            controller.height = isCrouching ? originalHeight * 0.5f : originalHeight;
        }

        // Calculate speed
        float currentSpeed = walkSpeed;

        if (Input.GetKey(KeyCode.LeftShift) && !isCrouching && stamina > 0)
        {
            currentSpeed = runSpeed;
            stamina -= staminaDecreaseRate * Time.deltaTime;
        }
        else if (isCrouching)
        {
            currentSpeed = crouchSpeed;
        }

        // Move
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * currentSpeed * Time.deltaTime);

        // Jump (optional for horror - lehet kiveszed)
        if (Input.GetButtonDown("Jump") && isGrounded && !isCrouching)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleCamera()
    {
        if (playerCamera == null) return;

        // Mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotate camera up/down
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Rotate player left/right
        transform.Rotate(Vector3.up * mouseX);

        // Camera shake when running or scared
        if (Input.GetKey(KeyCode.LeftShift) || fearLevel > 50f)
        {
            float shakeAmount = cameraShakeIntensity * (fearLevel / 100f);
            Vector3 shake = new Vector3(
                Random.Range(-shakeAmount, shakeAmount),
                Random.Range(-shakeAmount, shakeAmount),
                0f
            );
            playerCamera.localPosition = originalCameraPos + shake;
        }
        else
        {
            playerCamera.localPosition = Vector3.Lerp(
                playerCamera.localPosition,
                originalCameraPos,
                Time.deltaTime * 5f
            );
        }
    }

    void HandleFlashlight()
    {
        if (flashlight == null) return;

        // Toggle flashlight with F key
        if (Input.GetKeyDown(KeyCode.F))
        {
            flashlight.enabled = !flashlight.enabled;
        }

        // Flicker effect when fear is high
        if (flashlight.enabled && fearLevel > 70f)
        {
            flashlight.intensity = Random.Range(0.5f, 1f);
        }
    }

    void HandleStamina()
    {
        // Recover stamina
        if (!Input.GetKey(KeyCode.LeftShift))
        {
            stamina += staminaRecoveryRate * Time.deltaTime;
            stamina = Mathf.Clamp(stamina, 0f, 100f);
        }

        // Heavy breathing when low stamina
        if (breathingSound != null)
        {
            if (stamina < 30f && !breathingSound.isPlaying)
            {
                breathingSound.Play();
            }
            else if (stamina > 50f && breathingSound.isPlaying)
            {
                breathingSound.Stop();
            }
        }
    }

    void HandleHorrorEffects()
    {
        // Példa: növeld a fear level-t bizonyos helyzetekben
        // Ezt majd a játék logikájában hívhatod meg

        // Fear slowly decreases over time
        if (fearLevel > 0)
        {
            fearLevel -= 5f * Time.deltaTime;
            fearLevel = Mathf.Clamp(fearLevel, 0f, 100f);
        }
    }

    // Publikus metódusok amiket más scriptek hívhatnak
    public void IncreaseFear(float amount)
    {
        fearLevel += amount;
        fearLevel = Mathf.Clamp(fearLevel, 0f, 100f);
    }

    public float GetStamina()
    {
        return stamina;
    }

    public float GetFearLevel()
    {
        return fearLevel;
    }

    public bool IsFlashlightOn()
    {
        return flashlight != null && flashlight.enabled;
    }

    // ESC billentyû a kurzor feloldásához (teszteléshez)
    void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}