using UnityEngine;

public class CharacterAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private CharacterController characterController;

    [Header("Animation Parameters")]
    private readonly int speedHash = Animator.StringToHash("Speed");
    private readonly int isGroundedHash = Animator.StringToHash("IsGrounded");
    private readonly int isCrouchingHash = Animator.StringToHash("IsCrouching");

    private Vector3 lastPosition;
    private float currentSpeed;

    void Start()
    {
        // Auto-find components if not assigned
        if (animator == null)
            animator = GetComponent<Animator>();

        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        lastPosition = transform.position;
    }

    void Update()
    {
        if (animator == null) return;

        UpdateMovementAnimation();
    }

    void UpdateMovementAnimation()
    {
        // Calculate actual movement speed
        Vector3 currentPosition = transform.position;
        Vector3 velocity = (currentPosition - lastPosition) / Time.deltaTime;
        currentSpeed = new Vector3(velocity.x, 0, velocity.z).magnitude;
        lastPosition = currentPosition;

        // Smooth speed value
        float animSpeed = Mathf.Lerp(
            animator.GetFloat(speedHash),
            currentSpeed,
            Time.deltaTime * 10f
        );

        // Set animator parameters
        animator.SetFloat(speedHash, animSpeed);

        // Ground check
        if (characterController != null)
        {
            animator.SetBool(isGroundedHash, characterController.isGrounded);
        }

        // Crouch check (from input)
        bool isCrouching = Input.GetKey(KeyCode.LeftControl);
        animator.SetBool(isCrouchingHash, isCrouching);
    }

    // Public methods for other scripts to call
    public void SetCrouching(bool crouching)
    {
        if (animator != null)
            animator.SetBool(isCrouchingHash, crouching);
    }

    public void PlayAnimation(string triggerName)
    {
        if (animator != null)
            animator.SetTrigger(triggerName);
    }
}