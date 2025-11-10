using System.Collections;
using UnityEngine;

public class ReusableDoorController : MonoBehaviour
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
    public float doorOpenDuration = 3f; // Meddig maradjon nyitva
    public bool autoClose = true; // Automatikusan becsukódjon?

    [Header("Key Settings")]
    public bool requiresKey = false; // Kell-e kulcs?
    public HorrorInventorySystem.ItemType requiredKeyType = HorrorInventorySystem.ItemType.Key;

    private Animator doorAnimator;
    private bool playerInRange = false;
    private bool isDoorOpen = false;
    private bool isAnimating = false;
    private HorrorInventorySystem inventorySystem;

    void Start()
    {
        if (instructionText != null)
            instructionText.SetActive(false);

        if (doorObject != null)
            doorAnimator = doorObject.GetComponent<Animator>();

        // Find inventory system
        inventorySystem = FindObjectOfType<HorrorInventorySystem>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (instructionText != null && !isDoorOpen)
            {
                if (requiresKey)
                {
                    instructionText.SetActive(true);
                    // Show different message based on key availability
                    UnityEngine.UI.Text txtComponent = instructionText.GetComponent<UnityEngine.UI.Text>();
                    if (txtComponent != null)
                    {
                        if (HasRequiredKey())
                        {
                            txtComponent.text = "[E] Ajtó kinyitása kulccsal";
                        }
                        else
                        {
                            txtComponent.text = "Zárva - Kulcs szükséges";
                        }
                    }
                }
                else
                {
                    instructionText.SetActive(true);
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (instructionText != null)
                instructionText.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && playerInRange && !isAnimating)
        {
            if (isDoorOpen)
            {
                // Close door
                CloseDoor();
            }
            else
            {
                // Open door
                if (requiresKey)
                {
                    if (HasRequiredKey())
                    {
                        OpenDoor();
                    }
                    else
                    {
                        // Play locked sound or show message
                        Debug.Log("Nincs megfelelõ kulcsod!");
                    }
                }
                else
                {
                    OpenDoor();
                }
            }
        }
    }

    bool HasRequiredKey()
    {
        if (inventorySystem == null) return false;
        return inventorySystem.HasItem(requiredKeyType);
    }

    void OpenDoor()
    {
        if (doorAnimator == null || isAnimating) return;

        isAnimating = true;
        isDoorOpen = true;

        // Hide instruction
        if (instructionText != null)
            instructionText.SetActive(false);

        // Play animation
        doorAnimator.Play(openAnimationName);

        // Play sound
        if (doorOpenSound != null)
            doorOpenSound.Play();

        // Start close timer if auto-close is enabled
        if (autoClose)
        {
            StartCoroutine(AutoCloseDoor());
        }
        else
        {
            isAnimating = false;
        }
    }

    void CloseDoor()
    {
        if (doorAnimator == null || isAnimating) return;

        isAnimating = true;
        isDoorOpen = false;

        // Play close animation (if you have one)
        if (!string.IsNullOrEmpty(closeAnimationName))
        {
            doorAnimator.Play(closeAnimationName);
        }
        else
        {
            // If no close animation, play open animation backwards
            doorAnimator.Play(openAnimationName);
            doorAnimator.SetFloat("Speed", -1f);
        }

        // Play close sound
        if (doorCloseSound != null)
            doorCloseSound.Play();

        StartCoroutine(ResetDoorState());
    }

    IEnumerator AutoCloseDoor()
    {
        yield return new WaitForSeconds(doorOpenDuration);

        CloseDoor();
    }

    IEnumerator ResetDoorState()
    {
        yield return new WaitForSeconds(1f); // Wait for animation to finish

        isAnimating = false;

        // Show instruction again if player still in range
        if (playerInRange && instructionText != null)
        {
            instructionText.SetActive(true);
        }
    }

    // Public method to lock/unlock door from other scripts
    public void SetLocked(bool locked)
    {
        requiresKey = locked;
    }

    public void ForceOpen()
    {
        OpenDoor();
    }

    public void ForceClose()
    {
        CloseDoor();
    }
}