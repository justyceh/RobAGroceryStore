using UnityEngine;

// Briefly shows a "Backpack full!" message when you try to steal with no room left.
// Put this on the Canvas, not on the message itself: a hidden object's scripts stop
// running, so it couldn't show itself again.
public class BackpackFullUI : MonoBehaviour
{
    [Tooltip("The PlayerLoot on the Player. Drag the Player here.")]
    [SerializeField] private PlayerLoot playerLoot;

    [Tooltip("The message object to show, e.g. a text saying 'Backpack full!'.")]
    [SerializeField] private GameObject message;

    [Tooltip("How long the message stays on screen, in seconds.")]
    [SerializeField] private float showTime = 1.5f;

    private float hideTimer;

    private void OnEnable()
    {
        playerLoot.BackpackFull += ShowMessage;
    }

    private void OnDisable()
    {
        playerLoot.BackpackFull -= ShowMessage;
    }

    private void Start()
    {
        message.SetActive(false);
    }

    private void Update()
    {
        if (!message.activeSelf)
        {
            return;
        }

        hideTimer -= Time.deltaTime;
        if (hideTimer <= 0f)
        {
            message.SetActive(false);
        }
    }

    private void ShowMessage()
    {
        // Pressing E again while it's showing just restarts the timer.
        hideTimer = showTime;
        message.SetActive(true);
    }
}
