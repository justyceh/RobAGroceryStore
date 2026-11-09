using UnityEngine;
using UnityEngine.SceneManagement;

// The main menu's buttons. Money and items aren't shown here; they'll live in the Shop screen.
// The buttons call Play() and Quit() through their On Click events, set up in the Inspector.
public class MainMenuUI : MonoBehaviour
{
    [Tooltip("The scene the Play button loads. It must be in File > Build Profiles > Scene List.")]
    [SerializeField] private string firstLevelScene = "Level_01";

    private void Start()
    {
        // Coming back from a level, the cursor could still be locked and time frozen, so reset both.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;
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
