using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HorrorHUDManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HorrorPlayerController playerController;

    [Header("Stamina UI")]
    [SerializeField] private Image staminaBar;
    [SerializeField] private Image staminaBarBackground;
    [SerializeField] private CanvasGroup staminaGroup;
    [SerializeField] private Color staminaNormalColor = new Color(0.3f, 0.8f, 1f, 0.8f);
    [SerializeField] private Color staminaLowColor = new Color(1f, 0.3f, 0.3f, 0.8f);

    [Header("Fear UI")]
    [SerializeField] private Image fearVignette;
    [SerializeField] private Image fearBar;
    [SerializeField] private CanvasGroup fearGroup;
    [SerializeField] private Color fearColor = new Color(0.8f, 0f, 0f, 0.5f);

    [Header("Flashlight UI")]
    [SerializeField] private Image flashlightIcon;
    [SerializeField] private TextMeshProUGUI flashlightText;
    [SerializeField] private Color flashlightOnColor = Color.yellow;
    [SerializeField] private Color flashlightOffColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

    [Header("Interaction Prompt")]
    [SerializeField] private TextMeshProUGUI interactionText;
    [SerializeField] private CanvasGroup interactionGroup;

    [Header("Inventory UI")]
    [SerializeField] private Image[] inventorySlots = new Image[3];
    [SerializeField] private TextMeshProUGUI[] inventorySlotNumbers = new TextMeshProUGUI[3];
    [SerializeField] private Color emptySlotColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
    [SerializeField] private Color filledSlotColor = new Color(0.4f, 0.4f, 0.4f, 0.8f);
    [SerializeField] private Color selectedSlotColor = new Color(0.8f, 0.8f, 0.2f, 0.9f);

    [Header("Health/Sanity")]
    [SerializeField] private Image healthBar;
    [SerializeField] private float currentHealth = 100f;
    [SerializeField] private float maxHealth = 100f;

    [Header("Settings")]
    [SerializeField] private float fadeSpeed = 2f;
    [SerializeField] private bool showStaminaOnlyWhenUsed = true;
    [SerializeField] private float staminaDisplayTime = 2f;

    private float staminaDisplayTimer = 0f;
    private float previousStamina = 100f;
    private bool isFlashlightOn = false;

    void Start()
    {
        // Find player if not assigned
        if (playerController == null)
        {
            playerController = FindObjectOfType<HorrorPlayerController>();
        }

        // Initialize UI
        if (staminaGroup != null)
            staminaGroup.alpha = showStaminaOnlyWhenUsed ? 0f : 1f;

        if (fearGroup != null)
            fearGroup.alpha = 0f;

        if (interactionGroup != null)
            interactionGroup.alpha = 0f;

        if (interactionText != null)
            interactionText.text = "";
    }

    void Update()
    {
        if (playerController == null) return;

        UpdateStaminaBar();
        UpdateFearEffects();
        UpdateFlashlightIndicator();
        UpdateHealthBar();
    }

    void UpdateStaminaBar()
    {
        if (staminaBar == null) return;

        float stamina = playerController.GetStamina();

        // Update bar fill
        staminaBar.fillAmount = Mathf.Lerp(
            staminaBar.fillAmount,
            stamina / 100f,
            Time.deltaTime * 5f
        );

        // Color change based on stamina level
        if (stamina < 30f)
        {
            staminaBar.color = Color.Lerp(staminaBar.color, staminaLowColor, Time.deltaTime * 3f);

            // Pulsing effect when low
            float pulse = Mathf.PingPong(Time.time * 2f, 0.3f);
            staminaBar.color = new Color(
                staminaBar.color.r,
                staminaBar.color.g,
                staminaBar.color.b,
                0.5f + pulse
            );
        }
        else
        {
            staminaBar.color = Color.Lerp(staminaBar.color, staminaNormalColor, Time.deltaTime * 3f);
        }

        // Auto-hide stamina bar when full and not used
        if (showStaminaOnlyWhenUsed && staminaGroup != null)
        {
            if (stamina < previousStamina || stamina < 95f)
            {
                staminaDisplayTimer = staminaDisplayTime;
            }

            if (staminaDisplayTimer > 0)
            {
                staminaGroup.alpha = Mathf.Lerp(staminaGroup.alpha, 1f, Time.deltaTime * fadeSpeed);
                staminaDisplayTimer -= Time.deltaTime;
            }
            else
            {
                staminaGroup.alpha = Mathf.Lerp(staminaGroup.alpha, 0f, Time.deltaTime * fadeSpeed);
            }
        }

        previousStamina = stamina;
    }

    void UpdateFearEffects()
    {
        float fear = playerController.GetFearLevel();

        // Vignette effect
        if (fearVignette != null)
        {
            Color vignetteColor = fearColor;
            vignetteColor.a = Mathf.Lerp(0f, 0.7f, fear / 100f);
            fearVignette.color = Color.Lerp(fearVignette.color, vignetteColor, Time.deltaTime * 2f);
        }

        // Fear bar (only visible when fear > 0)
        if (fearBar != null)
        {
            fearBar.fillAmount = Mathf.Lerp(fearBar.fillAmount, fear / 100f, Time.deltaTime * 3f);
        }

        if (fearGroup != null)
        {
            float targetAlpha = fear > 5f ? 1f : 0f;
            fearGroup.alpha = Mathf.Lerp(fearGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
        }

        // Screen shake intensity increases with fear (handled in player controller)
    }

    void UpdateFlashlightIndicator()
    {
        if (flashlightIcon == null) return;

        // Check if flashlight is on (you'll need to add a public method to player controller)
        // For now, we'll check with Input as a workaround
        if (Input.GetKeyDown(KeyCode.F))
        {
            isFlashlightOn = !isFlashlightOn;
        }

        Color targetColor = isFlashlightOn ? flashlightOnColor : flashlightOffColor;
        flashlightIcon.color = Color.Lerp(flashlightIcon.color, targetColor, Time.deltaTime * 5f);

        if (flashlightText != null)
        {
            flashlightText.text = isFlashlightOn ? "ON" : "OFF";
            flashlightText.color = targetColor;
        }
    }

    void UpdateHealthBar()
    {
        if (healthBar == null) return;

        healthBar.fillAmount = Mathf.Lerp(
            healthBar.fillAmount,
            currentHealth / maxHealth,
            Time.deltaTime * 5f
        );

        // Red color when low health
        if (currentHealth < 30f)
        {
            float pulse = Mathf.PingPong(Time.time * 3f, 0.5f);
            healthBar.color = new Color(1f, pulse, pulse, 1f);
        }
    }

    // Public methods to control UI
    public void ShowInteractionPrompt(string message)
    {
        if (interactionText != null)
        {
            interactionText.text = message;
        }

        if (interactionGroup != null)
        {
            interactionGroup.alpha = 1f;
        }
    }

    public void HideInteractionPrompt()
    {
        if (interactionGroup != null)
        {
            StartCoroutine(FadeOutInteraction());
        }
    }

    System.Collections.IEnumerator FadeOutInteraction()
    {
        while (interactionGroup.alpha > 0)
        {
            interactionGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }

        if (interactionText != null)
        {
            interactionText.text = "";
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        // Increase fear when taking damage
        if (playerController != null)
        {
            playerController.IncreaseFear(damage * 0.5f);
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
    }

    public float GetHealth()
    {
        return currentHealth;
    }
}