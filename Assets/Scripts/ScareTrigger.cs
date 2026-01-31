using UnityEngine;

public class ScareTrigger : MonoBehaviour
{
    public ScaryFloatAttack target;
    public string playerTag = "Player";
    public bool oneShot = true;

    bool used;

    void Reset()
    {
        // Helps auto-configure when you add the script
        var c = GetComponent<Collider>();
        if (c) c.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[ScareTrigger] Enter: {other.name} (tag={other.tag})");

        if (used && oneShot) return;
        if (!other.CompareTag(playerTag)) return;
        if (target == null)
        {
            Debug.LogError("[ScareTrigger] Target is NOT assigned!");
            return;
        }

        used = true;
        Debug.Log("[ScareTrigger] Triggering scare!");
        target.TriggerScare();

        // Optional: disable trigger collider so it can never fire again
        if (oneShot)
        {
            var col = GetComponent<Collider>();
            if (col) col.enabled = false;
        }
    }
}
