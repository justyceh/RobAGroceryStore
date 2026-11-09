using UnityEngine;

// Holds everything that should survive loading a new level: for now, the backpack.
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

    public static GameSession Instance { get; private set; }

    public Backpack Backpack { get; private set; }

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
}
