using UnityEngine;

public class ProximityDoor : MonoBehaviour
{
    [Header("References")]
    public Animator doorAnimator;
    public AudioSource openSound;
    public AudioSource laughSound;
    public AudioSource closeSound;

    [Header("Settings")]
    public string playerTag = "Player";

    private static readonly int OpenID = Animator.StringToHash("Open");

    private bool isOpen;
    private bool hasTriggered;

    private void Awake()
    {
        if (doorAnimator != null)
            doorAnimator.SetBool(OpenID, false);

        isOpen = false;
        hasTriggered = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag(playerTag)) return;
        if (doorAnimator == null) return;

        hasTriggered = true;
        isOpen = true;

        doorAnimator.SetBool(OpenID, true);
        if (openSound != null) openSound.Play();
        if (laughSound != null) laughSound.Play();

        GetComponent<Collider>().enabled = false;
    }

    private void OnTriggerExit(Collider other)
    {

    }
}
