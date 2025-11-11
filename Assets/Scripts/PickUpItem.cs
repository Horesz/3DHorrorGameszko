using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [Header("Item Data")]
    public string itemName = "Kulcs";
    public ItemType itemType = ItemType.Key;
    public GameObject itemPrefab; // Reference to itself for dropping

    [Header("Visual")]
    public Material normalMaterial;
    public Material highlightMaterial;

    private Renderer itemRenderer;
    private Material originalMaterial;

    // ItemType enum - ez kell az új inventory rendszerhez
    public enum ItemType
    {
        Key,
        Broom,
        Document,
        Medicine,
        Flashlight,
        Tool,
        Other
    }

    void Start()
    {
        itemRenderer = GetComponent<Renderer>();

        if (itemRenderer != null)
        {
            originalMaterial = itemRenderer.material;

            // Create highlight material if not assigned
            if (highlightMaterial == null)
            {
                highlightMaterial = new Material(originalMaterial);
                highlightMaterial.color = Color.yellow;
                // Add emission for glow effect
                highlightMaterial.EnableKeyword("_EMISSION");
                highlightMaterial.SetColor("_EmissionColor", Color.yellow * 0.5f);
            }
        }

        // Set prefab reference to self if not assigned
        if (itemPrefab == null)
        {
            itemPrefab = gameObject;
        }
    }

    public void SetHighlight(bool highlight)
    {
        if (itemRenderer == null) return;

        if (highlight)
        {
            itemRenderer.material = highlightMaterial;
        }
        else
        {
            itemRenderer.material = originalMaterial;
        }
    }

    // Optional: Add rotation for visual effect
    void Update()
    {
        transform.Rotate(Vector3.up * 30f * Time.deltaTime);
    }
}