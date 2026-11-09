using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// The main menu: shows your saved progress and has the Play and Quit buttons.
// The buttons call Play() and Quit() through their On Click events, set up in the Inspector.
public class MainMenuUI : MonoBehaviour
{
    [Tooltip("The scene the Play button loads. It must be in File > Build Profiles > Scene List.")]
    [SerializeField] private string firstLevelScene = "Level_01";

    [Tooltip("Text that shows your saved money and what's in your backpack.")]
    [SerializeField] private TMP_Text progressText;

    private void Start()
    {
        // Coming back from a level, the cursor could still be locked and time frozen, so reset both.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;

        // GameSession loaded the save file in its Awake, which always runs before this Start.
        GameSession session = GameSession.Instance;
        Backpack backpack = session.Backpack;
        progressText.text = $"Money: ${session.Money}\n" +
                            $"Backpack: {backpack.Items.Count}/{backpack.Capacity} items (worth ${backpack.TotalValue})";
    }

    // Called by the Play button.
    public void Play()
    {
        SceneManager.LoadScene(firstLevelScene);
    }

    // Called by the Quit button.
    public void Quit()
    {
        // Application.Quit only works in a built game; in the Editor it does nothing.
        // "#if UNITY_EDITOR" code only exists while running inside the Editor, so there we
        // stop Play mode instead, which is the closest equivalent.
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
