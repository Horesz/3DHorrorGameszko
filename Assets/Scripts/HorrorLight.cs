using UnityEngine;

public class HorrorLight : MonoBehaviour
{
    private Light myLight;
    public float minIntensity = 0.1f;
    public float maxIntensity = 2.0f;

    void Start()
    {
        myLight = GetComponent<Light>();
    }

    void Update()
    {
        if (Random.value > 0.9f)
        {
            myLight.intensity = Random.Range(minIntensity, maxIntensity);
        }
    }
}