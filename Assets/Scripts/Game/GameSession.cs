using System.Collections.Generic;
using UnityEngine;

// Holds everything that should survive loading a new level, and loads/saves it to disk.
// Normally every object is destroyed when a scene loads (like a full page refresh wiping
// React state). DontDestroyOnLoad moves this object into a special scene that never unloads.
//
// Every scene has one (inside Core in levels, on its own in the menu). The first one to wake
// up becomes the real session; copies in later scenes see that one already exists and delete
// themselves. Other scripts reach it with GameSession.Instance (a "singleton": exactly one, global).
public class GameSession : MonoBehaviour
{
    [Tooltip("How many slots the hotbar (the bar at the bottom of the screen) has.")]
    [SerializeField] private int hotbarSize = 8;

    [Tooltip("How many slots the backpack (the extra storage opened with Tab) has.")]
    [SerializeField] private int backpackSize = 16;

    [Tooltip("The ItemDatabase asset listing every item, used to turn saved item IDs back into items.")]
    [SerializeField] private ItemDatabase itemDatabase;

    public static GameSession Instance { get; private set; }

    // The 8 slots you carry and use during a level.
    public Inventory Hotbar { get; private set; }

    // Extra storage. Items here can be carried and sold, but must be moved to the hotbar to be used.
    public Inventory Backpack { get; private set; }

    // Money earned at the shop. Saved with the items. Only changed through AddMoney / TrySpendMoney.
    public int Money { get; private set; }

    // Awake runs before any Start, so scripts should use GameSession.Instance from Start onward, not Awake.
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // A session already exists from an earlier scene. Keep that one and its items.
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Hotbar = new Inventory(hotbarSize);
        Backpack = new Inventory(backpackSize);
        LoadGame();

        // DontDestroyOnLoad only works on top-level objects, so detach from any parent first.
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // Called by the shop when you sell something.
    public void AddMoney(int amount)
    {
        if (amount > 0)
        {
            Money += amount;
        }
    }

    // Called by the shop when you buy something. Returns false (and spends nothing)
    // if you can't afford it, so money can never go below 0.
    public bool TrySpendMoney(int amount)
    {
        if (amount < 0 || amount > Money)
        {
            return false;
        }

        Money -= amount;
        return true;
    }

    // Saving only happens at safe moments (when a level ends, or after a shop purchase or sale),
    // never mid-level. Otherwise you could steal, quit before getting caught, and keep the loot.
    public void SaveGame()
    {
        SaveData data = new SaveData();
        data.money = Money;
        WriteSlots(Hotbar, data.hotbar);
        WriteSlots(Backpack, data.backpack);

        SaveSystem.Save(data);
        Debug.Log($"Game saved to {SaveSystem.SavePath}");
    }

    private void LoadGame()
    {
        SaveData data = SaveSystem.Load();
        if (data == null)
        {
            Debug.Log("No save file yet, starting with empty pockets.");
            return;
        }

        if (itemDatabase == null)
        {
            Debug.LogError("GameSession has no Item Database set, so saved items can't be loaded.");
            return;
        }

        Money = data.money;
        ReadSlots(data.hotbar, Hotbar);
        ReadSlots(data.backpack, Backpack);

        // Old format from before stacking: just a list of item IDs. Add them one by one so they stack.
        foreach (string id in data.backpackItemIds)
        {
            ItemData item = FindItem(id);
            if (item != null && !Hotbar.TryAdd(item) && !Backpack.TryAdd(item))
            {
                Debug.LogWarning("Saved items don't all fit, the rest were dropped.");
                break;
            }
        }

        Debug.Log($"Loaded save: {Hotbar.TotalItemCount + Backpack.TotalItemCount} items, ${Money}.");
    }

    // Copies every non-empty slot of an inventory into the save list.
    private static void WriteSlots(Inventory inventory, List<SavedSlot> savedSlots)
    {
        for (int i = 0; i < inventory.SlotCount; i++)
        {
            InventorySlot slot = inventory.GetSlot(i);
            if (!slot.IsEmpty)
            {
                savedSlots.Add(new SavedSlot { index = i, itemId = slot.Item.Id, count = slot.Count });
            }
        }
    }

    // Puts each saved stack back into the exact slot it was saved from.
    private void ReadSlots(List<SavedSlot> savedSlots, Inventory inventory)
    {
        foreach (SavedSlot saved in savedSlots)
        {
            ItemData item = FindItem(saved.itemId);
            if (item != null && saved.index >= 0 && saved.index < inventory.SlotCount)
            {
                inventory.SetSlot(saved.index, item, Mathf.Min(saved.count, item.MaxStack));
            }
        }
    }

    private ItemData FindItem(string id)
    {
        ItemData item = itemDatabase.FindById(id);
        if (item == null)
        {
            // E.g. the item asset was renamed or removed from the database.
            Debug.LogWarning($"Saved item '{id}' isn't in the Item Database, skipping it.");
        }
        return item;
    }

    // Right-click the component's ⋮ menu to use these while testing. They work outside Play mode too.
    [ContextMenu("Debug: Delete Save File")]
    private void DebugDeleteSave()
    {
        SaveSystem.Delete();
        Debug.Log($"Deleted save file at {SaveSystem.SavePath}");
    }

    [ContextMenu("Debug: Log Save File Location")]
    private void DebugLogSaveLocation()
    {
        Debug.Log($"Save file location: {SaveSystem.SavePath}");
    }
}
