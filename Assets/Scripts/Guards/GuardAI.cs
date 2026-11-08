using System.Collections.Generic;
using UnityEngine;

// The guard's "brain": it decides which behaviour is active right now.
// Each behaviour is its own component (GuardWander, GuardChase, GuardSearch, GuardCatch), and
// switching state just turns the right ones on and the others off. Later states (Stunned,
// Blinded...) slot in the same way.
//
// Stealth rule: guards ignore you until one sees you steal. That makes you Wanted, and from
// then on any guard who sees you starts chasing. A guard who witnesses a theft also radios
// the other guards nearby, who come to search the spot.
//
// GuardAI also finds the Player and GameManager once and hands them to the other guard
// scripts, so a guard prefab can be dropped into the scene without wiring anything up.
//
//   Wander --(sees Wanted player)--> Chase --(loses sight for a while)--> Search
//     ^                                ^                                    |
//     |                                +-------(sees Wanted player)---------+
//     +-----------------------(search finished)-----------------------------+
[RequireComponent(typeof(GuardWander), typeof(GuardChase), typeof(GuardCatch))]
[RequireComponent(typeof(GuardSight), typeof(GuardSearch))]
public class GuardAI : MonoBehaviour
{
    public enum State
    {
        Wander,
        Chase,
        Search
    }

    [Tooltip("What the guard does when the game starts.")]
    [SerializeField] private State startState = State.Wander;

    [Tooltip("Seconds the guard keeps chasing after losing sight of you before it gives up and searches. Lets it follow you around a corner.")]
    [SerializeField] private float loseSightTime = 2f;

    [Tooltip("When this guard witnesses a theft, other guards within this many meters are radioed to search the spot.")]
    [SerializeField] private float radioRange = 25f;

    public State CurrentState { get; private set; }

    // Every active guard in the scene. "static" means one shared list for the whole GuardAI class,
    // not one per guard, like a module-level variable in JS. Guards add and remove themselves.
    private static readonly List<GuardAI> allGuards = new List<GuardAI>();

    private GuardWander wander;
    private GuardChase chase;
    private GuardSearch search;
    private GuardCatch catcher;
    private GuardSight sight;

    private Transform player;
    private PlayerLoot playerLoot;
    private PlayerWanted playerWanted;

    private Vector3 lastKnownPosition;
    private float timeSinceSeen;

    private void Awake()
    {
        wander = GetComponent<GuardWander>();
        chase = GetComponent<GuardChase>();
        search = GetComponent<GuardSearch>();
        catcher = GetComponent<GuardCatch>();
        sight = GetComponent<GuardSight>();

        // Look these up once here, never in Update. FindWithTag searches the scene for the object
        // tagged "Player"; FindAnyObjectByType finds the one GameManager.
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject == null)
        {
            Debug.LogError("GuardAI: no object is tagged 'Player'. Select the Player and set its Tag to Player.");
            enabled = false;
            return;
        }
        player = playerObject.transform;
        playerLoot = playerObject.GetComponent<PlayerLoot>();
        playerWanted = playerObject.GetComponent<PlayerWanted>();
        GameManager gameManager = FindAnyObjectByType<GameManager>();

        // Hand the references to the scripts that need them.
        chase.Init(player);
        sight.Init(player);
        catcher.Init(player, gameManager);
    }

    private void OnEnable()
    {
        playerLoot.ItemStolen += OnItemStolen;
        allGuards.Add(this);
    }

    private void OnDisable()
    {
        playerLoot.ItemStolen -= OnItemStolen;
        allGuards.Remove(this);
    }

    // Start (not Awake) so every component has finished its own setup before we switch them.
    private void Start()
    {
        SetState(startState);
    }

    private void Update()
    {
        // A switch on the current state, like a reducer: each state only checks the
        // events that can move it to another state.
        switch (CurrentState)
        {
            case State.Wander:
                if (playerWanted.IsWanted && sight.CanSeePlayer())
                {
                    StartChase();
                }
                break;

            case State.Chase:
                UpdateChase();
                break;

            case State.Search:
                if (playerWanted.IsWanted && sight.CanSeePlayer())
                {
                    StartChase();
                }
                else if (search.IsFinished)
                {
                    SetState(State.Wander);
                }
                break;
        }
    }

    private void UpdateChase()
    {
        if (sight.CanSeePlayer())
        {
            // Still in view: remember where they are and reset the "lost them" timer.
            lastKnownPosition = player.position;
            timeSinceSeen = 0f;
            return;
        }

        timeSinceSeen += Time.deltaTime;
        if (timeSinceSeen >= loseSightTime)
        {
            search.SetSearchPoint(lastKnownPosition);
            SetState(State.Search);
        }
    }

    // Runs at the exact moment the player steals something, for every guard.
    private void OnItemStolen()
    {
        if (sight.CanSeePlayer())
        {
            // This guard is a witness: the player is now Wanted, and this guard reacts right away.
            playerWanted.MakeWanted();
            StartChase();
            RadioOtherGuards(player.position);
        }
    }

    private void RadioOtherGuards(Vector3 theftPosition)
    {
        foreach (GuardAI guard in allGuards)
        {
            if (guard != this && Vector3.Distance(transform.position, guard.transform.position) <= radioRange)
            {
                guard.HearRadio(theftPosition);
            }
        }
    }

    // Another guard called in a theft. Head over and search, unless already chasing someone.
    // Once there, the normal rules apply: seeing the Wanted player starts a chase.
    public void HearRadio(Vector3 position)
    {
        if (CurrentState == State.Chase)
        {
            return;
        }

        search.SetSearchPoint(position);

        // Re-enable search so it restarts with the new point, even if this guard was already searching.
        search.enabled = false;
        SetState(State.Search);
    }

    private void StartChase()
    {
        lastKnownPosition = player.position;
        timeSinceSeen = 0f;
        SetState(State.Chase);
    }

    public void SetState(State newState)
    {
        CurrentState = newState;

        wander.enabled = newState == State.Wander;
        chase.enabled = newState == State.Chase;
        search.enabled = newState == State.Search;

        // Only a chasing guard can catch you, so bumping into a wandering or searching guard doesn't end the game.
        catcher.enabled = newState == State.Chase;
    }

    // [ContextMenu] adds these to the component's ⋮ menu in the Inspector, so you can
    // test states by hand during Play mode before anything triggers them for real.
    [ContextMenu("Debug: Start Chasing")]
    private void DebugStartChasing()
    {
        StartChase();
    }

    [ContextMenu("Debug: Start Wandering")]
    private void DebugStartWandering()
    {
        SetState(State.Wander);
    }
}
