using UnityEngine;

public class FloatAndRotate : MonoBehaviour
{
    [Header("Float")]
    public float floatHeight = 0.4f;
    public float floatSpeed = 1.5f;

    [Header("Rotate")]
    public Vector3 rotationSpeed = new Vector3(0f, 60f, 0f);

    [Header("Activation")]
    public bool startActive = false;

    private Vector3 startPos;
    private bool active;

    void Awake()
    {
        startPos = transform.position;
        active = startActive;
    }

    void Update()
    {
        if (!active) return;

        float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = startPos + new Vector3(0f, yOffset, 0f);

        transform.Rotate(rotationSpeed * Time.deltaTime, Space.Self);
    }

    public void Activate()
    {
        active = true;
    }

    public void Deactivate()
    {
        active = false;
        transform.position = startPos;
    }

    public void Toggle()
    {
        if (active) Deactivate();
        else Activate();
    }
}
