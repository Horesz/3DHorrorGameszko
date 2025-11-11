using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class REStyleInventory : MonoBehaviour
{
    [Header("Inventory UI")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform itemGridParent;
    [SerializeField] private GameObject itemSlotPrefab;
    [SerializeField] private Text itemNameText;
    [SerializeField] private Text itemDescriptionText;
    [SerializeField] private Image itemPreviewImage;

    [Header("Documents Panel")]
    [SerializeField] private GameObject documentPanel;
    [SerializeField] private Text documentTitleText;
    [SerializeField] private Text documentContentText;
    [SerializeField] private Button closeDocumentButton;

    [Header("Settings")]
    [SerializeField] private int maxInventorySlots = 8;
    [SerializeField] private KeyCode inventoryKey = KeyCode.Tab;
    [SerializeField] private KeyCode useItemKey = KeyCode.E;
    [SerializeField] private KeyCode examineKey = KeyCode.Space;

    [Header("Audio")]
    [SerializeField] private AudioSource inventorySound;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    [SerializeField] private AudioClip selectSound;
    [SerializeField] private AudioClip pickupSound;

    // Private variables
    private List<InventoryItem> items = new List<InventoryItem>();
    private List<GameObject> itemSlots = new List<GameObject>();
    private int selectedSlotIndex = -1;
    private bool isInventoryOpen = false;
    private HorrorPlayerController playerController;

    [System.Serializable]
    public class InventoryItem
    {
        public string itemName;
        public string description;
        public ItemCategory category;
        public Sprite icon;
        public GameObject worldPrefab;
        public bool isUsable;
        public bool isExaminable;
        public string documentContent; // For papers/notes

        // For keys
        public string keyID; // Which door it opens
    }

    public enum ItemCategory
    {
        Key,
        Document,
        Tool,
        Medicine,
        Flashlight,
        Other
    }

    void Start()
    {
        // Hide inventory at start
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);

        if (documentPanel != null)
            documentPanel.SetActive(false);

        // Setup close button
        if (closeDocumentButton != null)
            closeDocumentButton.onClick.AddListener(CloseDocument);

        // Find player
        playerController = FindObjectOfType<HorrorPlayerController>();

        // Create inventory slots
        CreateInventorySlots();
    }

    void Update()
    {
        // Toggle inventory
        if (Input.GetKeyDown(inventoryKey))
        {
            ToggleInventory();
        }

        // Inventory controls
        if (isInventoryOpen)
        {
            HandleInventoryInput();
        }
    }

    void CreateInventorySlots()
    {
        if (itemGridParent == null || itemSlotPrefab == null) return;

        for (int i = 0; i < maxInventorySlots; i++)
        {
            GameObject slot = Instantiate(itemSlotPrefab, itemGridParent);
            itemSlots.Add(slot);

            // Setup slot button
            Button slotButton = slot.GetComponent<Button>();
            if (slotButton != null)
            {
                int index = i; // Capture for lambda
                slotButton.onClick.AddListener(() => SelectSlot(index));
            }

            // Hide initially
            Image icon = slot.transform.Find("Icon")?.GetComponent<Image>();
            if (icon != null)
                icon.enabled = false;
        }
    }

    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;

        if (inventoryPanel != null)
            inventoryPanel.SetActive(isInventoryOpen);

        // Pause game / lock cursor
        if (isInventoryOpen)
        {
            Time.timeScale = 0f; // Pause
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (inventorySound != null && openSound != null)
                inventorySound.PlayOneShot(openSound);

            RefreshInventoryUI();
        }
        else
        {
            Time.timeScale = 1f; // Resume
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (inventorySound != null && closeSound != null)
                inventorySound.PlayOneShot(closeSound);

            CloseDocument();
        }
    }

    void HandleInventoryInput()
    {
        // Use item
        if (Input.GetKeyDown(useItemKey) && selectedSlotIndex >= 0)
        {
            UseSelectedItem();
        }

        // Examine item
        if (Input.GetKeyDown(examineKey) && selectedSlotIndex >= 0)
        {
            ExamineSelectedItem();
        }

        // Arrow key navigation
        if (Input.GetKeyDown(KeyCode.UpArrow))
            NavigateSlots(-4);
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            NavigateSlots(4);
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            NavigateSlots(-1);
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            NavigateSlots(1);
    }

    void NavigateSlots(int direction)
    {
        if (items.Count == 0) return;

        int newIndex = selectedSlotIndex + direction;
        newIndex = Mathf.Clamp(newIndex, 0, items.Count - 1);

        SelectSlot(newIndex);
    }

    void SelectSlot(int index)
    {
        if (index < 0 || index >= items.Count) return;

        selectedSlotIndex = index;

        // Play sound
        if (inventorySound != null && selectSound != null)
            inventorySound.PlayOneShot(selectSound);

        // Update UI
        UpdateSelectionUI();
        DisplayItemInfo(items[index]);
    }

    void UpdateSelectionUI()
    {
        for (int i = 0; i < itemSlots.Count; i++)
        {
            Image background = itemSlots[i].GetComponent<Image>();
            if (background != null)
            {
                background.color = (i == selectedSlotIndex) ?
                    new Color(1f, 1f, 0f, 0.5f) : // Yellow highlight
                    new Color(1f, 1f, 1f, 0.3f);  // Normal
            }
        }
    }

    void DisplayItemInfo(InventoryItem item)
    {
        if (itemNameText != null)
            itemNameText.text = item.itemName;

        if (itemDescriptionText != null)
            itemDescriptionText.text = item.description;

        if (itemPreviewImage != null && item.icon != null)
        {
            itemPreviewImage.sprite = item.icon;
            itemPreviewImage.enabled = true;
        }
    }

    public bool AddItem(InventoryItem newItem)
    {
        if (items.Count >= maxInventorySlots)
        {
            Debug.Log("Inventory tele!");
            return false;
        }

        items.Add(newItem);

        // Play pickup sound
        if (inventorySound != null && pickupSound != null)
            inventorySound.PlayOneShot(pickupSound);

        RefreshInventoryUI();
        return true;
    }

    void RefreshInventoryUI()
    {
        // Clear all slots
        foreach (var slot in itemSlots)
        {
            Image icon = slot.transform.Find("Icon")?.GetComponent<Image>();
            if (icon != null)
                icon.enabled = false;
        }

        // Fill slots with items
        for (int i = 0; i < items.Count; i++)
        {
            if (i >= itemSlots.Count) break;

            Image icon = itemSlots[i].transform.Find("Icon")?.GetComponent<Image>();
            if (icon != null && items[i].icon != null)
            {
                icon.sprite = items[i].icon;
                icon.enabled = true;
            }
        }

        // Select first item if nothing selected
        if (selectedSlotIndex < 0 && items.Count > 0)
        {
            SelectSlot(0);
        }
    }

    void UseSelectedItem()
    {
        if (selectedSlotIndex < 0 || selectedSlotIndex >= items.Count) return;

        InventoryItem item = items[selectedSlotIndex];

        if (!item.isUsable)
        {
            Debug.Log($"{item.itemName} nem használható!");
            return;
        }

        switch (item.category)
        {
            case ItemCategory.Medicine:
                UseMedicine(item);
                break;

            case ItemCategory.Flashlight:
                EquipFlashlight(item);
                break;

            case ItemCategory.Key:
                Debug.Log($"{item.itemName} használatához menj egy ajtóhoz!");
                break;

            default:
                Debug.Log($"{item.itemName} használva!");
                break;
        }
    }

    void ExamineSelectedItem()
    {
        if (selectedSlotIndex < 0 || selectedSlotIndex >= items.Count) return;

        InventoryItem item = items[selectedSlotIndex];

        if (item.category == ItemCategory.Document && !string.IsNullOrEmpty(item.documentContent))
        {
            OpenDocument(item);
        }
        else
        {
            Debug.Log($"Examining: {item.itemName} - {item.description}");
        }
    }

    void OpenDocument(InventoryItem item)
    {
        if (documentPanel == null) return;

        documentPanel.SetActive(true);

        if (documentTitleText != null)
            documentTitleText.text = item.itemName;

        if (documentContentText != null)
            documentContentText.text = item.documentContent;
    }

    void CloseDocument()
    {
        if (documentPanel != null)
            documentPanel.SetActive(false);
    }

    void UseMedicine(InventoryItem item)
    {
        Debug.Log($"{item.itemName} használva! +Health");

        // TODO: Heal player
        // if (playerController != null)
        //     playerController.Heal(50f);

        // Remove item
        RemoveItem(selectedSlotIndex);
    }

    void EquipFlashlight(InventoryItem item)
    {
        Debug.Log("Zseblámpa felszerelve!");
        // Flashlight is permanent, don't remove
    }

    public void RemoveItem(int index)
    {
        if (index < 0 || index >= items.Count) return;

        items.RemoveAt(index);
        selectedSlotIndex = -1;

        RefreshInventoryUI();
    }

    public bool HasItem(string itemName)
    {
        return items.Exists(item => item.itemName == itemName);
    }

    public bool HasKey(string keyID)
    {
        return items.Exists(item => item.category == ItemCategory.Key && item.keyID == keyID);
    }

    public InventoryItem GetItem(string itemName)
    {
        return items.Find(item => item.itemName == itemName);
    }
}