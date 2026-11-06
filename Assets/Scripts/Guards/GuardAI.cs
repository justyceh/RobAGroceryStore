using UnityEngine;

// The guard's "brain": it decides which behaviour is active right now.
// Each behaviour is its own component (GuardWander, GuardChase, GuardCatch), and switching
// state just turns the right ones on and the others off. Later states (Search, Stunned,
// Blinded...) slot in the same way.
//
// Stealth rule: guards ignore you until one sees you steal. That makes you Wanted, and from
// then on any guard who sees you starts chasing.
[RequireComponent(typeof(GuardWander), typeof(GuardChase), typeof(GuardCatch))]
[RequireComponent(typeof(GuardSight))]
public class GuardAI : MonoBehaviour
{
    public enum State
    {
        Wander,
        Chase
    }

    [Tooltip("What the guard does when the game starts.")]
    [SerializeField] private State startState = State.Wander;

    [Tooltip("The PlayerLoot on the Player, so this guard hears about every theft. Drag the Player here.")]
    [SerializeField] private PlayerLoot playerLoot;

    [Tooltip("The PlayerWanted on the Player. Drag the Player here.")]
    [SerializeField] private PlayerWanted playerWanted;

    public State CurrentState { get; private set; }

    private GuardWander wander;
    private GuardChase chase;
    private GuardCatch catcher;
    private GuardSight sight;

    private void Awake()
    {
        wander = GetComponent<GuardWander>();
        chase = GetComponent<GuardChase>();
        catcher = GetComponent<GuardCatch>();
        sight = GetComponent<GuardSight>();
    }

    private void OnEnable()
    {
        playerLoot.ItemStolen += OnItemStolen;
    }

    private void OnDisable()
    {
        playerLoot.ItemStolen -= OnItemStolen;
    }

    private void Update()
    {
        // A wandering guard keeps an eye out. Spotting a Wanted player starts a chase.
        // (Chasing never stops on its own yet; losing the player comes with vision cones.)
        if (CurrentState == State.Wander && playerWanted.IsWanted && sight.CanSeePlayer())
        {
            SetState(State.Chase);
        }
    }

    // Runs at the exact moment the player steals something, for every guard.
    private void OnItemStolen()
    {
        if (sight.CanSeePlayer())
        {
            // This guard is a witness: the player is now Wanted, and this guard reacts right away.
            playerWanted.MakeWanted();
            SetState(State.Chase);
        }
    }

    // Start (not Awake) so every component has finished its own setup before we switch them.
    private void Start()
    {
        SetState(startState);
    }

    public void SetState(State newState)
    {
        CurrentState = newState;

        wander.enabled = newState == State.Wander;
        chase.enabled = newState == State.Chase;

        // Only a chasing guard can catch you, so a wandering guard bumping into you doesn't end the game.
        catcher.enabled = newState == State.Chase;
    }

    // [ContextMenu] adds these to the component's ⋮ menu in the Inspector, so you can
    // test states by hand during Play mode before anything triggers them for real.
    [ContextMenu("Debug: Start Chasing")]
    private void DebugStartChasing()
    {
        SetState(State.Chase);
    }

    [ContextMenu("Debug: Start Wandering")]
    private void DebugStartWandering()
    {
        SetState(State.Wander);
    }
}
