using System.Collections;
using UnityEngine;

public class SimpleAutoDoor : MonoBehaviour
{
    public GameObject Instruction;
    public GameObject AnimeObject;
    public AudioSource DoorOpenSound;
    public AudioSource DoorCloseSound;

    [Header("Settings")]
    public float autoCloseDelay = 3f; // Mennyi idõ múlva csukódjon be
    public bool canReuse = true; // Többször használható?

    private Animator doorAnimator;
    private bool playerInRange = false;
    private bool isDoorOpen = false;
    private bool isAnimating = false;

    void Start()
    {
        if (Instruction != null)
            Instruction.SetActive(false);

        if (AnimeObject != null)
            doorAnimator = AnimeObject.GetComponent<Animator>();
    }

    void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;

            if (Instruction != null && !isDoorOpen && !isAnimating)
            {
                Instruction.SetActive(true);
            }
        }
    }

    void OnTriggerExit(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;

            if (Instruction != null)
                Instruction.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && playerInRange && !isAnimating)
        {
            if (!isDoorOpen)
            {
                OpenDoor();
            }
            else if (canReuse)
            {
                CloseDoor();
            }
        }
    }

    void OpenDoor()
    {
        if (isAnimating) return;

        isAnimating = true;
        isDoorOpen = true;

        // Hide instruction
        if (Instruction != null)
            Instruction.SetActive(false);

        // Play animation
        if (doorAnimator != null)
            doorAnimator.Play("DoorOpen");

        // Play sound
        if (DoorOpenSound != null)
            DoorOpenSound.Play();

        // Start auto-close coroutine
        StartCoroutine(AutoCloseCoroutine());
    }

    void CloseDoor()
    {
        if (!isDoorOpen) return;

        isDoorOpen = false;

        // Play close animation (reverse or separate animation)
        if (doorAnimator != null)
        {
            // If you have a "DoorClose" animation, use this:
            // doorAnimator.Play("DoorClose");

            // Or reverse the open animation:
            doorAnimator.Play("DoorOpen", 0, 1f); // Start from end
            doorAnimator.speed = -1f; // Play backwards
        }

        // Play close sound
        if (DoorCloseSound != null)
            DoorCloseSound.Play();

        StartCoroutine(ResetAfterClose());
    }

    IEnumerator AutoCloseCoroutine()
    {
        // Wait for door to fully open
        yield return new WaitForSeconds(1f);

        isAnimating = false;

        // Show instruction again if player still near and reusable
        if (playerInRange && Instruction != null && canReuse)
        {
            Instruction.SetActive(true);
        }

        // Wait before auto-closing
        yield return new WaitForSeconds(autoCloseDelay);

        // Auto close if door is still open
        if (isDoorOpen && canReuse)
        {
            CloseDoor();
        }
    }

    IEnumerator ResetAfterClose()
    {
        yield return new WaitForSeconds(1f);

        // Reset animator speed
        if (doorAnimator != null)
            doorAnimator.speed = 1f;

        isAnimating = false;

        // Show instruction if player still in range
        if (playerInRange && Instruction != null && canReuse)
        {
            Instruction.SetActive(true);
        }
    }
}