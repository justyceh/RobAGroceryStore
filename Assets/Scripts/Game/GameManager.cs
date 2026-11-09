using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// Decides when the game is over. Other scripts report what happened (GuardCatch calls
// PlayerCaught, ExitZone calls PlayerEscaped) and this script freezes the game,
// shows the result, and restarts or moves on when a button is clicked.
// Levels are separate scenes; their order is the scene list in File > Build Profiles.
public class GameManager : MonoBehaviour
{
    [Tooltip("The panel shown when the game ends. It's hidden automatically when the game starts.")]
    [SerializeField] private GameObject gameOverPanel;

    [Tooltip("The text on the panel that says whether you won or lost.")]
    [SerializeField] private TMP_Text resultText;

    [Tooltip("The Next Level button on the panel. Only shown after a win, and only if another level comes after this one in Build Profiles.")]
    [SerializeField] private GameObject nextLevelButton;

    [Tooltip("The Player. All of its scripts are switched off when the game ends so you can't keep playing behind the message.")]
    [SerializeField] private GameObject player;

    [Tooltip("Message when you escape. {0} is replaced with how many items you stole.")]
    [SerializeField] private string winMessage = "You escaped with {0} loot!";

    [Tooltip("Message when a guard catches you.")]
    [SerializeField] private string loseMessage = "Caught by security!";

    public bool IsGameOver { get; private set; }

    private PlayerLoot playerLoot;

    private void Awake()
    {
        playerLoot = player.GetComponent<PlayerLoot>();
    }

    private void Start()
    {
        gameOverPanel.SetActive(false);

        // timeScale is global and survives a scene reload, so make sure time is running.
        Time.timeScale = 1f;
    }

    public void PlayerCaught()
    {
        // Guards keep calling this every frame while touching you, even after a win,
        // so bail out early. Otherwise you could lose your loot after already escaping.
        if (IsGameOver)
        {
            return;
        }

        // The penalty for getting caught: only what you grabbed in this level is lost.
        playerLoot.LoseThisLevelsLoot();
        EndGame(loseMessage, false);
    }

    public void PlayerEscaped(int lootCount)
    {
        EndGame(string.Format(winMessage, lootCount), true);
    }

    private void EndGame(string message, bool won)
    {
        // A guard keeps "catching" you every frame while touching you, so only end the game once.
        if (IsGameOver)
        {
            return;
        }
        IsGameOver = true;

        // Turning off the player's scripts stops movement, looking, stealing and camera toggling.
        // PlayerLook's OnDisable also unlocks the cursor, which you need to click Play Again.
        // GetComponents is fine here because this runs once, not every frame.
        foreach (MonoBehaviour script in player.GetComponents<MonoBehaviour>())
        {
            script.enabled = false;
        }

        // timeScale 0 freezes everything that uses Time.deltaTime, including the guard's NavMeshAgent.
        // UI buttons still work because they don't depend on game time.
        Time.timeScale = 0f;

        resultText.text = message;
        nextLevelButton.SetActive(won && HasNextLevel());
        gameOverPanel.SetActive(true);
    }

    private bool HasNextLevel()
    {
        // buildIndex is this scene's position in the Build Profiles list (0 = first).
        return SceneManager.GetActiveScene().buildIndex + 1 < SceneManager.sceneCountInBuildSettings;
    }

    // Called by the Next Level button's On Click event.
    public void LoadNextLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // Called by the Play Again button's On Click event (set up in the Inspector, not in code).
    public void Restart()
    {
        Time.timeScale = 1f;

        // Reloading the scene resets everything to how it was saved, like refreshing a web page.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
