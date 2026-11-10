using UnityEngine;
using UnityEngine.SceneManagement;

// The main menu's buttons, and switching between its three panels: the menu itself
// (Play / Shop / Settings / Quit), the Shop, and Settings. Only one panel shows at a time,
// like conditionally rendering one "page" in a React app without changing the URL.
// Money and items aren't shown on the menu; they only appear in the Shop panel.
// The buttons call these methods through their On Click events, set up in the Inspector.
public class MainMenuUI : MonoBehaviour
{
    [Tooltip("The scene the Play button loads. It must be in File > Build Profiles > Scene List.")]
    [SerializeField] private string firstLevelScene = "Level_01";

    [Tooltip("The panel holding the Play / Shop / Settings / Quit buttons.")]
    [SerializeField] private GameObject menuPanel;

    [Tooltip("The Shop panel (the object with ShopScreenUI).")]
    [SerializeField] private GameObject shopPanel;

    [Tooltip("The Settings panel.")]
    [SerializeField] private GameObject settingsPanel;

    private void Start()
    {
        // Coming back from a level, the cursor could still be locked and time frozen, so reset both.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;

        ShowMenu();
    }

    // Called by the Play button.
    public void Play()
    {
        SceneManager.LoadScene(firstLevelScene);
    }

    // Called by the Shop button.
    public void OpenShop()
    {
        ShowOnly(shopPanel);
    }

    // Called by the Settings button.
    public void OpenSettings()
    {
        ShowOnly(settingsPanel);
    }

    // Called by the Back buttons in the Shop and Settings panels.
    public void ShowMenu()
    {
        ShowOnly(menuPanel);
    }

    private void ShowOnly(GameObject panel)
    {
        menuPanel.SetActive(panel == menuPanel);
        shopPanel.SetActive(panel == shopPanel);
        settingsPanel.SetActive(panel == settingsPanel);
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
