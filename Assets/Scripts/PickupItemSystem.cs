using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [Header("Item Info")]
    public string itemName = "Item";
    public HorrorInventorySystem.ItemType itemType;
    public GameObject itemPrefab; // Ez lesz az inventory-ban

    [Header("Visual Settings")]
    [SerializeField] private bool rotateItem = true;
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private bool bobUpDown = true;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobAmount = 0.2f;

    [Header("Highlight Effect")]
    [SerializeField] private bool highlightWhenLookingAt = true;
    [SerializeField] private Color highlightColor = Color.yellow;

    private Vector3 startPosition;
    private Renderer[] renderers;
    private Color[] originalColors;
    private bool isBeingLookedAt = false;

    void Start()
    {
        startPosition = transform.position;

        // Mentjük az eredeti színeket
        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.HasProperty("_Color"))
            {
                originalColors[i] = renderers[i].material.color;
            }
        }

        // Ha nincs beállítva itemPrefab, használjuk ezt a GameObject-et
        if (itemPrefab == null)
        {
            Debug.LogWarning($"{itemName} - itemPrefab nincs beállítva! Használd a Prefab mezõt!");
        }
    }

    void Update()
    {
        // Forgás animáció
        if (rotateItem)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }

        // Lebegés animáció
        if (bobUpDown)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobAmount;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    // Ezt az Inventory System hívja meg amikor ránézünk
    public void SetHighlight(bool highlight)
    {
        if (!highlightWhenLookingAt) return;

        isBeingLookedAt = highlight;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && renderers[i].material.HasProperty("_Color"))
            {
                if (highlight)
                {
                    renderers[i].material.color = highlightColor;
                }
                else
                {
                    renderers[i].material.color = originalColors[i];
                }
            }
        }
    }

    void OnDestroy()
    {
        // Cleanup materials
        foreach (var renderer in renderers)
        {
            if (renderer != null && renderer.material != null)
            {
                Destroy(renderer.material);
            }
        }
    }
}