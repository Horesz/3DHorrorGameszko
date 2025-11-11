using UnityEngine;

public class PickupToInventory : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private float pickupRange = 3f;
    [SerializeField] private KeyCode pickupKey = KeyCode.E;
    [SerializeField] private LayerMask pickupLayer;

    [Header("UI")]
    [SerializeField] private GameObject pickupPrompt;
    [SerializeField] private UnityEngine.UI.Text promptText;

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private REStyleInventory inventory;

    private PickupItem currentItem;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (inventory == null)
            inventory = FindObjectOfType<REStyleInventory>();

        if (pickupPrompt != null)
            pickupPrompt.SetActive(false);
    }

    void Update()
    {
        CheckForPickupItem();

        if (Input.GetKeyDown(pickupKey) && currentItem != null)
        {
            TryPickupItem();
        }
    }

    void CheckForPickupItem()
    {
        if (playerCamera == null) return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, pickupRange, pickupLayer))
        {
            PickupItem item = hit.collider.GetComponent<PickupItem>();

            if (item != null)
            {
                if (currentItem != item)
                {
                    // New item
                    if (currentItem != null)
                        currentItem.SetHighlight(false);

                    currentItem = item;
                    currentItem.SetHighlight(true);

                    ShowPickupPrompt(item.itemName);
                }
                return;
            }
        }

        // No item in sight
        if (currentItem != null)
        {
            currentItem.SetHighlight(false);
            currentItem = null;
            HidePickupPrompt();
        }
    }

    void TryPickupItem()
    {
        if (currentItem == null || inventory == null) return;

        // Create inventory item from pickup
        REStyleInventory.InventoryItem invItem = CreateInventoryItem(currentItem);

        if (inventory.AddItem(invItem))
        {
            // Successfully added
            Destroy(currentItem.gameObject);
            currentItem = null;
            HidePickupPrompt();
        }
        else
        {
            Debug.Log("Nincs hely az inventory-ban!");
        }
    }

    REStyleInventory.InventoryItem CreateInventoryItem(PickupItem pickup)
    {
        REStyleInventory.InventoryItem item = new REStyleInventory.InventoryItem();

        item.itemName = pickup.itemName;
        item.worldPrefab = pickup.itemPrefab;

        // Set category based on type (using PickupItem.ItemType now!)
        item.category = ConvertItemType(pickup.itemType);

        // Set default descriptions
        switch (item.category)
        {
            case REStyleInventory.ItemCategory.Key:
                item.description = "Egy rozsdás kulcs. Vajon mit nyit?";
                item.isUsable = true;
                item.keyID = pickup.itemName; // Use name as key ID
                break;

            case REStyleInventory.ItemCategory.Document:
                item.description = "Egy régi feljegyzés.";
                item.isExaminable = true;
                item.documentContent = "Lorem ipsum..."; // TODO: Add real content
                break;

            case REStyleInventory.ItemCategory.Tool:
                item.description = "Egy eszköz.";
                item.isUsable = true;
                break;

            case REStyleInventory.ItemCategory.Flashlight:
                item.description = "Egy mûködõ zseblámpa.";
                item.isUsable = true;
                break;

            case REStyleInventory.ItemCategory.Medicine:
                item.description = "Gyógyszer. Helyreállítja az egészséget.";
                item.isUsable = true;
                break;

            default:
                item.description = "Egy furcsa tárgy.";
                break;
        }

        // Try to load icon (you'll need to create icons!)
        // item.icon = Resources.Load<Sprite>($"Icons/{pickup.itemName}");

        return item;
    }

    REStyleInventory.ItemCategory ConvertItemType(PickupItem.ItemType oldType)
    {
        switch (oldType)
        {
            case PickupItem.ItemType.Key:
                return REStyleInventory.ItemCategory.Key;

            case PickupItem.ItemType.Broom:
                return REStyleInventory.ItemCategory.Tool;

            case PickupItem.ItemType.Document:
                return REStyleInventory.ItemCategory.Document;

            case PickupItem.ItemType.Medicine:
                return REStyleInventory.ItemCategory.Medicine;

            case PickupItem.ItemType.Flashlight:
                return REStyleInventory.ItemCategory.Flashlight;

            case PickupItem.ItemType.Tool:
                return REStyleInventory.ItemCategory.Tool;

            default:
                return REStyleInventory.ItemCategory.Other;
        }
    }

    void ShowPickupPrompt(string itemName)
    {
        if (pickupPrompt != null)
        {
            pickupPrompt.SetActive(true);

            if (promptText != null)
                promptText.text = $"[{pickupKey}] Felvesz: {itemName}";
        }
    }

    void HidePickupPrompt()
    {
        if (pickupPrompt != null)
            pickupPrompt.SetActive(false);
    }
}