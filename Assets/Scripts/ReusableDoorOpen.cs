using System.Collections;
using UnityEngine;

public class DebuggedReusableDoor : MonoBehaviour
{
    [Header("UI")]
    public GameObject instructionText;

    [Header("Door Settings")]
    public GameObject doorObject;
    public AudioSource doorOpenSound;
    public AudioSource doorCloseSound;

    [Header("Animation")]
    public string openAnimationName = "DoorOpen";
    public string closeAnimationName = "DoorClose";

    [Header("Timing")]
    public float doorOpenDuration = 3f;
    public bool autoClose = true;

    [Header("Debug")]
    public bool showDebugLogs = true;

    private Animator doorAnimator;
    private bool playerInRange = false;
    private bool isDoorOpen = false;
    private bool isAnimating = false;
    private Collider triggerCollider;

    void Start()
    {
        if (instructionText != null)
            instructionText.SetActive(false);

        if (doorObject != null)
            doorAnimator = doorObject.GetComponent<Animator>();

        // Get trigger collider
        triggerCollider = GetComponent<Collider>();

        if (triggerCollider != null && !triggerCollider.isTrigger)
        {
            Debug.LogError("HIBA: Collider nem trigger! Állítsd be Is Trigger-t!");
        }

        DebugLog("Door inicializálva");
    }

    void OnTriggerEnter(Collider other)
    {
        DebugLog($"OnTriggerEnter - Collider: {other.name}, Tag: {other.tag}");

        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            DebugLog("Játékos belépett a trigger-be");

            if (instructionText != null && !isDoorOpen)
            {
                instructionText.SetActive(true);
                DebugLog("Instruction megjelenítve");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        DebugLog($"OnTriggerExit - Collider: {other.name}");

        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            DebugLog("Játékos kilépett a trigger-bõl");

            if (instructionText != null)
            {
                instructionText.SetActive(false);
                DebugLog("Instruction elrejtve");
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        // Folyamatosan ellenõrizzük hogy a játékos bent van-e
        if (other.CompareTag("Player"))
        {
            if (!playerInRange)
            {
                DebugLog("OnTriggerStay - Player range újra true");
                playerInRange = true;
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            DebugLog($"E lenyomva - playerInRange: {playerInRange}, isAnimating: {isAnimating}, isDoorOpen: {isDoorOpen}");

            if (playerInRange && !isAnimating)
            {
                if (isDoorOpen)
                {
                    DebugLog("Ajtó bezárása...");
                    CloseDoor();
                }
                else
                {
                    DebugLog("Ajtó nyitása...");
                    OpenDoor();
                }
            }
        }
    }

    void OpenDoor()
    {
        if (doorAnimator == null)
        {
            Debug.LogError("HIBA: doorAnimator null!");
            return;
        }

        if (isAnimating)
        {
            DebugLog("Már animálódik, skip");
            return;
        }

        isAnimating = true;
        isDoorOpen = true;

        DebugLog("OpenDoor végrehajtva");

        // Hide instruction
        if (instructionText != null)
            instructionText.SetActive(false);

        // Play animation
        doorAnimator.Play(openAnimationName);
        DebugLog($"Animáció lejátszva: {openAnimationName}");

        // Play sound
        if (doorOpenSound != null)
            doorOpenSound.Play();

        // Start auto-close
        if (autoClose)
        {
            StartCoroutine(AutoCloseDoor());
        }
        else
        {
            StartCoroutine(ResetAnimatingFlag());
        }
    }

    void CloseDoor()
    {
        if (doorAnimator == null || !isDoorOpen) return;

        isAnimating = true;
        isDoorOpen = false;

        DebugLog("CloseDoor végrehajtva");

        // Play close animation
        if (!string.IsNullOrEmpty(closeAnimationName))
        {
            doorAnimator.Play(closeAnimationName);
            DebugLog($"Animáció lejátszva: {closeAnimationName}");
        }

        // Play close sound
        if (doorCloseSound != null)
            doorCloseSound.Play();

        StartCoroutine(ResetDoorState());
    }

    IEnumerator AutoCloseDoor()
    {
        DebugLog($"AutoClose timer kezdõdött ({doorOpenDuration}s)");

        // Wait for animation to finish
        yield return new WaitForSeconds(1f);
        isAnimating = false;

        // Show instruction if player still there
        if (playerInRange && instructionText != null)
        {
            instructionText.SetActive(true);
            DebugLog("Instruction újra megjelenítve (nyitott ajtónál)");
        }

        // Wait before closing
        yield return new WaitForSeconds(doorOpenDuration);

        if (isDoorOpen)
        {
            DebugLog("Auto-close aktiválva");
            CloseDoor();
        }
    }

    IEnumerator ResetDoorState()
    {
        yield return new WaitForSeconds(1f);

        isAnimating = false;
        DebugLog($"Animating flag reset - playerInRange: {playerInRange}");

        // Show instruction again if player still in range
        if (playerInRange && instructionText != null && !isDoorOpen)
        {
            instructionText.SetActive(true);
            DebugLog("Instruction újra megjelenítve (zárt ajtónál)");
        }
    }

    IEnumerator ResetAnimatingFlag()
    {
        yield return new WaitForSeconds(1f);
        isAnimating = false;

        if (playerInRange && instructionText != null)
        {
            instructionText.SetActive(true);
        }
    }

    void DebugLog(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[DOOR {gameObject.name}] {message}");
        }
    }

    // Gizmo a Scene view-ban
    void OnDrawGizmos()
    {
        if (triggerCollider == null)
            triggerCollider = GetComponent<Collider>();

        if (triggerCollider != null)
        {
            Gizmos.color = playerInRange ? Color.green : Color.yellow;
            Gizmos.DrawWireCube(transform.position + triggerCollider.bounds.center, triggerCollider.bounds.size);
        }
    }
}