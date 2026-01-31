using UnityEngine;
using System.Collections;

public class ScaryFloatAttack : MonoBehaviour
{
    [Header("Phase 1 - Rise")]
    public float riseHeight = 1.5f;
    public float riseDuration = 2f;

    [Header("Phase 2 - Spin")]
    public float spinStartSpeed = 30f;
    public float spinMaxSpeed = 400f;
    public float spinAccelerationTime = 2f;

    [Header("Phase 3 - Attack")]
    public float attackSpeed = 8f;

    [Header("One-shot")]
    public bool oneShot = true;

    Vector3 startPos;
    bool active;

    void Awake()
    {
        startPos = transform.position;
        active = false;
    }

    public void TriggerScare()
    {
        if (active && oneShot) return;
        if (!active) StartCoroutine(ScareSequence());
    }

    IEnumerator ScareSequence()
    {
        active = true;

        // Phase 1: rise
        float t = 0f;
        Vector3 targetPos = startPos + Vector3.up * riseHeight;

        while (t < 1f)
        {
            t += Time.deltaTime / riseDuration;
            float eased = t * t;
            transform.position = Vector3.Lerp(startPos, targetPos, eased);
            yield return null;
        }

        // Phase 2: spin accelerate
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / spinAccelerationTime;
            float spinSpeed = Mathf.Lerp(spinStartSpeed, spinMaxSpeed, t);
            transform.Rotate(Vector3.up * spinSpeed * Time.deltaTime, Space.World);
            yield return null;
        }

        // Phase 3: move world +X forever
        Vector3 dir = Vector3.right; // +X
        while (true)
        {
            transform.position += dir * attackSpeed * Time.deltaTime;
            transform.Rotate(Vector3.up * spinMaxSpeed * Time.deltaTime, Space.World);
            yield return null;
        }
    }
}
