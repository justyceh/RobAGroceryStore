using UnityEngine;

// Shows the WANTED indicator on screen once the player becomes Wanted.
// Like LootCounterUI, it listens for an event instead of checking every frame.
public class WantedUI : MonoBehaviour
{
    [Tooltip("The PlayerWanted component on the Player. Drag the Player object here.")]
    [SerializeField] private PlayerWanted playerWanted;

    [Tooltip("The WANTED indicator object to show. It's hidden automatically when the game starts.")]
    [SerializeField] private GameObject indicator;

    private void OnEnable()
    {
        playerWanted.BecameWanted += ShowIndicator;
    }

    private void OnDisable()
    {
        playerWanted.BecameWanted -= ShowIndicator;
    }

    private void Start()
    {
        indicator.SetActive(playerWanted.IsWanted);
    }

    private void ShowIndicator()
    {
        indicator.SetActive(true);
    }
}
