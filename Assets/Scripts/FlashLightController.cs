using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    [Header("Beállítások")]
    public Light flashlightSource;
    public bool isOn = true;

    [Header("Elem (Battery) Rendszer")]
    public bool useBattery = true;
    public float maxBattery = 100.0f;
    public float currentBattery = 100.0f;
    public float drainRate = 2.0f;

    [Header("Fényerõ")]
    public float maxIntensity = 3.0f;

 
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleFlashlight();
        }
        if (isOn && useBattery)
        {
            DrainBattery();
        }
    }
    void ToggleFlashlight()
    {
        isOn = !isOn;

        if (flashlightSource != null)
        {
            flashlightSource.enabled = isOn;
            Debug.Log("Lámpa kapcsolva: " + isOn);
        }
    }
    void DrainBattery()
    {
        if (currentBattery > 0)
        {
            currentBattery -= drainRate * Time.deltaTime;
            if (flashlightSource != null)
            {
                flashlightSource.intensity = Mathf.Lerp(0, maxIntensity, currentBattery / maxBattery);
            }
        }
        else
        {
            currentBattery = 0;
            isOn = false;
            flashlightSource.enabled = false;
            Debug.Log("Az elem lemerült!");
        }
    }
}