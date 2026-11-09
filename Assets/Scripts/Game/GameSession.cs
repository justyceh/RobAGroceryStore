using UnityEngine;

// Holds everything that should survive loading a new level, and loads/saves it to disk.
// Normally every object is destroyed when a scene loads (like a full page refresh wiping
// React state). DontDestroyOnLoad moves this object into a special scene that never unloads.
//
// It lives in the Core prefab, so every level has one. The first one to wake up becomes the
// real session; the copies in later levels see that one already exists and delete themselves.
// Other scripts reach it with GameSession.Instance (a "singleton": exactly one, global).
public class GameSession : MonoBehaviour
{
    [Tooltip("How many items the backpack can hold.")]
    [SerializeField] private int backpackSize = 8;

    [Tooltip("The ItemDatabase asset listing every item, used to turn saved item IDs back into items.")]
    [SerializeField] private ItemDatabase itemDatabase;

    public static GameSession Instance { get; private set; }

    public Backpack Backpack { get; private set; }

    // Money earned at the shop. Saved with the backpack; the shop will change it.
    public int Money { get; private set; }

    // Awake runs before any Start, so scripts should use GameSession.Instance from Start onward, not Awake.
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // A session already exists from an earlier level. Keep that one and its backpack.
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Backpack = new Backpack(backpackSize);
        LoadGame();

        // DontDestroyOnLoad only works on top-level objects, so detach from the Core parent first.
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

    // Saving only happens at safe moments (after escaping a level), never mid-level.
    // Otherwise you could steal, quit before getting caught, and keep the loot.
    public void SaveGame()
    {
        SaveData data = new SaveData();
        data.money = Money;
        foreach (ItemData item in Backpack.Items)
        {
            data.backpackItemIds.Add(item.Id);
        }

        SaveSystem.Save(data);
        Debug.Log($"Game saved to {SaveSystem.SavePath}");
    }

    private void LoadGame()
    {
        SaveData data = SaveSystem.Load();
        if (data == null)
        {
            Debug.Log("No save file yet, starting with an empty backpack.");
            return;
        }

        if (itemDatabase == null)
        {
            Debug.LogError("GameSession has no Item Database set, so saved items can't be loaded.");
            return;
        }

        Money = data.money;
        foreach (string id in data.backpackItemIds)
        {
            ItemData item = itemDatabase.FindById(id);
            if (item == null)
            {
                // E.g. the item asset was renamed or removed from the database.
                Debug.LogWarning($"Saved item '{id}' isn't in the Item Database, skipping it.");
                continue;
            }
            if (!Backpack.TryAdd(item))
            {
                Debug.LogWarning("Saved backpack has more items than fit, the rest were dropped.");
                break;
            }
        }

        Debug.Log($"Loaded save: {Backpack.Items.Count} items, ${Money}.");
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
