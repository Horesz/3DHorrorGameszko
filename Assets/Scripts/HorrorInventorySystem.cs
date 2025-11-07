using UnityEngine;
using System.Collections.Generic;

public class HorrorInventorySystem : MonoBehaviour
{
    [Header("Inventory Settings")]
    [SerializeField] private int maxSlots = 3;
    [SerializeField] private Transform itemHoldPosition; // Kamera elõtti pozíció
    [SerializeField] private float itemHoldDistance = 1.5f;
    [SerializeField] private Vector3 itemHoldOffset = new Vector3(0.3f, -0.2f, 0);

    [Header("Item Prefabs - Drag items here")]
    [SerializeField] private GameObject broomPrefab; // Seprû prefab
    [SerializeField] private GameObject keyPrefab; // Kulcs prefab

    [Header("Pickup Settings")]
    [SerializeField] private float pickupRange = 3f;
    [SerializeField] private LayerMask pickupLayer;
    [SerializeField] private KeyCode pickupKey = KeyCode.E;

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private HorrorHUDManager hudManager;

    // Private variables
    private List<InventoryItem> inventorySlots = new List<InventoryItem>();
    private int currentSlot = -1; // -1 = nincs semmi kézben
    private GameObject currentItemObject;
    private PickupItem lookingAtItem;

    [System.Serializable]
    public class InventoryItem
    {
        public string itemName;
        public GameObject itemPrefab;
        public ItemType itemType;
    }

    public enum ItemType
    {
        Broom,
        Key,
        Other
    }

    void Start()
    {
        // Ha nincs itemHoldPosition, hozzuk létre a kamera alatt
        if (itemHoldPosition == null && playerCamera != null)
        {
            GameObject holder = new GameObject("ItemHolder");
            holder.transform.SetParent(playerCamera.transform);
            holder.transform.localPosition = new Vector3(0, 0, itemHoldDistance) + itemHoldOffset;
            holder.transform.localRotation = Quaternion.identity;
            itemHoldPosition = holder.transform;
        }

        // Inicializáljuk az inventory slot-okat
        for (int i = 0; i < maxSlots; i++)
        {
            inventorySlots.Add(null);
        }
    }

    void Update()
    {
        CheckForPickupItems();
        HandlePickup();
        HandleItemSwitching();
    }

    void CheckForPickupItems()
    {
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, pickupRange, pickupLayer))
        {
            PickupItem item = hit.collider.GetComponent<PickupItem>();

            if (item != null)
            {
                lookingAtItem = item;

                // HUD prompt megjelenítése
                if (hudManager != null)
                {
                    string prompt = $"[{pickupKey}] Felvesz: {item.itemName}";
                    hudManager.ShowInteractionPrompt(prompt);
                }

                return;
            }
        }

        // Ha nem nézünk semmire
        if (lookingAtItem != null)
        {
            lookingAtItem = null;
            if (hudManager != null)
            {
                hudManager.HideInteractionPrompt();
            }
        }
    }

    void HandlePickup()
    {
        if (Input.GetKeyDown(pickupKey) && lookingAtItem != null)
        {
            PickupItem(lookingAtItem);
        }
    }

    void PickupItem(PickupItem pickup)
    {
        // Keresünk szabad helyet
        int emptySlot = FindEmptySlot();

        if (emptySlot == -1)
        {
            Debug.Log("Inventory tele!");
            if (hudManager != null)
            {
                hudManager.ShowInteractionPrompt("Nincs hely az inventory-ban!");
            }
            return;
        }

        // Létrehozzuk az inventory item-et
        InventoryItem newItem = new InventoryItem
        {
            itemName = pickup.itemName,
            itemPrefab = pickup.itemPrefab,
            itemType = pickup.itemType
        };

        inventorySlots[emptySlot] = newItem;

        // Eltüntetjük a világból
        Destroy(pickup.gameObject);
        lookingAtItem = null;

        if (hudManager != null)
        {
            hudManager.HideInteractionPrompt();
        }

        Debug.Log($"{pickup.itemName} felvéve! Slot: {emptySlot + 1}");

        // Automatikusan equipeljük ha üres a kézünk
        if (currentSlot == -1)
        {
            EquipItem(emptySlot);
        }
    }

    void HandleItemSwitching()
    {
        // 1, 2, 3 billentyûk
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SelectSlot(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SelectSlot(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SelectSlot(2);
        }

        // Scroll wheel (opcionális)
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            int direction = scroll > 0 ? -1 : 1;
            int nextSlot = currentSlot + direction;

            // Wrap around
            if (nextSlot < -1) nextSlot = maxSlots - 1;
            if (nextSlot >= maxSlots) nextSlot = -1;

            SelectSlot(nextSlot);
        }

        // Q gomb - eldobás/visszarakás
        if (Input.GetKeyDown(KeyCode.Q) && currentSlot != -1)
        {
            UnequipCurrentItem();
        }
    }

    void SelectSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxSlots)
        {
            UnequipCurrentItem();
            return;
        }

        // Ha ugyanaz a slot, ne csináljunk semmit
        if (slotIndex == currentSlot) return;

        // Ha üres a slot
        if (inventorySlots[slotIndex] == null)
        {
            Debug.Log($"Slot {slotIndex + 1} üres!");
            return;
        }

        EquipItem(slotIndex);
    }

    void EquipItem(int slotIndex)
    {
        // Eltüntetjük a jelenlegi item-et
        if (currentItemObject != null)
        {
            Destroy(currentItemObject);
        }

        currentSlot = slotIndex;
        InventoryItem item = inventorySlots[slotIndex];

        // Létrehozzuk az új item-et a kamerához
        if (item != null && item.itemPrefab != null && itemHoldPosition != null)
        {
            currentItemObject = Instantiate(item.itemPrefab, itemHoldPosition);
            currentItemObject.transform.localPosition = Vector3.zero;
            currentItemObject.transform.localRotation = Quaternion.identity;

            // Eltávolítjuk a pickup scriptet és collider-t ha van
            PickupItem pickup = currentItemObject.GetComponent<PickupItem>();
            if (pickup != null) Destroy(pickup);

            Collider col = currentItemObject.GetComponent<Collider>();
            if (col != null) col.enabled = false;

            // Rigidbody eltávolítása
            Rigidbody rb = currentItemObject.GetComponent<Rigidbody>();
            if (rb != null) Destroy(rb);

            // Beállítjuk a megfelelõ layer-t (ne akadályozza a raycast-ot)
            SetLayerRecursively(currentItemObject, LayerMask.NameToLayer("Ignore Raycast"));

            Debug.Log($"{item.itemName} equipped! Slot: {slotIndex + 1}");
        }
    }

    void UnequipCurrentItem()
    {
        if (currentItemObject != null)
        {
            Destroy(currentItemObject);
        }

        currentSlot = -1;
        Debug.Log("Item visszatéve");
    }

    int FindEmptySlot()
    {
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (inventorySlots[i] == null)
            {
                return i;
            }
        }
        return -1;
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    // Publikus metódusok
    public bool HasItem(ItemType type)
    {
        foreach (var item in inventorySlots)
        {
            if (item != null && item.itemType == type)
            {
                return true;
            }
        }
        return false;
    }

    public InventoryItem GetCurrentItem()
    {
        if (currentSlot >= 0 && currentSlot < inventorySlots.Count)
        {
            return inventorySlots[currentSlot];
        }
        return null;
    }

    public void RemoveCurrentItem()
    {
        if (currentSlot >= 0 && currentSlot < inventorySlots.Count)
        {
            inventorySlots[currentSlot] = null;
            UnequipCurrentItem();
        }
    }
}